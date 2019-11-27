using System;
using Unity.Entities;

namespace FootStone.ECS
{
    [Serializable]
    public struct WorldTime : IComponentData
    {
        public GameTick GameTick;
        public double FrameTime;

        public uint Tick => GameTick.Tick;
        public float TickDuration => GameTick.TickDuration;
        public float TickDurationAsFraction => GameTick.TickDurationAsFraction;


        public void SetTick(uint tick, float duration)
        {
            GameTick.SetTick(tick, duration);
        }
    }
}