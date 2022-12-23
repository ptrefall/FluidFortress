
using System.Collections;
using System.Collections.Generic;
using Fluid.AI;
using Fluid.AI.Character;
using FluidHTN;
using UnityEngine;
using UnityEngine.Rendering;

namespace Fluid
{
    public partial class Character : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;

        private CharacterContext _aiContext;
        private Domain<CharacterContext> _aiDomain;
        private Planner<CharacterContext> _aiPlanner;

        private static Color[] _debugColors = new Color[]
        {
            ColorPalette.YELLOW,
            ColorPalette.LMAGENTA,
            ColorPalette.LCYAN,
            ColorPalette.LBLUE,
            ColorPalette.BLUE,
            ColorPalette.MAGENTA,
            ColorPalette.RED,
            ColorPalette.LRED
        };

        private int _debugIndex = 0;
        private static List<int> _spentDebugIndices = new List<int>();
        private List<Tile> _colorOverrideTiles = new List<Tile>();

        private bool _isBusy;
        private bool _isIdle;
        private Vector3 _lastPosition;
        private string _name;

        private int _currentLayer;

        private Fortress _fortress;

        private Tile _currentJobTile;
        private Stack<Tile> _currentPath = new Stack<Tile>();

        private CharacterUI _ui;

        public (int x, int y) Pos => ((int)transform.position.x, (int)transform.position.y);
        public int Layer => _currentLayer;

        public Tile JobTile => _currentJobTile;
        public Fortress.Job Job => _currentJobTile != null ? _currentJobTile.Job : Fortress.Job.None;
        public Fortress Fortress => _fortress;

        public Stack<Tile> Path => _currentPath;

        public void GiveJobOrder(Fortress.Job job)
        {
            _aiContext.SetJobOrder(job, EffectType.Permanent);
        }

        public void Spawn(Fortress fortress, CharacterDbEntry entry, int layer, bool isLeader = false)
        {
            _fortress = fortress;
            _currentLayer = layer;

            if (isLeader)
            {
                _renderer.sprite = entry.LeaderSprite;
                _name = $"{entry.LeaderPrefix} {entry.GetRandomName()}";
            }
            else
            {
                _renderer.sprite = entry.GetRandomSprite();
                _name = entry.GetRandomName();
            }

            _aiContext = new CharacterContext(this);
            _aiPlanner = new Planner<CharacterContext>();
            _aiDomain = CharacterDomain.Create(_name);

            // Attempt to find a unique debug color 10 times, otherwise we'll just go with what we got.
            for (var i = 0; i < 10; i++)
            {
                _debugIndex = Random.Range(0, _debugColors.Length);
                if (_spentDebugIndices.Contains(_debugIndex) == false)
                {
                    _spentDebugIndices.Add(_debugIndex);
                    break;
                }
            }
        }

        public void SetUi(CharacterUI ui)
        {
            _ui = ui;
            _ui.Portrait = _renderer.sprite;
            _ui.Name = _name;
            _ui.UpdateJob(null);
            _ui.ListenClick(OnUiClicked);
        }

        public void ReturnJob()
        {
            if (_currentJobTile != null)
            {
                _fortress.ReturnJob(_currentJobTile);
                _currentJobTile = null;
                ClearPath();
                _ui?.UpdateJob(null);
                _aiContext.SetState(CharacterWorldState.HasJob, 0, EffectType.Permanent);
                _aiContext.SetState(CharacterWorldState.HasJobInRange, 0, EffectType.Permanent);
            }
        }

        public bool UpdateJob(Fortress.Job job, Tile tile)
        {
            if (job == Fortress.Job.None && _currentJobTile != null)
            {
                _fortress.CompleteJob(_currentJobTile);
                _currentJobTile = null;
                ClearPath();
                _ui?.UpdateJob(null);

                _aiContext.SetState(CharacterWorldState.HasJob,0, EffectType.Permanent);
                _aiContext.SetState(CharacterWorldState.HasJobInRange, 0, EffectType.Permanent);
                return true;
            }

            if (tile == null)
            {
                return false;
            }

            ClearPath();
            if (Map.Instance.FindPath(Layer, Pos.x, Pos.y, tile, ref _currentPath))
            {
                _ui?.UpdateJob(Map.Instance.GetJobSprite(job));
                _currentJobTile = tile;

                foreach (var t in _currentPath)
                {
                    t.OverrideColor = _debugColors[_debugIndex];
                    _colorOverrideTiles.Add(t);
                }

                return true;
            }

            return false;
        }

        public void ClearPath()
        {
            foreach (var t in _currentPath)
            {
                t.ResetColorOverride();
            }

            foreach (var t in _colorOverrideTiles)
            {
                t.ResetColorOverride();
            }

            _currentPath.Clear();
            _colorOverrideTiles.Clear();
        }

        public void TakeDamage(int damage)
        {
            StartCoroutine(LerpTakeDamage(0.75f, ColorPalette.LRED ,0.5f));
        }

        public bool Attack(Transform target, Vector3 dir)
        {
            StartCoroutine(LerpInOut(transform.position + (dir * 0.5f), _moveDuration));
            if (target != null)
            {

            }
            else
            {
                if (Map.Instance.TryAttack(Layer, transform.position, transform.position + dir))
                {
                    return true;
                }
            }

            return false;
        }

        public void MoveLeft()
        {
            Move(new Vector3(-1, 0, 0));
            _renderer.flipX = false;
        }

        public void MoveRight()
        {
            Move(new Vector3(1, 0, 0));
            _renderer.flipX = true;
        }

        public void MoveUp()
        {
            Move(new Vector3(0, 1, 0));
        }

        public void MoveDown()
        {
            Move(new Vector3(0, -1, 0));
        }

        public bool Move(Vector3 dir)
        {
            // TODO: Fix bug here that gets us into a wrong layer when we move through a dug out corridor
            var moveCost = Map.Instance.MoveCost(Layer, transform.position, transform.position + dir);
            if (moveCost < 0)
            {
                return false;
            }

            // 0 -> moved down a layer
            if (moveCost == Map.MoveDownCost)
            {
                _currentLayer--;
            }
            // 3 -> moved up a layer
            else if (moveCost == Map.MoveUpCost)
            {
                _currentLayer++;
            }

            _lastPosition = transform.position;
            StartCoroutine(LerpToDestination(transform.position + dir, _moveDuration + moveCost * 0.2f));
            StartCoroutine(LerpMoveScale(1.3f, 1f));
            return true;
        }

        private void Awake()
        {
            if (_renderer != null)
            {
                _renderer.shadowCastingMode = ShadowCastingMode.Off;
                _renderer.receiveShadows = false;

                if (_renderer.material != null)
                {
                    _renderer.material.color = _renderer.color;
                }
            }
        }

        private void Update()
        {
            UpdateVisibility();

            if (_isBusy)
            {
                return;
            }

            if (_isIdle == false)
            {
                StartCoroutine(LerpIdle(1.1f, _renderer.color, 1.2f));
            }

            UpdateInput();

            if (_aiDomain != null && _aiContext != null)
            {
                _aiPlanner?.Tick(_aiDomain, _aiContext, false);
            }

            /*if (_currentPath.Count > 0)
            {
                var to = _currentPath.Pop();
                to.ResetColorOverride();

                var dir = to.transform.position - transform.position;
                if (dir.magnitude > 0.01f)
                {
                    if (to == _currentJobTile)
                    {
                        Attack(null, dir);
                        UpdateJob(Fortress.Job.None, null);
                    }
                    else
                    {
                        Move(dir);
                    }
                }
            }
            else if (_currentJobTile != null)
            {
                var dir = _currentJobTile.transform.position - transform.position;
                if (dir.magnitude < 2f)
                {
                    Attack(null, dir);
                    UpdateJob(Fortress.Job.None, null);
                }
                else
                {
                    ReturnJob();
                }
            }*/
        }

        private void UpdateVisibility()
        {
            if (Map.Instance.CurrentLayer == Layer)
            {
                if (_renderer.gameObject.activeSelf == false)
                {
                    _renderer.gameObject.SetActive(true);
                }
            }
            else
            {
                if (_renderer.gameObject.activeSelf)
                {
                    _renderer.gameObject.SetActive(false);
                }
            }
        }

        private void OnUiClicked()
        {
            Map.Instance.Focus(Pos.x, Pos.y, Layer, update:true);
        }
    }
}
