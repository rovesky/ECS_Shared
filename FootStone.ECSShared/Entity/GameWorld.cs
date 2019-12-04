using System;
using System.Diagnostics;
using Unity.Entities;
using UnityEngine;

namespace FootStone.ECS
{
    public class GameWorld : IGameTime
    {
        private readonly long stopwatchFrequency;

        public GameWorld(string name = "world")
        {
            //  GameDebug.Log("GameWorld " + name + " initializing");

            stopwatchFrequency = Stopwatch.Frequency;
            Clock = new Stopwatch();
            Clock.Start();
        }

        public static GameWorld Active { get; set; }
        public Stopwatch Clock { get; set; }

        public double FrameTime { get; set; }

        public long ElapsedTicks => Clock.ElapsedTicks;

        public void Update()
        {
            FrameTime = (double) Clock.ElapsedTicks / stopwatchFrequency;
        }

        public T Spawn<T>(GameObject prefab)
        {
            throw new NotImplementedException();
        }

        public EntityManager GetEntityManager()
        {
            return World.Active.EntityManager;
        }
     
    }
}