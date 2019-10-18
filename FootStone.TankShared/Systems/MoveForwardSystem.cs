﻿using FootStone.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
{

    [DisableAutoCreation]
    public class MoveForwardSystem : FSComponentSystem
    {
        protected override void OnUpdate()
        {

            Entities.ForEach((ref LocalToWorld lw, ref Translation position, ref MoveForward move) =>
            {
                var tickDuration = GetSingleton<WorldTime>().tick.TickDuration;
                position = new Translation()
                {
                    Value = position.Value - lw.Forward * move.Speed * tickDuration
                };
            });

        }
    }
}
