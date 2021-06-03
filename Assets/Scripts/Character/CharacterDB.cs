using System.Collections.Generic;
using UnityEngine;

namespace Fluid
{
    [CreateAssetMenu(fileName = "Character DB", menuName = "Fluid/Fortress/Character DB")]
    public class CharacterDB : ScriptableObject
    {
        public List<CharacterDbEntry> CharacterTypes;

        public CharacterDbEntry FindCharacter(Fortress.FortressType type)
        {
            foreach (var character in CharacterTypes)
            {
                if (character.Type == type)
                {
                    return character;
                }
            }

            return null;
        }
    }
}