using Fluid.AI.Character.Conditions;
using Fluid.AI.Character.Effects;
using Fluid.AI.Character.Operators;
using FluidHTN;
using FluidHTN.Compounds;
using FluidHTN.Factory;
using FluidHTN.PrimitiveTasks;

namespace Fluid.AI.Character
{
    public class CharacterDomainBuilder : BaseDomainBuilder<CharacterDomainBuilder, CharacterContext>
    {
        public CharacterDomainBuilder(string domainName) : base(domainName, new DefaultFactory())
        {
        }

        public CharacterDomainBuilder HasState(CharacterWorldState state)
        {
            var condition = new HasWorldStateCondition(state);
            Pointer.AddCondition(condition);
            return this;
        }

        public CharacterDomainBuilder HasState(CharacterWorldState state, byte value)
        {
            var condition = new HasWorldStateCondition(state, value);
            Pointer.AddCondition(condition);
            return this;
        }

        public CharacterDomainBuilder HasStateGreaterThan(CharacterWorldState state)
        {
            var condition = new HasWorldStateGreaterThanCondition(state);
            Pointer.AddCondition(condition);
            return this;
        }

        public CharacterDomainBuilder HasStateGreaterThan(CharacterWorldState state, byte value)
        {
            var condition = new HasWorldStateGreaterThanCondition(state, value);
            Pointer.AddCondition(condition);
            return this;
        }

        public CharacterDomainBuilder SetState(CharacterWorldState state, EffectType type)
        {
            if (Pointer is IPrimitiveTask task)
            {
                var effect = new SetWorldStateEffect(state, type);
                task.AddEffect(effect);
            }
            return this;
        }

        public CharacterDomainBuilder SetState(CharacterWorldState state, bool value, EffectType type)
        {
            if (Pointer is IPrimitiveTask task)
            {
                var effect = new SetWorldStateEffect(state, value, type);
                task.AddEffect(effect);
            }
            return this;
        }

        public CharacterDomainBuilder SetState(CharacterWorldState state, byte value, EffectType type)
        {
            if (Pointer is IPrimitiveTask task)
            {
                var effect = new SetWorldStateEffect(state, value, type);
                task.AddEffect(effect);
            }
            return this;
        }

        public CharacterDomainBuilder IncrementState(CharacterWorldState state, EffectType type)
        {
            if (Pointer is IPrimitiveTask task)
            {
                var effect = new IncrementWorldStateEffect(state, type);
                task.AddEffect(effect);
            }
            return this;
        }

        public CharacterDomainBuilder IncrementState(CharacterWorldState state, byte value, EffectType type)
        {
            if (Pointer is IPrimitiveTask task)
            {
                var effect = new IncrementWorldStateEffect(state, value, type);
                task.AddEffect(effect);
            }
            return this;
        }

        public CharacterDomainBuilder GiveJob()
        {
            Action("Give job");
            if (Pointer is IPrimitiveTask task)
            {
                HasStateGreaterThan(CharacterWorldState.JobOrder, (byte) Fluid.Fortress.Job.None); // We require a job order
                HasState(CharacterWorldState.HasJob, 0); // And we can't already have a job
                
                task.SetOperator(new GiveJobOperator());
                
                SetState(CharacterWorldState.JobOrder, 0, EffectType.PlanAndExecute); // When a job is given, we remove the job order (it's spent)
                SetState(CharacterWorldState.HasJob, 1, EffectType.PlanAndExecute); // And we now have a new job
                SetState(CharacterWorldState.HasPath, 1, EffectType.PlanAndExecute); // And finally a successful path has been found to the job
            }
            End();

            return this;
        }

        public CharacterDomainBuilder MoveToJob()
        {
            Action("Move to job");
            if (Pointer is IPrimitiveTask task)
            {
                HasState(CharacterWorldState.HasJob, 1);
                HasState(CharacterWorldState.HasPath, 1);

                task.SetOperator(new MoveOnPathOperator());

                SetState(CharacterWorldState.HasPath, 0, EffectType.PlanAndExecute); // When we have reached our destination, we no longer have a path
                SetState(CharacterWorldState.HasJobInRange, 1, EffectType.PlanAndExecute); // When movement is completed, we have moved into range of the job
            }
            End();

            return this;
        }

        public CharacterDomainBuilder DoJob()
        {
            Action("Do my job");
            if (Pointer is IPrimitiveTask task)
            {
                HasState(CharacterWorldState.HasJob, 1);
                HasState(CharacterWorldState.HasJobInRange, 1);

                task.SetOperator(new DoJobOperator());

                SetState(CharacterWorldState.HasJob, 0); // We have completed our job, and thus no longer have a job
                SetState(CharacterWorldState.HasJobInRange, 0); // We have completed our job, and thus no longer have a job
            }
            End();

            return this;
        }

        public CharacterDomainBuilder Idle()
        {
            Action("Idle");
            if (Pointer is IPrimitiveTask task)
            {
                task.SetOperator(new IdleOperator());
            }

            End();

            return this;
        }

        public CharacterDomainBuilder AddBehaviour()
        {
            Select("Select behavior");
            { 
                Sequence("Job sequence");
                {
                    GiveJob();
                    MoveToJob();
                    DoJob();
                }
                End();
                Idle();
            }
            End();
            return this;
        }
    }
}