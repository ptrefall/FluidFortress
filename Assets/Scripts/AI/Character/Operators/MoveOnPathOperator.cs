using FluidHTN;
using FluidHTN.Operators;

namespace Fluid.AI.Character.Operators
{
    public class MoveOnPathOperator : IOperator
    {
        public TaskStatus Update(IContext ctx)
        {
            var c = ctx as CharacterContext;
            if (c == null)
            {
                return TaskStatus.Failure;
            }

            if (c.Self.Job == Fluid.Fortress.Job.None)
            {
                return TaskStatus.Failure;
            }

            if (c.Self.Path.Count > 0)
            {
                var to = c.Self.Path.Pop();
                to.ResetColorOverride();

                var dir = to.transform.position - c.Self.transform.position;
                if (dir.magnitude > 0.01f)
                {
                    if (to == c.Self.JobTile)
                    {
                        c.Self.Path.Clear();
                        return TaskStatus.Success;
                    }

                    c.Self.Move(dir);
                    return TaskStatus.Continue;
                }

                return TaskStatus.Continue;
            }
            else
            {
                return TaskStatus.Success;
            }
        }

        public void Stop(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                c.Self.Path.Clear();
            }
        }
    }
}