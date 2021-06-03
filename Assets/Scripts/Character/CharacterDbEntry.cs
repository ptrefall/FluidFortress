using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fluid
{
    [Serializable]
    public class CharacterDbEntry
    {
        public string Name;
        public Fortress.FortressType Type;
        public Sprite[] Sprites;
        public Sprite LeaderSprite;
        public string[] Names;
        public string LeaderPrefix;

        public Sprite GetRandomSprite()
        {
            if (Sprites.Length == 0)
            {
                return null;
            }

            return Sprites[Random.Range(0, Sprites.Length)];
        }

        public string GetRandomName()
        {
            if (Names.Length == 0)
            {
                return "Unknown";
            }

            return Names[Random.Range(0, Names.Length)];
        }
    }
}