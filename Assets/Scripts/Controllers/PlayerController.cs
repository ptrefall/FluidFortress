using System.Collections.Generic;
using UnityEngine;

namespace Fluid
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [SerializeField] private Camera _camera;
        [SerializeField] private CameraFollower _cameraFollower;
        [SerializeField] private MouseReticle _reticle;
        [SerializeField] private Fortress _fortress;

        private Vector2 _lastMousePosition;
        private Vector2 _dragStartPosition;
        private readonly List<Tile> _currentSelection = new List<Tile>();
        private bool _isDragging = false;

        private static readonly Vector2 _mouseToTileOffset = new Vector2(1.25f, 0.5f);

        public Fortress Fortress => _fortress;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            UI.Instance.ListenBuild(OnBuildClicked);
            UI.Instance.ListenDig(OnDigClicked);
            UI.Instance.ListenGather(OnGatherClicked);
            UI.Instance.ListenDecorate(OnDecorateClicked);
            UI.Instance.ListenCancel(OnCancelClicked);
        }

        private void Update()
        {
            if (_camera == null)
            {
                return;
            }

            Vector2 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);

            if (UI.Instance.IsModal == false)
            {
                UpdateReticle(mousePosition);

                if (Input.GetMouseButtonDown(0))
                {
                    StartDrag(mousePosition);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    EndDrag(mousePosition);
                }
                else if (Input.GetMouseButton(0))
                {
                    UpdateDrag(mousePosition);
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    OnCancelClicked();
                }
            }

            if (Input.GetMouseButton(1))
            {
                DragCamera(mousePosition);
            }

            if (Input.mouseScrollDelta.y > 0.1f)
            {
                Map.Instance.IncrementLayer();
            }
            else if (Input.mouseScrollDelta.y < -0.1f)
            {
                Map.Instance.DecrementLayer();
            }

            _lastMousePosition = mousePosition;
        }

        private void OnDecorateClicked()
        {
            if (_currentSelection.Count == 0)
            {
                return;
            }

            _fortress.Decorate(_currentSelection);
            ResetSelection();
            UI.Instance.HideSelectionActions();
        }

        private void OnBuildClicked()
        {
            if (_currentSelection.Count == 0)
            {
                return;
            }

            _fortress.Build(_currentSelection);
            ResetSelection();
            UI.Instance.HideSelectionActions();
        }

        private void OnDigClicked()
        {
            if (_currentSelection.Count == 0)
            {
                return;
            }

            _fortress.Dig(_currentSelection);
            ResetSelection();
            UI.Instance.HideSelectionActions();
        }

        private void OnGatherClicked()
        {
            if (_currentSelection.Count == 0)
            {
                return;
            }

            _fortress.Gather(_currentSelection);
            ResetSelection();
            UI.Instance.HideSelectionActions();
        }

        private void OnCancelClicked()
        {
            ResetSelection();
            UI.Instance.HideSelectionActions();
        }

        private void StartDrag(Vector2 mousePosition)
        {
            _dragStartPosition = mousePosition;
        }

        private void UpdateDrag(Vector2 mousePosition)
        {
            if (_dragStartPosition != mousePosition)
            {
                if (_isDragging == false && _currentSelection.Count > 0)
                {
                    ResetSelection();
                    _isDragging = true;
                }

                UpdateSelectionGhost(mousePosition, ColorIndex.LCYAN);
            }
        }

        private void EndDrag(Vector2 mousePosition)
        {
            if (_isDragging == false)
            {
                return;
            }

            _isDragging = false;

            UpdateSelectionGhost(mousePosition, ColorIndex.LGREEN);
            UI.Instance.ShowSelectionActions();
        }

        private void UpdateSelectionGhost(Vector2 mousePosition, ColorIndex color)
        {
            ResetSelection();

            var startX = (int)(_dragStartPosition.x + _mouseToTileOffset.x);
            var startY = (int)(_dragStartPosition.y + _mouseToTileOffset.y);
            var endX = (int)(mousePosition.x + _mouseToTileOffset.x);
            var endY = (int)(mousePosition.y + _mouseToTileOffset.y);

            if (startX > endX)
            {
                var x = startX;
                startX = endX;
                endX = x;
            }

            if (startY > endY)
            {
                var y = startY;
                startY = endY;
                endY = y;
            }

            for (var x = startX; x <= endX; x++)
            {
                for (var y = startY; y <= endY; y++)
                {
                    var tile = Map.Instance.GetTile(x, y);
                    if (tile != null)
                    {
                        _currentSelection.Add(tile);
                        tile.Select(color);
                    }
                }
            }
        }

        private void ResetSelection()
        {
            foreach (var tile in _currentSelection)
            {
                if (tile != null)
                {
                    tile.Deselect();
                }
            }

            _currentSelection.Clear();
        }

        private void DragCamera(Vector2 mousePosition)
        {
            var diff = _lastMousePosition - mousePosition;
            _cameraFollower.transform.Translate(diff);
        }

        private void UpdateReticle(Vector2 mousePosition)
        {
            var x = (int) (mousePosition.x + _mouseToTileOffset.x);
            var y = (int) (mousePosition.y + _mouseToTileOffset.y);

            if (x < 0 || y < 0 || x >= Map.Instance.Width || y >= Map.Instance.Height)
            {
                if (_reticle.gameObject.activeSelf)
                {
                    _reticle.gameObject.SetActive(false);
                }
            }
            else
            {
                if (_reticle.gameObject.activeSelf == false)
                {
                    _reticle.gameObject.SetActive(true);
                }

                _reticle.transform.position = new Vector3(x, y, 0);
            }
        }
    }
}