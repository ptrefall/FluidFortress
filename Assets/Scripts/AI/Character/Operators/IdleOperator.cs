
using FluidHTN;
using FluidHTN.Operators;
using UnityEngine;

namespace Fluid.AI.Character.Operators
{
    public class IdleOperator : IOperator
    {
        public TaskStatus Update(IContext ctx)
        {
            var c = ctx as CharacterContext;
            if (c == null)
            {
                return TaskStatus.Failure;
            }

            c.SetState(CharacterWorldState.HasJob, 0, EffectType.Permanent);
            c.SetState(CharacterWorldState.HasJobInRange, 0, EffectType.Permanent);

            if (Random.value < 0.01f)
            {
                var (x, y) = c.Self.Pos;
                var tile = Map.Instance.GetTile(c.Self.Layer, x, y);
                var to = Pathfinder.GetWalkableAdjacentTile(tile);
                if (to != null)
                {
                    var dir = to.transform.position - c.Self.transform.position;
                    if (dir.magnitude < 2f)
                    {
                        c.Self.Move(dir);
                    }

                    return TaskStatus.Continue;
                }

                return TaskStatus.Continue;
            }
            else
            {
                return TaskStatus.Continue;
            }
        }

        public void Stop(IContext ctx)
        {
        }
    }
}