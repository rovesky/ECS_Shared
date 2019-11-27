using System.Diagnostics;
using Unity.Entities;

namespace FootStone.ECS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class WorldTimeSystem : ComponentSystem
    {
        private Stopwatch clock;
        private long stopwatchFrequency;


        protected override void OnCreate()
        {
            base.OnCreate();

            var worldTimeQuery = GetEntityQuery(ComponentType.ReadWrite<WorldTime>());
            EntityManager.CreateEntity(typeof(WorldTime));
            worldTimeQuery.SetSingleton(new WorldTime
            {
                FrameTime = 0,
                GameTick = new GameTick(30)
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
            worldTime.FrameTime = (double) clock.ElapsedTicks / stopwatchFrequency;
            SetSingleton(worldTime);
        }

        public long GetCurrentTime()
        {
            return clock.ElapsedMilliseconds;
        }
    }
}