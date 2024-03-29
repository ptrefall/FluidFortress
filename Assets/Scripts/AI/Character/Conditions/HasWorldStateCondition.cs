﻿using System;
using FluidHTN;
using FluidHTN.Conditions;

namespace Fluid.AI.Character.Conditions
{
    public class HasWorldStateCondition : ICondition
    {
        public string Name { get; }
        public CharacterWorldState State { get; }
        public byte Value { get; }

        public HasWorldStateCondition(CharacterWorldState state)
        {
            Name = $"HasState({state})";
            State = state;
            Value = 1;
        }

        public HasWorldStateCondition(CharacterWorldState state, byte value)
        {
            Name = $"HasState({state})";
            State = state;
            Value = value;
        }

        public bool IsValid(IContext ctx)
        {
            if (ctx is CharacterContext c)
            {
                var result = c.HasState(State, Value);
                if (ctx.LogDecomposition) ctx.Log(Name, $"HasWorldStateCondition.IsValid({State}:{Value}:{result})", ctx.CurrentDecompositionDepth, this);
                return result;
            }

            throw new Exception("Unexpected context type!");
        }
    }
}