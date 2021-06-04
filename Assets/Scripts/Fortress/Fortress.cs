using System;
using System.Collections;
using System.Collections.Generic;
using Fluid.AI;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

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
            Decorate,
            Any
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

        private List<Tile> _openBuildJobs = new List<Tile>();
        private List<Tile> _openDigJobs = new List<Tile>();
        private List<Tile> _openGatherJobs = new List<Tile>();
        private List<Tile> _openDecorateJobs = new List<Tile>();

        private List<Tile> _grabbedBuildJobs = new List<Tile>();
        private List<Tile> _grabbedDigJobs = new List<Tile>();
        private List<Tile> _grabbedGatherJobs = new List<Tile>();
        private List<Tile> _grabbedDecorateJobs = new List<Tile>();

        private List<Character> _characters = new List<Character>();

        private float _alphaPulse = 1f;

        public List<Character> Characters => _characters;

        public bool GiveJob(Character character, Job jobType = Job.Any)
        {
            if (character.Job != Job.None)
            {
                return false;
            }

            Tile job = null;

            if (jobType == Job.Any)
            {
                var options = new List<Job>();
                if (_openBuildJobs.Count > 0) options.Add(Job.Build);
                if (_openDigJobs.Count > 0) options.Add(Job.Dig);
                if (_openGatherJobs.Count > 0) options.Add(Job.Gather);
                if (_openDecorateJobs.Count > 0) options.Add(Job.Decorate);

                if (options.Count == 0)
                {
                    return false;
                }

                return GiveJob(character, options[Random.Range(0, options.Count)]);
            }

            if (jobType == Job.Build)
            {
                job = GetBuildJob();
                if (job != null)
                {
                    if (character.UpdateJob(job.Job, job))
                    {
                        _openBuildJobs.Remove(job);
                        _grabbedBuildJobs.Add(job);
                        return true;
                    }
                }
            }

            if (jobType == Job.Dig)
            {
                job = GetDigJob();
                if (job != null)
                {
                    if (character.UpdateJob(job.Job, job))
                    {
                        _openDigJobs.Remove(job);
                        _grabbedDigJobs.Add(job);
                        return true;
                    }
                }
            }

            if (jobType == Job.Gather)
            {
                job = GetGatherJob();
                if (job != null)
                {
                    if (character.UpdateJob(job.Job, job))
                    {
                        _openGatherJobs.Remove(job);
                        _grabbedGatherJobs.Add(job);
                        return true;
                    }
                }
            }

            if (jobType == Job.Decorate)
            {
                job = GetDecorateJob();
                if (job != null)
                {
                    if (character.UpdateJob(job.Job, job))
                    {
                        _openDecorateJobs.Remove(job);
                        _grabbedDecorateJobs.Add(job);
                        return true;
                    }
                }
            }

            return false;
        }

        public Tile GetBuildJob()
        {
            if (_openBuildJobs.Count == 0)
            {
                return null;
            }

            foreach (var job in _openBuildJobs)
            {
                if (Map.Instance.IsWalkable(job))
                {
                    return job;
                }

                var adj = Pathfinder.GetWalkableAdjacentTile(job);
                if (adj != null)
                {
                    return job;
                }
            }

            return null;
        }

        public Tile GetDigJob()
        {
            if (_openDigJobs.Count == 0)
            {
                return null;
            }

            foreach (var job in _openDigJobs)
            {
                if (Map.Instance.IsWalkable(job) == false)
                {
                    var adj = Pathfinder.GetWalkableAdjacentTile(job);
                    if (adj != null)
                    {
                        return job;
                    }
                }
                else
                {
                    return job;
                }
            }

            return null;
        }

        public Tile GetGatherJob()
        {
            if (_openGatherJobs.Count == 0)
            {
                return null;
            }

            foreach (var job in _openGatherJobs)
            {
                if (Map.Instance.IsWalkable(job) == false)
                {
                    var adj = Pathfinder.GetWalkableAdjacentTile(job);
                    if (adj != null)
                    {
                        return job;
                    }
                }
                else
                {
                    return job;
                }
            }

            return null;
        }

        public Tile GetDecorateJob()
        {
            if (_openDecorateJobs.Count == 0)
            {
                return null;
            }

            foreach (var job in _openDecorateJobs)
            {
                if (Map.Instance.IsWalkable(job) == false)
                {
                    var adj = Pathfinder.GetWalkableAdjacentTile(job);
                    if (adj != null)
                    {
                        return job;
                    }
                }
                else
                {
                    return job;
                }
            }

            return null;
        }

        public void ReturnJob(Tile tile)
        {
            switch (tile.Job)
            {
                default:
                case Job.None:
                    return;
                case Job.Build:
                    _grabbedBuildJobs.Remove(tile);
                    if (_openBuildJobs.Contains(tile) == false)
                    {
                        _openBuildJobs.Add(tile);
                    }
                    break;
                case Job.Dig:
                    _grabbedDigJobs.Remove(tile);
                    if (_openDigJobs.Contains(tile) == false)
                    {
                        _openDigJobs.Add(tile);
                    }
                    break;
                case Job.Gather:
                    _grabbedGatherJobs.Remove(tile);
                    if (_openGatherJobs.Contains(tile) == false)
                    {
                        _openGatherJobs.Add(tile);
                    }
                    break;
                case Job.Decorate:
                    _grabbedDecorateJobs.Remove(tile);
                    if (_openDecorateJobs.Contains(tile) == false)
                    {
                        _openDecorateJobs.Add(tile);
                    }
                    break;
            }
        }

        public void CompleteJob(Tile tile)
        {
            tile.SetJob(Job.None);

            if (_grabbedBuildJobs.Remove(tile)) return;
            if (_grabbedDigJobs.Remove(tile)) return;
            if (_grabbedGatherJobs.Remove(tile)) return;
            if (_grabbedDecorateJobs.Remove(tile)) return;
            if (_openBuildJobs.Remove(tile)) return;
            if (_openDigJobs.Remove(tile)) return;
            if (_openGatherJobs.Remove(tile)) return;
            if (_openDecorateJobs.Remove(tile)) return;
        }

        public void Decorate(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                if (_openDecorateJobs.Contains(tile))
                {
                    continue;
                }

                if (Map.Instance.IsWalkable(tile) || tile.BuildingBlock != null)
                {
                    tile.SetJob(Job.Decorate);
                    _openDecorateJobs.Add(tile);
                }
            }
        }

        public void Build(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                if (_openBuildJobs.Contains(tile))
                {
                    continue;
                }

                if (Map.Instance.IsWalkable(tile) && tile.Item == null && tile.BuildingBlock == null)
                {
                    tile.SetJob(Job.Build);
                    _openBuildJobs.Add(tile);
                }
            }
        }

        public void Dig(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                if (_openDigJobs.Contains(tile))
                {
                    continue;
                }

                if (Map.Instance.IsInShadow(tile) && Map.Instance.IsWalkable(tile) == false)
                {
                    tile.SetJob(Job.Dig);
                    _openDigJobs.Add(tile);
                }
                else if (tile.BuildingBlock != null)
                {
                    tile.SetJob(Job.Dig);
                    _openDigJobs.Add(tile);
                }
            }
        }

        public void Gather(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                if (_openGatherJobs.Contains(tile))
                {
                    continue;
                }

                if (tile.Item != null)
                {
                    tile.SetJob(Job.Gather);
                    _openGatherJobs.Add(tile);
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
                character.Spawn(this, _characterDb.FindCharacter(_type), h, i == 0);
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
            if (_openBuildJobs.Count > 0 || _openDigJobs.Count > 0 || _openGatherJobs.Count > 0 || _openDecorateJobs.Count > 0)
            {
                foreach (var c in _characters)
                {
                    if (c.Job == Job.None)
                    {
                        c.GiveJobOrder(Job.Any);
                    }
                }
            }

            foreach (var tile in _openBuildJobs)
            {
                tile.UpdateJobAlpha(_alphaPulse);
            }

            foreach (var tile in _openDigJobs)
            {
                tile.UpdateJobAlpha(_alphaPulse);
            }

            foreach (var tile in _openGatherJobs)
            {
                tile.UpdateJobAlpha(_alphaPulse);
            }

            foreach (var tile in _openDecorateJobs)
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
            var safetyCounter = _openBuildJobs.Count;
            while (_openBuildJobs.Count > 0)
            {
                Map.Instance.TryBuild(_openBuildJobs[_openBuildJobs.Count-1]);
                
                safetyCounter--;
                if (safetyCounter <= 0)
                {
                    break;
                }
            }

            safetyCounter = _openDigJobs.Count;
            while (_openDigJobs.Count > 0)
            {
                Map.Instance.TryDig(_openDigJobs[_openDigJobs.Count - 1]);

                safetyCounter--;
                if (safetyCounter <= 0)
                {
                    break;
                }
            }

            safetyCounter = _openGatherJobs.Count;
            while (_openGatherJobs.Count > 0)
            {
                Map.Instance.TryGather(_openGatherJobs[_openGatherJobs.Count - 1]);

                safetyCounter--;
                if (safetyCounter <= 0)
                {
                    break;
                }
            }

            safetyCounter = _openDecorateJobs.Count;
            while (_openDecorateJobs.Count > 0)
            {
                Map.Instance.TryDecorate(_openDecorateJobs[_openDecorateJobs.Count - 1]);

                safetyCounter--;
                if (safetyCounter <= 0)
                {
                    break;
                }
            }
        }
    }
}