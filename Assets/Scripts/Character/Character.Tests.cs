
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Fluid
{
    public partial class Character : MonoBehaviour
    {
        private void UpdateInput()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                MoveLeft();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                MoveRight();
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                MoveUp();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                MoveDown();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                Attack(null, (transform.position - _lastPosition).normalized);
            }
        }

        [ContextMenu("TakeDamage")]
        private void TestTakeDamage()
        {
            TakeDamage(1);
        }

        [ContextMenu("Attack Right")]
        private void TestAttackRight()
        {
            Attack(null, Vector3.right);
        }

        [ContextMenu("Attack Left")]
        private void TestAttackLeft()
        {
            Attack(null, Vector3.left);
        }

        [ContextMenu("Attack Up")]
        private void TestAttackUp()
        {
            Attack(null, Vector3.up);
        }

        [ContextMenu("Attack Down")]
        private void TestAttackDown()
        {
            Attack(null, Vector3.down);
        }

        [ContextMenu("Print Tile at Pos")]
        private void PrintTileAtPos()
        {
            if (Map.Instance != null)
            {
                var tile = Map.Instance.FindTopTile(Pos.x, Pos.y);
                if (tile != null)
                {
                    Debug.Log(tile.Type);
                }
            }
        }
    }
}
