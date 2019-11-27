using FootStone.ECS;
using System;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ECS
{

    [Serializable]
    public struct WorldTime : IComponentData
    {
        public GameTick gameTick;
        public double frameTime;

        public uint Tick => gameTick.Tick;
        public float TickDuration => gameTick.TickDuration;
        public float TickDurationAsFraction => gameTick.TickDurationAsFraction;
      

        public void SetTick(uint tick,float duration)
        {
            gameTick.SetTick(tick, duration);
        }

        public void SetTick(GameTick gameTick)
        {
            this.gameTick = gameTick;
        }
    }

}