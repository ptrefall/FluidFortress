using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Fluid
{
    public static class REXLoader
    {
        public static MapSource Load(Stream stream)
        {
            var deflatedStream = new GZipStream(stream, CompressionMode.Decompress);

            using (var reader = new BinaryReader(deflatedStream))
            {
                var version = reader.ReadInt32();
                var layerCount = reader.ReadInt32();
                MapSource image = null;

                for (int currentLayer = 0; currentLayer < layerCount; currentLayer++)
                {
                    int width = reader.ReadInt32();
                    int height = reader.ReadInt32();

                    MapSource.Layer layer = null;

                    if (currentLayer == 0)
                    {
                        image = new MapSource(width, height);
                        layer = image.Layers[0];
                    }
                    else
                        layer = image.Create(currentLayer);

                    // Process cells (could probably be streamlined into index processing instead of x,y...
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            var glyph = reader.ReadInt32();
                            var fgr = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), 255);
                            var bgr = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), 255);

                            // skip transparent
                            if (bgr.r == 255 && bgr.g == 0 && bgr.b == 255)
                            {
                                continue;
                            }

                            var cell = new MapSource.REXCell(glyph, fgr, bgr); // background
                            layer[x, y] = cell;
                        }
                    }
                }

                return image;
            }
        }
    }
}