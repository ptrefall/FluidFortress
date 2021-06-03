using System.IO;
using UnityEngine;

namespace Fluid
{
    public static class REXMapLoader
    {
        public static MapSource Load(string mapName = "map")
        {
            var directoryInfo = new DirectoryInfo($"{Application.streamingAssetsPath}/Maps");
            var allFiles = directoryInfo.GetFiles("*.xp");

            foreach (var file in allFiles)
            {
                if (file.Name.ToLower() == $"{mapName.ToLower()}.xp")
                {
                    return REXLoader.Load(file.OpenRead());
                }
            }

            return null;
        }
    }
}