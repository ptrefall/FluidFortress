using FluidHTN;
using FluidHTN.Operators;

namespace Fluid.AI.Character.Operators
{
    public class DoJobOperator : IOperator
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

            var dir = c.Self.JobTile.transform.position - c.Self.transform.position;
            if (dir.magnitude < 2f)
            {
                c.Self.Attack(null, dir);
                c.Self.UpdateJob(Fluid.Fortress.Job.None, null);
                c.Self.ClearPath();
                return TaskStatus.Success;
            }

            c.Self.ReturnJob();
            c.Self.ClearPath();
            return TaskStatus.Success;
        }

        public void Stop(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                c.Self.ClearPath();
            }
        }

        public void Aborted(IContext ctx)
        {
            Stop(ctx);
        }
    }
}