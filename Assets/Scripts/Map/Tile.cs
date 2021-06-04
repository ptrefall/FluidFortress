using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Fluid
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private SpriteRenderer _transition;
        [SerializeField] private SpriteRenderer _itemRenderer;
        [SerializeField] private SpriteRenderer _job;
        [SerializeField] private SpriteRenderer _selection;
        [SerializeField] private ColorIndex _color;

        private TileDbEntry _entry; // Only needed for shadows
        private int _layer;
        private int _transitionCost;

        private Color? _overrideColor;
        private Color? _lastColor;

        private Fortress.Job _currentJob = Fortress.Job.None;

        private TileDbEntry _item;
        private TileDbEntry _buildingBlock;

        public string Type => _color.ToString();
        public (int x, int y) Pos => ((int) transform.position.x, (int) transform.position.y);
        public int Layer => _layer;
        public TileDbEntry Item => _item;
        public TileDbEntry BuildingBlock => _buildingBlock;
        public Fortress.Job Job => _currentJob;

        public Color OverrideColor
        {
            get => _overrideColor ?? _renderer.color;
            set
            {
                _lastColor = _renderer.color;
                _overrideColor = value;
                _renderer.color = value;
            }
        }

        public void ResetColorOverride()
        {
            _overrideColor = null;
            _renderer.color = _lastColor ?? _renderer.color;
        }

        private void Awake()
        {
            if (_renderer != null)
            {
                _renderer.shadowCastingMode = ShadowCastingMode.On;
                _renderer.receiveShadows = true;
                _renderer.color = ColorPalette.GetColor(_color);

                if (_renderer.material != null)
                {
                    _renderer.material.color = ColorPalette.GetColor(_color);
                }
            }
        }

        public void Init(TileDbEntry entry, int layer, bool isShadow = false)
        {
            _layer = layer;
            _color = entry.Color;

            _renderer.color = ColorPalette.GetColor(_color);

            if (isShadow)
            {
                _entry = entry;
                _renderer.sortingLayerName = "Shadow";
                _renderer.sprite = entry.GetShadowSprite(-1);
            }
            else
            {
                _renderer.sortingOrder = layer;
                _transition.sortingOrder = layer + 1;
                _renderer.sprite = entry.GetRandomSprite();
            }
            

            if (_renderer.material != null)
            {
                _renderer.material.color = ColorPalette.GetColor(_color);
            }
        }

        public void SetItem(TileDbEntry item)
        {
            _item = item;

            if (item == null)
            {
                _itemRenderer.sprite = null;
            }
            else
            {
                _itemRenderer.sprite = item.GetRandomSprite();
                _itemRenderer.color = ColorPalette.GetColor(item.Color);
            }
        }

        public void SetBuildingBlock(TileDbEntry buildingBlock, TileDbEntry ground = null)
        {
            _buildingBlock = buildingBlock;

            if (buildingBlock == null)
            {
                _itemRenderer.sprite = null;

                if (ground != null)
                {
                    _renderer.sprite = ground.GetRandomSprite();
                    _renderer.color = ColorPalette.GetColor(ground.Color);
                }
            }
            else
            {
                _itemRenderer.sprite = buildingBlock.GetRandomSprite();
                _itemRenderer.color = ColorPalette.GetColor(buildingBlock.Color);

                if (ground != null)
                {
                    _renderer.sprite = ground.GetRandomSprite();
                    _renderer.color = ColorPalette.GetColor(ground.Color);
                }
            }
        }

        public void SetDecoration(TileDbEntry decoration, TileDbEntry ground = null)
        {
            if (BuildingBlock != null)
            {
                SetBuildingBlock(decoration, ground);
            }
            else
            {
                _renderer.sprite = decoration.GetRandomSprite();
                _renderer.color = ColorPalette.GetColor(decoration.Color);
            }
        }

        public void UpdateTransition(int cost, bool overrideTileSettings)
        {
            if (!overrideTileSettings && _transition.sprite != null)
            {
                if (_transitionCost == cost)
                {
                    return;
                }

                if (cost != Map.MoveUpCost && cost != Map.MoveDownCost)
                {
                    return;
                }
            }

            _transition.sprite = Map.Instance.GetTransitionSprite(cost);
            _transitionCost = cost;
        }

        public void UpdateShadow(int fromLayer, int fromX, int fromY)
        {
            if (_entry == null)
            {
                return;
            }

            var moveCost = Map.Instance.MoveCost(fromLayer, fromX, fromY, Pos.x, Pos.y);
            _renderer.sprite = _entry.GetShadowSprite(moveCost);
        }

        public void Select(ColorIndex color)
        {
            _selection.gameObject.SetActive(true);
            _selection.color = ColorPalette.GetColor(color);
        }

        public void Deselect()
        {
            _selection.gameObject.SetActive(false);
        }

        public void SetJob(Fortress.Job job)
        {
            _currentJob = job;

            switch (job)
            {
                case Fortress.Job.None:
                default:
                    _job.gameObject.SetActive(false);
                    break;
                case Fortress.Job.Build:
                    _job.gameObject.SetActive(true);
                    _job.sprite = Map.Instance.GetJobSprite(job);
                    break;
                case Fortress.Job.Dig:
                    _job.gameObject.SetActive(true);
                    _job.sprite = Map.Instance.GetJobSprite(job);
                    break;
                case Fortress.Job.Gather:
                    _job.gameObject.SetActive(true);
                    _job.sprite = Map.Instance.GetJobSprite(job);
                    break;
                case Fortress.Job.Decorate:
                    _job.gameObject.SetActive(true);
                    _job.sprite = Map.Instance.GetJobSprite(job);
                    break;
            }
        }

        public void UpdateJobAlpha(float alpha)
        {
            if (_job.gameObject.activeSelf)
            {
                _job.color = new Color(_job.color.r, _job.color.g, _job.color.b, alpha);
            }
        }

        [ContextMenu("Update Color")]
        public void UpdateColor()
        {
            if (_renderer != null)
            {
                _renderer.color = ColorPalette.GetColor(_color);

                if (_renderer.material != null)
                {
                    _renderer.material.color = ColorPalette.GetColor(_color);
                }
            }
        }
    }
}
