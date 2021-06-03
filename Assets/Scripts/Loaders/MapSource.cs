using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fluid
{
    public class MapSource
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<Layer> Layers { get; private set; } = new List<Layer>();
        public MapSource(int width, int height)
        {
            Width = width;
            Height = height;
            Layers.Add(new Layer(0, Width, Height));
        }

        public void SortLayers()
        {
            Layers = Layers.OrderBy(l => l.Id).ToList();
        }

        public Layer GetOrCreate(int layerId)
        {
            foreach (var layer in Layers)
            {
                if (layer.Id == layerId)
                {
                    return layer;
                }
            }

            return Create(layerId);
        }

        public Layer Create(int layerId)
        {
            var layer = new Layer(layerId, Width, Height);
            Layers.Add(layer);
            return layer;
        }

        public class Layer
        {
            public int Id { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public Cell[] Cells { get; private set; }
            public Layer(int layerId, int width, int height)
            {
                Id = layerId;
                Width = width;
                Height = height;
                Cells = new Cell[Width * Height];
            }

            public Cell this[int x, int y]
            {
                get => Cells[y * Width + x];
                set => Cells[y * Width + x] = value;
            }
        }

        public class Cell
        {
            public Color32 GroundCode { get; set; }
            public Color32 ItemCode { get; set; }
            public Cell(Color32 itemCode, Color32 groundCode)
            {
                ItemCode = itemCode;
                GroundCode = groundCode;
            }
        }

        public class REXCell : Cell
        {
            public int Id { get; private set; }

            public REXCell(int id, Color32 itemCode, Color32 groundCode) : base(itemCode, groundCode)
            {
                Id = id;
            }
        }
    }
}