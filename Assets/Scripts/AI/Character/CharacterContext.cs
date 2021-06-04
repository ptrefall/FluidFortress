using System;
using System.Collections.Generic;
using FluidHTN;
using FluidHTN.Contexts;
using FluidHTN.Factory;

namespace Fluid.AI.Character
{
    public sealed class CharacterContext : BaseContext
    {
        public Fluid.Character Self { get; }

        public override IFactory Factory { get; set; } = new DefaultFactory();
        public override List<string> MTRDebug { get; set; } = new List<string>();
        public override List<string> LastMTRDebug { get; set; } = new List<string>();
        public override bool DebugMTR { get; } = true;
        public override Queue<FluidHTN.Debug.IBaseDecompositionLogEntry> DecompositionLog { get; set; }
        public override bool LogDecomposition => false;

        public override byte[] WorldState { get; } = new byte[Enum.GetValues(typeof(CharacterWorldState)).Length];

        public CharacterContext(Fluid.Character self)
        {
            Self = self;

            Init();
        }

        public bool HasState(CharacterWorldState state, bool value)
        {
            return HasState((int)state, (byte)(value ? 1 : 0));
        }

        public bool HasState(CharacterWorldState state, byte value)
        {
            return HasState((int)state, value);
        }

        public bool HasState(CharacterWorldState state)
        {
            return HasState((int)state, 1);
        }

        public bool HasStateGreaterThan(CharacterWorldState state, byte value)
        {
            return GetState((int) state) > value;
        }

        public void SetState(CharacterWorldState state, bool value, EffectType type)
        {
            SetState((int)state, (byte)(value ? 1 : 0), true, type);
        }

        public void SetState(CharacterWorldState state, int value, EffectType type)
        {
            SetState((int)state, (byte)value, true, type);
        }

        public byte GetState(CharacterWorldState state)
        {
            return GetState((int)state);
        }

        public void SetJobOrder(Fluid.Fortress.Job job, EffectType type)
        {
            SetState(CharacterWorldState.JobOrder, (int)job, type);
        }

        public Fluid.Fortress.Job GetJobOrder()
        {
            var numTypes = Enum.GetValues(typeof(Fluid.Fortress.Job)).Length;
            var state = GetState(CharacterWorldState.JobOrder);
            if (state >= numTypes)
            {
                return Fluid.Fortress.Job.None;
            }

            return (Fluid.Fortress.Job) state;
        }
    }
}