
using System.Collections.Generic;
using UnityEngine;

namespace Fluid
{
    public class MapLayer : MonoBehaviour
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Tile[] Tiles { get; private set; }

        public void Init(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[width * height];
        }
        public Tile this[int x, int y]
        {
            get => Tiles[y * Width + x];
            set => Tiles[y * Width + x] = value;
        }
    }
}
