using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fluid
{
    public class Fortress : MonoBehaviour
    {
        public enum Job
        {
            None,
            Dig,
            Gather,
            Build,
            Decorate
        };

        public enum FortressType
        {
            Dwarven,
            Human,
            Elven,
            Goblin
        }

        [SerializeField] private FortressType _type;
        [SerializeField] private CharacterDB _characterDb;
        [SerializeField] private Character _characterPrefab;
        [SerializeField] private int _spawnCount = 3;
        [SerializeField] private AnimationCurve _jobCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private List<Tile> _buildJobs = new List<Tile>();
        private List<Tile> _digJobs = new List<Tile>();
        private List<Tile> _gatherJobs = new List<Tile>();
        private List<Tile> _decorateJobs = new List<Tile>();

        private List<Character> _characters = new List<Character>();

        private float _alphaPulse = 1f;

        public List<Character> Characters => _characters;

        public void CompleteJob(Tile tile)
        {
            tile.SetJob(Job.None);

            if (_buildJobs.Remove(tile)) return;
            if (_digJobs.Remove(tile)) return;
            if (_gatherJobs.Remove(tile)) return;
            if (_decorateJobs.Remove(tile)) return;
        }

        public void Decorate(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                if (_decorateJobs.Contains(tile))
                {
                    continue;
                }

                if (Map.Instance.IsWalkable(tile) || tile.BuildingBlock != null)
                {
                    tile.SetJob(Job.Decorate);
                    _decorateJobs.Add(tile);
                }
            }
        }

        public void Build(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                if (_buildJobs.Contains(tile))
                {
                    continue;
                }

                if (Map.Instance.IsWalkable(tile) && tile.Item == null && tile.BuildingBlock == null)
                {
                    tile.SetJob(Job.Build);
                    _buildJobs.Add(tile);
                }
            }
        }

        public void Dig(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                if (_digJobs.Contains(tile))
                {
                    continue;
                }

                if (Map.Instance.IsInShadow(tile) && Map.Instance.IsWalkable(tile) == false)
                {
                    tile.SetJob(Job.Dig);
                    _digJobs.Add(tile);
                }
                else if (tile.BuildingBlock != null)
                {
                    tile.SetJob(Job.Dig);
                    _digJobs.Add(tile);
                }
            }
        }

        public void Gather(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                if (_gatherJobs.Contains(tile))
                {
                    continue;
                }

                if (tile.Item != null)
                {
                    tile.SetJob(Job.Gather);
                    _gatherJobs.Add(tile);
                }
            }
        }

        public void Spawn(int x, int y)
        {
            for (var i = 0; i < _spawnCount; i++)
            {
                var px = x;
                var py = y;

                if (i != 0)
                {
                    for (var j = 0; j < 100; j++)
                    {
                        var offset = Random.insideUnitCircle * 3;
                        var ox = (int) (x + offset.x);
                        var oy = (int) (y + offset.y);

                        if (Map.Instance.IsValid(ox, oy))
                        {
                            var positionTaken = false;
                            foreach (var c in _characters)
                            {
                                if (c.Pos.x == ox && c.Pos.y == oy)
                                {
                                    positionTaken = true;
                                    break;
                                }
                            }

                            if (positionTaken)
                            {
                                continue;
                            }

                            px = ox;
                            py = oy;
                            break;
                        }
                    }
                }

                var h = Map.Instance.FindTopLayer(px, py);

                var character = Instantiate(_characterPrefab, new Vector3(px, py, 0), Quaternion.identity, transform);
                character.Spawn(_characterDb.FindCharacter(_type), h, i == 0);
                _characters.Add(character);
                UI.Instance.AddCharacter(character);
            }
        }

        private void Start()
        {
            StartCoroutine(JobPulse(2.0f));
        }

        private void Update()
        {
            foreach (var tile in _buildJobs)
            {
                tile.UpdateJobAlpha(_alphaPulse);
            }

            foreach (var tile in _digJobs)
            {
                tile.UpdateJobAlpha(_alphaPulse);
            }

            foreach (var tile in _gatherJobs)
            {
                tile.UpdateJobAlpha(_alphaPulse);
            }

            foreach (var tile in _decorateJobs)
            {
                tile.UpdateJobAlpha(_alphaPulse);
            }
        }

        private IEnumerator JobPulse(float duration)
        {
            while (gameObject.activeSelf)
            {
                yield return LerpAlpha(0.1f, duration * 0.5f);
                yield return LerpAlpha(1f, duration * 0.5f);
            }
        }

        IEnumerator LerpAlpha(float targetAlpha, float duration)
        {
            float time = 0;
            var startAlpha = _alphaPulse;

            while (time < duration)
            {
                var delta = time / duration;
                _alphaPulse = Mathf.Lerp(startAlpha, targetAlpha, _jobCurve.Evaluate(delta));
                time += Time.deltaTime;
                yield return null;
            }

            _alphaPulse = targetAlpha;
        }

        [ContextMenu("Finish All Jobs")]
        private void FinishAllJobs()
        {
            var safetyCounter = _buildJobs.Count;
            while (_buildJobs.Count > 0)
            {
                Map.Instance.TryBuild(_buildJobs[_buildJobs.Count-1]);
                
                safetyCounter--;
                if (safetyCounter <= 0)
                {
                    break;
                }
            }

            safetyCounter = _digJobs.Count;
            while (_digJobs.Count > 0)
            {
                Map.Instance.TryDig(_digJobs[_digJobs.Count - 1]);

                safetyCounter--;
                if (safetyCounter <= 0)
                {
                    break;
                }
            }

            safetyCounter = _gatherJobs.Count;
            while (_gatherJobs.Count > 0)
            {
                Map.Instance.TryGather(_gatherJobs[_gatherJobs.Count - 1]);

                safetyCounter--;
                if (safetyCounter <= 0)
                {
                    break;
                }
            }

            safetyCounter = _decorateJobs.Count;
            while (_decorateJobs.Count > 0)
            {
                Map.Instance.TryDecorate(_decorateJobs[_decorateJobs.Count - 1]);

                safetyCounter--;
                if (safetyCounter <= 0)
                {
                    break;
                }
            }
        }
    }
}