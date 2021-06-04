using System;
using FluidHTN;
using FluidHTN.Conditions;

namespace Fluid.AI.Character.Conditions
{
    public class HasWorldStateGreaterThanCondition : ICondition
    {
        public string Name { get; }
        public CharacterWorldState State { get; }
        public byte Value { get; }

        public HasWorldStateGreaterThanCondition(CharacterWorldState state)
        {
            Name = $"HasStateGreaterThan({state}, {0})";
            State = state;
            Value = 0;
        }

        public HasWorldStateGreaterThanCondition(CharacterWorldState state, byte value)
        {
            Name = $"HasStateGreaterThan({state}, {value})";
            State = state;
            Value = value;
        }

        public bool IsValid(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                var result = c.HasStateGreaterThan(State, Value);
                if (ctx.LogDecomposition) ctx.Log(Name, $"HasWorldStateGreaterThanCondition.IsValid({State}:{Value}:{result})", ctx.CurrentDecompositionDepth, this);
                return result;
            }

            throw new Exception("Unexpected context type!");
        }
    }
}