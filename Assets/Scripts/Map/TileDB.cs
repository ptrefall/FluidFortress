using System.Collections.Generic;
using UnityEngine;

namespace Fluid
{
    [CreateAssetMenu(fileName = "Tile DB", menuName = "Fluid/Fortress/Tile DB")]
    public class TileDB : ScriptableObject
    {
        public List<TileDbEntry> Tiles;
        public List<TileDbEntry> Items;
        public List<TileDbEntry> BuildingBlocks;
        public Sprite Up;
        public Sprite Down;
        public Sprite Left;
        public Sprite Right;
        public Sprite Blocked;

        public Sprite Build;
        public Sprite Dig;
        public Sprite Gather;
        public Sprite Decorate;

        public TileDbEntry FindGround(string name)
        {
            foreach (var tile in Tiles)
            {
                if (tile.Name.Equals(name))
                {
                    return tile;
                }
            }

            return null;
        }

        public TileDbEntry FindItem(string name)
        {
            foreach (var tile in Items)
            {
                if (tile.Name.Equals(name))
                {
                    return tile;
                }
            }

            return null;
        }

        public TileDbEntry FindBuildingBlock(string name)
        {
            foreach (var tile in BuildingBlocks)
            {
                if (tile.Name.Equals(name))
                {
                    return tile;
                }
            }

            return null;
        }
    }
}