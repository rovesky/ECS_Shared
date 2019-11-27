using UnityEngine;

namespace FootStone.ECS
{
    public struct GameTick
    {
        public static GameTick DefaultGameTick = new GameTick(30);

        public GameTick(int tickRate)
        {
            this.tickRate = tickRate;
            TickInterval = 1.0f / this.tickRate;
            Tick = 1;
            TickDuration = 0;
        }

        public void SetTick(uint tick, float tickDuration)
        {
            Tick = tick;
            TickDuration = tickDuration;
        }

        public float DurationSinceTick(int tick)
        {
            return (Tick - tick) * TickInterval + TickDuration;
        }

        public void AddDuration(float duration)
        {
            TickDuration += duration;
            var deltaTicks = Mathf.FloorToInt(TickDuration * TickRate);
            Tick += (uint) deltaTicks;
            TickDuration = TickDuration % TickInterval;
        }

        public static float GetDuration(GameTick start, GameTick end)
        {
            if (start.TickRate != end.TickRate)
            {
                GameDebug.LogError("Trying to compare time with different Tick rates (" + start.TickRate + " and " +
                                   end.TickRate + ")");
                return 0;
            }

            var result = (end.Tick - start.Tick) * start.TickInterval + end.TickDuration - start.TickDuration;
            return result;
        }

        /// <summary>Number of ticks per second.</summary>
        public int TickRate
        {
            get => tickRate;
            set
            {
                tickRate = value;
                TickInterval = 1.0f / tickRate;
            }
        }

        ///<summary>Duration of current Tick as fraction.</summary>
        public float TickDurationAsFraction => TickDuration / TickInterval;

        /// <summary>Length of each world Tick at current tickrate, e.g. 0.0166s if ticking at 60fps.</summary>
        public float TickInterval { get; private set; }

        /// <summary>Current Tick  </summary>
        public uint Tick { get; set; }

        /// <summary>Duration of current Tick </summary>
        public float TickDuration { get; set; }


        private int tickRate;
    }
}