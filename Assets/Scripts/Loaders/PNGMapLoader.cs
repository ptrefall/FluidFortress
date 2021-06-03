using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Fluid
{
    public static class PNGMapLoader
    {
        public static MapSource Load()
        {
            var directoryInfo = new DirectoryInfo($"{Application.streamingAssetsPath}/Map");
            var allFiles = directoryInfo.GetFiles("*.png");

            if (allFiles.Length == 0)
            {
                return null;
            }

            MapSource map = null;

            foreach (var file in allFiles)
            {
                var name = file.Name.Substring(0, file.Name.Length - 4);
                if (int.TryParse(name, out var layerId))
                {
                    var data = System.IO.File.ReadAllBytes(file.FullName);
                    
                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(data);
                    var pixels = tex.GetPixels32();
                    
                    if (map == null)
                    {
                        map = new MapSource(tex.width, tex.height);
                    }

                    var layer = map.GetOrCreate(layerId);

                    for (var x = 0; x < tex.width; x++)
                    {
                        for (var y = 0; y < tex.height; y++)
                        {
                            var pixel = pixels[y * tex.width + x];
                            if (pixel.a == 0)
                            {
                                continue;
                            }

                            var cell = layer[x, y];
                            if (cell == null)
                            {
                                cell = new MapSource.Cell(new Color32(0, 0, 0, 0), pixel);
                                layer[x, y] = cell;
                            }
                            else
                            {
                                cell.GroundCode = pixel;
                            }
                        }
                    }
                }
                else if (name.StartsWith("i") && name.Length > 1)
                {
                    if (int.TryParse(name.Substring(1, name.Length - 1), out var layerId2))
                    {
                        var data = System.IO.File.ReadAllBytes(file.FullName);

                        var tex = new Texture2D(2, 2);
                        tex.LoadImage(data);
                        var pixels = tex.GetPixels32();

                        if (map == null)
                        {
                            map = new MapSource(tex.width, tex.height);
                        }

                        var layer = map.GetOrCreate(layerId2);

                        for (var x = 0; x < tex.width; x++)
                        {
                            for (var y = 0; y < tex.height; y++)
                            {
                                var pixel = pixels[y * tex.width + x];
                                if (pixel.a == 0)
                                {
                                    continue;
                                }

                                var cell = layer[x, y];
                                if (cell == null)
                                {
                                    cell = new MapSource.Cell(pixel, new Color32(0, 0, 0, 0));
                                    layer[x, y] = cell;
                                }
                                else
                                {
                                    cell.ItemCode = pixel;
                                }
                            }
                        }
                    }
                }
            }

            map?.SortLayers();
            return map;
        }
    }
}