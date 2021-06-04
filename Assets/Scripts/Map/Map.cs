using System.Collections.Generic;
using Fluid.AI;
using UnityEngine;

namespace Fluid
{
    public class Map : MonoBehaviour
    {
        public static Map Instance { get; private set; }

        [SerializeField] private TileDB _tileDb;
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private MapLayer _mapLayerPrefab;
        [SerializeField] private MapLoaderType _mapLoaderType = MapLoaderType.PNG;
        [SerializeField] private Character _characterPrefab;
        [SerializeField] private CameraFollower _cameraFollower;

        private List<Character> _characters = new List<Character>();
        private List<Interactible> _interactible = new List<Interactible>();
        private List<MapLayer> _mapLayers = new List<MapLayer>();

        private Pathfinder _pathfinder = new Pathfinder();

        private MapLayer _shadowLayer;
        private int _currentLayer = 0;

        private int _width;
        private int _height;

        private TileDbEntry _grass;
        private TileDbEntry _dirt;
        private TileDbEntry _stone;

        private TileDbEntry _plantItem;

        private TileDbEntry _wall;
        private TileDbEntry _wallDecorated;
        private TileDbEntry _floor;

        public enum MapLoaderType { PNG, REX }
        public static int MoveDownCost = 0;
        public static int MoveForwardCost = 1;
        public static int MoveUpCost = 3;

        public int CurrentLayer => _currentLayer;
        public int Width => _width;
        public int Height => _height;

        public bool FindPath(int fromLayer, int fromX, int fromY, int toLayer, int toX, int toY, ref Stack<Tile> path)
        {
            return _pathfinder.FindPath(fromLayer, fromX, fromY, toLayer, toX, toY, ref path);
        }

        public bool FindPath(int fromLayer, int fromX, int fromY, Tile to, ref Stack<Tile> path)
        {
            return _pathfinder.FindPath(fromLayer, fromX, fromY, to.Layer, to.Pos.x, to.Pos.y, ref path);
        }

        public bool FindPath(Tile from, Tile to, ref Stack<Tile> path)
        {
            return _pathfinder.FindPath(from, to, ref path);
        }

        public void Focus(int x, int y, int h, bool update)
        {
            _cameraFollower.transform.position = new Vector3(x, y, 0);
            _currentLayer = h;

            if (update)
            {
                UpdateActiveLayer();
            }

            /*foreach (var c in PlayerController.Instance.Fortress.Characters)
            {
                if (c.Pos.x == x && c.Pos.y == y && c.Layer == h)
                {
                    c.GiveJobOrder(Fortress.Job.None);
                    break;
                }
            }*/
        }

        public void SpawnPlayerGroup()
        {
            var x = Random.Range(0, _width);
            var y = Random.Range(0, _height);
            var h = FindTopLayer(x, y);

            PlayerController.Instance.Fortress.Spawn(x, y);

            Focus(x, y, h, update:false);
        }

        public bool TryAttack(int layer, Vector2 from, Vector2 target)
        {
            return TryAttack(layer, (int) from.x, (int) from.y, (int) target.x, (int) target.y);
        }

        public bool TryAttack(int layer, int fromX, int fromY, int targetX, int targetY)
        {
            var tile = _mapLayers[layer][targetX, targetY];
            if (tile != null && tile.Job != Fortress.Job.None)
            {
                switch (tile.Job)
                {
                    case Fortress.Job.Build:
                        return TryBuild(tile);
                    case Fortress.Job.Dig:
                        return TryDig(tile);
                    case Fortress.Job.Gather:
                        return TryGather(tile);
                    case Fortress.Job.Decorate:
                        return TryDecorate(tile);
                }
            }

            return false;
        }

        public bool TryDecorate(Tile tile)
        {
            if (IsWalkable(tile))
            {
                tile.SetDecoration(_floor, _stone);
            }
            else if (tile.BuildingBlock != null)
            {
                tile.SetDecoration(_wallDecorated, _stone);
            }

            PlayerController.Instance.Fortress.CompleteJob(tile);
            return true;
        }

        public bool TryBuild(Tile tile)
        {
            if (tile.Item != null || tile.BuildingBlock != null)
            {
                return false;
            }

            tile.SetBuildingBlock(_wall, _stone);

            PlayerController.Instance.Fortress.CompleteJob(tile);
            return true;
        }

        public bool TryGather(Tile tile)
        {
            if (tile.Item == null)
            {
                return false;
            }

            tile.SetItem(null);

            PlayerController.Instance.Fortress.CompleteJob(tile);
            return true;
        }

        public bool TryDig(Tile tile)
        {
            // If there's a building block on this tile, we tear it down!
            if (tile.BuildingBlock != null)
            {
                tile.SetBuildingBlock(null, _stone);
                return true;
            }

            int layer = tile.Layer;

            // If we're at the highest layer, we can't dig. We're at the top of the world!
            if (layer >= _mapLayers.Count - 1)
            {
                return false;
            }

            var targetTopLayer = FindTopLayer(tile.Pos.x, tile.Pos.y);

            // We can't attack something that's at a lower layer than us
            if (targetTopLayer < layer)
            {
                return false;
            }

            // We can't attack something that's at the same layer as us
            if (targetTopLayer == layer)
            {
                return false;
            }

            var targetTile = FindTileAbove(layer, tile.Pos.x, tile.Pos.y);
            if (targetTile != null)
            {
                if (tile.Type == _grass.Name)
                {
                    tile.Init(_dirt, layer);
                }

                Destroy(targetTile.gameObject);
                _mapLayers[layer + 1][tile.Pos.x, tile.Pos.y] = null;

                UpdateShadows();
            }

            PlayerController.Instance.Fortress.CompleteJob(tile);
            return true;
        }

        public Sprite GetJobSprite(Fortress.Job job)
        {
            switch (job)
            {
                case Fortress.Job.None:
                default:
                    return null;
                case Fortress.Job.Build:
                    return _tileDb.Build;
                case Fortress.Job.Dig:
                    return _tileDb.Dig;
                case Fortress.Job.Gather:
                    return _tileDb.Gather;
                case Fortress.Job.Decorate:
                    return _tileDb.Decorate;
            }
        }

        public Sprite GetTransitionSprite(int cost)
        {
            if (cost == Map.MoveDownCost)
            {
                return _tileDb.Down;
            }

            if (cost == Map.MoveUpCost)
            {
                return _tileDb.Up;
            }

            if (cost < 0)
            {
                return _tileDb.Blocked;
            }

            return null;
        }

        public void IncrementLayer()
        {
            if (_currentLayer >= _mapLayers.Count - 1)
            {
                return;
            }

            ChangeLayer(_currentLayer + 1);
        }

        public void DecrementLayer()
        {
            if (_currentLayer <= 0)
            {
                return;
            }

            ChangeLayer(_currentLayer - 1);
        }

        public void ChangeLayer(int layer)
        {
            _currentLayer = layer;
            UpdateActiveLayer();
        }

        public bool IsValid(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
            {
                return false;
            }

            return true;
        }

        public Tile GetTile(int x, int y)
        {
            return GetTile(_currentLayer, x, y);
        }

        public Tile GetTile(int layer, int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
            {
                return null;
            }

            return _mapLayers[layer][x, y];
        }

        public bool IsWalkable(Tile tile)
        {
            return IsWalkable(tile.Layer, tile.Pos.x, tile.Pos.y);
        }

        public bool IsWalkable(int layer, int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
            {
                return false;
            }

            var tile = _mapLayers[layer][x, y];
            if (tile != null)
            {
                if (tile.BuildingBlock != null)
                {
                    return false;
                }
            }

            var tileAbove = FindTileAbove(layer, x, y);
            if (tileAbove != null)
            {
                return false;
            }

            return true;
        }

        public bool IsInShadow(Tile tile)
        {
            return IsInShadow(tile.Pos.x, tile.Pos.y);
        }

        public bool IsInShadow(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
            {
                return false;
            }

            return _shadowLayer[x, y].gameObject.activeSelf;
        }

        public Tile FindTileAbove(int layer, int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
            {
                return null;
            }

            if (layer < 0 || layer >= _mapLayers.Count - 1)
            {
                return null;
            }

            return _mapLayers[layer + 1][x, y];
        }

        public Tile FindTopTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
            {
                return null;
            }

            for (var i = _mapLayers.Count - 1; i >= 0; i--)
            {
                var layer = _mapLayers[i];
                var tile = layer[x, y];
                if (tile != null)
                {
                    return tile;
                }
            }

            return null;
        }

        public int FindTopLayer(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
            {
                return -1;
            }

            for (var i = _mapLayers.Count - 1; i >= 0; i--)
            {
                var layer = _mapLayers[i];
                var tile = layer[x, y];
                if (tile != null)
                {
                    return i;
                }
            }

            return 0;
        }

        public int MoveCost(int fromLayer, Vector2 from, Vector2 to)
        {
            return MoveCost(fromLayer, (int) from.x, (int) from.y, (int) to.x, (int) to.y);
        }

        public int MoveCost(int fromLayer, Vector2 from, int toLayer, Vector2 to)
        {
            return MoveCost(fromLayer, (int)from.x, (int)from.y, toLayer, (int)to.x, (int)to.y);
        }

        public int MoveCost(int fromLayer, int fromX, int fromY, int toX, int toY)
        {
            return MoveCost(fromLayer, fromX, fromY, _currentLayer, toX, toY);
        }

        public int MoveCost(int fromLayer, int fromX, int fromY, int toLayer, int toX, int toY)
        {
            if (fromX < 0 || fromY < 0 || fromX >= _width || fromY >= _height)
            {
                return -1;
            }

            if (toX < 0 || toY < 0 || toX >= _width || toY >= _height)
            {
                return -1;
            }

            if (fromLayer < 0 || fromLayer >= _mapLayers.Count)
            {
                return -1;
            }

            if (toLayer < 0 || toLayer >= _mapLayers.Count)
            {
                return -1;
            }

            var toTile = _mapLayers[toLayer][toX, toY];
            if (toTile != null && toTile.BuildingBlock != null)
            {
                return -1;
            }

            var toTopLayer = FindTopLayer(toX, toY);
            var toAboveTile = FindTileAbove(toLayer, toX, toY);

            var diff = Mathf.Abs(fromLayer - toTopLayer);

            // Stay on same layer -> 1 move cost
            if (diff == 0)
            {
                return MoveForwardCost;
            }

            // Cliff -> invalid move, unless there is a hole and we stay on same layer -> 1 move cost
            if (diff > 1)
            {
                if (toAboveTile != null)
                {
                    return -1;
                }

                if (fromLayer > toTopLayer)
                {
                    return -1;
                }
                
                return MoveForwardCost;
            }

            // Move down a layer -> 0 move cost
            if (fromLayer > toTopLayer)
            {
                return MoveDownCost;
            }

            // Move up a layer -> 3 move cost (should be more expensive to move down a layer and back up than just moving on current layer)
            return MoveUpCost;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            MapSource mapSource = null;
            if (_mapLoaderType == MapLoaderType.REX)
            {
                mapSource = REXMapLoader.Load("map");
            }
            else
            {
                mapSource = PNGMapLoader.Load();
            }

            if(mapSource == null)
            {
                return;
            }

            _width = mapSource.Width;
            _height = mapSource.Height;

            _grass = _tileDb.FindGround("Grass");
            _dirt = _tileDb.FindGround("Dirt");
            _stone = _tileDb.FindGround("Stone");
            var shadow = _tileDb.FindGround("Shadow");

            _plantItem = _tileDb.FindItem("Plant");

            _wall = _tileDb.FindBuildingBlock("Wall");
            _wallDecorated = _tileDb.FindBuildingBlock("WallDeco");
            _floor = _tileDb.FindBuildingBlock("Floor");

            for (var h = 0; h < mapSource.Layers.Count; h++)
            {
                var layer = mapSource.Layers[h];
                
                var mapLayer = Instantiate(_mapLayerPrefab, new Vector3(0, 0, 0), Quaternion.identity, transform);
                mapLayer.name = $"Layer({layer.Id})";
                mapLayer.Init(_width, _height);
                _mapLayers.Add(mapLayer);

                for (var x = 0; x < mapSource.Width; x++)
                {
                    for (var y = 0; y < mapSource.Height; y++)
                    {
                        var cell = layer[x, y];
                        if (cell == null)
                        {
                            continue;
                        }

                        var tile = Instantiate(_tilePrefab, new Vector3(x, y, 0), Quaternion.identity, mapLayer.transform);
                        tile.name = $"Tile({x},{y})";
                        mapLayer[x, y] = tile;

                        var groundCode = cell.GroundCode;

                        if (groundCode.r == 32 && groundCode.g == 64 && groundCode.b == 0)
                        {
                            tile.Init(_grass, h);
                        }
                        else if (groundCode.r == 102 && groundCode.g == 82 && groundCode.b == 51)
                        {
                            tile.Init(_dirt, h);
                        }
                        else if (groundCode.r == 77 && groundCode.g == 77 && groundCode.b == 77)
                        {
                            tile.Init(_stone, h);
                        }

                        var itemCode = cell.ItemCode;
                        if (itemCode.a > 0)
                        {
                            if (itemCode.r == 0 && itemCode.g == 255 && itemCode.b == 0)
                            {
                                tile.SetItem(_plantItem);
                            }
                        }
                    }
                }
            }

            var shadowHeight = _mapLayers.Count;
            _shadowLayer = Instantiate(_mapLayerPrefab, new Vector3(0, 0, 0), Quaternion.identity, transform);
            _shadowLayer.Init(_width, _height);
            _shadowLayer.name = "ShadowLayer";
            for (var x = 0; x < mapSource.Width; x++)
            {
                for (var y = 0; y < mapSource.Height; y++)
                {
                    var tile = Instantiate(_tilePrefab, new Vector3(x, y, 0), Quaternion.identity, _shadowLayer.transform);
                    tile.name = $"Tile({x},{y})";
                    tile.Init(shadow, shadowHeight, isShadow:true);
                    _shadowLayer[x, y] = tile;
                }
            }

            SpawnPlayerGroup();

            UpdateActiveLayer();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                if (_currentLayer < _mapLayers.Count - 1)
                {
                    _currentLayer++;
                    UpdateActiveLayer();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                if (_currentLayer > 0)
                {
                    _currentLayer--;
                    UpdateActiveLayer();
                }
            }
        }

        private void UpdateActiveLayer()
        {
            UI.Instance.UpdateLayer(_currentLayer);

            for (var layerIndex = 0; layerIndex < _mapLayers.Count; layerIndex++)
            {
                _mapLayers[layerIndex].gameObject.SetActive(layerIndex == _currentLayer);
            }

            UpdateShadows();
        }

        private void UpdateShadows()
        {
            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    var topLayer = FindTopLayer(x, y);
                    var activeShadow = topLayer != _currentLayer;
                    if (activeShadow)
                    {
                        var diff = Mathf.Abs(topLayer - _currentLayer);
                        // If we don't have a tile directly above us, then don't shadow us.
                        if (diff > 1 && FindTileAbove(_currentLayer, x, y) == null)
                        {
                            activeShadow = false;
                        }
                    }

                    _shadowLayer[x, y].gameObject.SetActive(activeShadow);

                    UpdateTransitions(x, y);
                }
            }
        }

        private void UpdateTransitions(int x, int y)
        {
            /*var tile = _mapLayers[_currentLayer][x, y];
            if (tile != null)
            {
                tile.UpdateTransition(MoveForwardCost, overrideTileSettings: true);
            }*/

            var tile = FindTopTile(x, y + 1);
            if (tile != null && tile.Layer == _currentLayer)
            {
                var cost = MoveCost(tile.Layer, tile.Pos.x, tile.Pos.y, x, y);
                tile.UpdateTransition(cost, overrideTileSettings: false);
            }

            tile = FindTopTile(x, y - 1);
            if (tile != null && tile.Layer == _currentLayer)
            {
                var cost = MoveCost(tile.Layer, tile.Pos.x, tile.Pos.y, x, y);
                tile.UpdateTransition(cost, overrideTileSettings: false);
            }

            tile = FindTopTile(x - 1, y);
            if (tile != null && tile.Layer == _currentLayer)
            {
                var cost = MoveCost(tile.Layer, tile.Pos.x, tile.Pos.y, x, y);
                tile.UpdateTransition(cost, overrideTileSettings: false);
            }

            tile = FindTopTile(x + 1, y);
            if (tile != null && tile.Layer == _currentLayer)
            {
                var cost = MoveCost(tile.Layer, tile.Pos.x, tile.Pos.y, x, y);
                tile.UpdateTransition(cost, overrideTileSettings: false);
            }
        }
    }
}