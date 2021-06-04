using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fluid.AI
{
    public class Pathfinder
    {
        private List<PathNode> _open = new List<PathNode>();
        private List<Tile> _closed = new List<Tile>();
        private List<PathNode> _options = new List<PathNode>();

        private PathNodeComparer _sortComparer = new PathNodeComparer();

        public bool FindPath(int fromLayer, int fromX, int fromY, int toLayer, int toX, int toY, ref Stack<Tile> path)
        {
            var m = Map.Instance;

            var from = m.GetTile(fromLayer, fromX, fromY);
            var to = m.GetTile(toLayer, toX, toY);

            return FindPath(from, to, ref path);
        }

        public bool FindPath(Tile from, Tile to, ref Stack<Tile> path)
        {
            var m = Map.Instance;
            if (m.IsWalkable(to) == false)
            {
                to = GetWalkableAdjacentTile(to);
                if (to == null)
                {
                    return false;
                }
            }

            var start = new PathNode { Tile = from };
            var goal = new PathNode { Tile = to };

            start.SetDistance(goal);

            _open.Clear();
            _open.Add(start);

            _closed.Clear();

            while (_open.Count > 0)
            {
                var n = _open[0];

                if (n.Tile == goal.Tile)
                {
                    BuildPath(n, ref path);
                    return true;
                }

                _closed.Add(n.Tile);
                _open.Remove(n);

                _options.Clear();
                if (GetOptions(n, goal, ref _options))
                {
                    foreach (var no in _options)
                    {
                        var nx = _open.FirstOrDefault(x => x.Tile.GetInstanceID() == no.Tile.GetInstanceID());
                        if (nx != null)
                        {
                            if (no.CostDist < nx.CostDist)
                            {
                                _open.Remove(nx);
                                _open.Add(no);
                            }
                        }
                        else
                        {
                            _open.Add(no);
                        }
                    }

                    _open.Sort(_sortComparer);
                }
            }

            return false;
        }

        private void BuildPath(PathNode end, ref Stack<Tile> path)
        {
            path.Clear();

            var n = end;
            path.Push(n.Tile);

            while (n.Parent != null)
            {
                n = n.Parent;
                path.Push(n.Tile);
            }
        }

        private bool GetOptions(PathNode current, PathNode goal, ref List<PathNode> options)
        {
            var m = Map.Instance;
            var (x, y) = current.Tile.Pos;
            var l = current.Tile.Layer;

            var n = CreateNode(current, goal, m, l, x, y - 1);
            var e = CreateNode(current, goal, m, l, x + 1, y);
            var s = CreateNode(current, goal, m, l, x, y + 1);
            var w = CreateNode(current, goal, m, l, x - 1, y);

            if (n != null) options.Add(n);
            if (e != null) options.Add(e);
            if (s != null) options.Add(s);
            if (w != null) options.Add(w);

            return options.Count > 0;
        }

        private PathNode CreateNode(PathNode current, PathNode goal, Map m, int l, int x, int y)
        {
            var t = GetWalkableTile(m, l, x, y);
            if (t == null)
            {
                return null;
            }

            if (_closed.Contains(t))
            {
                return null;
            }

            var cost = m.MoveCost(l, current.Tile.Pos.x, current.Tile.Pos.y, t.Layer, x, y);
            if (cost < 0)
            {
                return null;
            }

            var pn = new PathNode { Parent = current, Tile = t, Cost = current.Cost + cost};
            pn.SetDistance(goal);
            return pn;
        }

        private static Tile GetWalkableTile(Map m, int l, int x, int y)
        {
            var t = m.GetTile(l, x, y);
            if (t != null)
            {
                if (m.IsWalkable(t))
                {
                    return t;
                }
            }

            t = m.FindTopTile(x, y);
            if (t != null)
            {
                if (m.IsWalkable(t))
                {
                    return t;
                }
            }

            return null;
        }

        private static System.Random rng = new System.Random();
        private static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private static List<(int x, int y)> _dir = new List<(int x, int y)>();
        public static Tile GetWalkableAdjacentTile(Tile tile)
        {
            var m = Map.Instance;
            var (x, y) = tile.Pos;
            var l = tile.Layer;

            _dir.Add((x, y - 1));
            _dir.Add((x + 1, y));
            _dir.Add((x, y + 1));
            _dir.Add((x - 1, y));
            Shuffle(_dir);

            foreach (var dir in _dir)
            {
                var t = GetWalkableTile(m, l, dir.x, dir.y);
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }
    }

    public class PathNode
    {
        public PathNode Parent;
        public Tile Tile;
        public float Cost;
        public float Distance;
        public float CostDist => Cost + Distance;

        public void SetDistance(int toX, int toY, int toH)
        {
            Distance = Mathf.Abs(toX - Tile.Pos.x) + Mathf.Abs(toY - Tile.Pos.y) + Mathf.Abs(toH - Tile.Layer);
        }

        public void SetDistance(Tile to)
        {
            SetDistance(to.Pos.x, to.Pos.y, to.Layer);
        }

        public void SetDistance(PathNode to)
        {
            SetDistance(to.Tile);
        }
    }

    public class PathNodeComparer : IComparer<PathNode>
    {
        public int Compare(PathNode x, PathNode y)
        {
            return x.CostDist.CompareTo(y.CostDist);
        }
    }
}