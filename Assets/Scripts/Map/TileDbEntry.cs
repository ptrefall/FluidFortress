using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fluid
{
    [Serializable]
    public class TileDbEntry
    {
        public string Name;
        public TileType Type;
        public ColorIndex Color;
        public Sprite[] Sprites;

        public Sprite GetRandomSprite()
        {
            if (Sprites.Length == 0)
            {
                return null;
            }

            return Sprites[Random.Range(0, Sprites.Length)];
        }

        public Sprite GetShadowSprite(int moveCost)
        {
            if (Sprites.Length == 0)
            {
                return null;
            }

            if (moveCost < 0)
            {
                return Sprites[0];
            }

            if (moveCost == Map.MoveForwardCost)
            {
                return Sprites[0];
            }

            if (moveCost == Map.MoveDownCost && Sprites.Length > 1)
            {
                return Sprites[1];
            }

            if (moveCost == Map.MoveUpCost && Sprites.Length > 2)
            {
                return Sprites[2];
            }

            return Sprites[0];
        }
    }

    public enum TileType
    {
        GRASS,
        DIRT,
        STONE,
        SHADOW
    }
}