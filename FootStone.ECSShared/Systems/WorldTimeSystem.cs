using FootStone.ECS;
using System;
using System.Diagnostics;
using Unity.Entities;

namespace Assets.Scripts.ECS
{

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class WorldTimeSystem : ComponentSystem
    {
        private long stopwatchFrequency;
        private Stopwatch clock;
      //  private double frameTime;
      //  private EntityQuery worldTimeQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

            var worldTimeQuery = GetEntityQuery(ComponentType.ReadWrite<WorldTime>());
            EntityManager.CreateEntity(typeof(WorldTime));
            worldTimeQuery.SetSingleton(new WorldTime()
            {
                frameTime = 0,
                gameTick = new GameTick(30)
            });

            stopwatchFrequency = Stopwatch.Frequency;
            clock = new Stopwatch();
            clock.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            clock.Stop();
        }
        protected override void OnUpdate()
        {
            var worldTime = GetSingleton<WorldTime>();
            worldTime.frameTime = (double)clock.ElapsedTicks / stopwatchFrequency;  
            SetSingleton(worldTime);
        }

        public long GetCurrentTime()
        {
            return clock.ElapsedMilliseconds;
        }

     
    }
}