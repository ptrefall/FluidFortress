using FluidHTN;
using FluidHTN.Operators;

namespace Fluid.AI.Character.Operators
{
    public class GiveJobOperator : IOperator
    {
        public TaskStatus Update(IContext ctx)
        {
            var c = ctx as CharacterContext;
            if (c == null)
            {
                return TaskStatus.Failure;
            }

            if (c.Self.Fortress.GiveJob(c.Self, (Fluid.Fortress.Job) c.GetJobOrder()))
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
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