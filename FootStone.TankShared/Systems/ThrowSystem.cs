﻿using FootStone.ECS;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    [DisableAutoCreation]
    public class ThrowSystem : ComponentSystem
    {
        private EntityQuery plateQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            plateQuery = GetEntityQuery(typeof(Plate));
        }

        protected override void OnUpdate()
        {
            Entities.WithAllReadOnly<Player>().ForEach((Entity entity,ref ThrowItem throwItem,ref UserCommand command,ref EntityPredictData predictData) =>
            {
                if (!command.buttons.IsSet(UserCommand.Button.Throw))
                    return;

                //  if (EntityManager.HasComponent<ReleaseItem>(entity))
                //    return;               

                var pickupEntity = predictData.pickupEntity;           
                if (pickupEntity == Entity.Null)
                    return;

                //   FSLog.Info("throw item");

                EntityManager.RemoveComponent<Parent>(pickupEntity);
                EntityManager.RemoveComponent<LocalToParent>(pickupEntity);
             
                FSLog.Info($"ThrowItem:{command.checkTick},{command.renderTick},{pickupEntity}");

                var physicsVelocity = EntityManager.GetComponentData<PhysicsVelocity>(pickupEntity);
                Vector3 linear = math.mul(predictData.rotation, Vector3.forward);
                linear.y = 0.4f;
                linear.Normalize();
                physicsVelocity.Linear = linear * throwItem.speed;
                EntityManager.SetComponentData(pickupEntity, physicsVelocity);

                EntityManager.SetComponentData(pickupEntity, new Translation()
                { Value = predictData.position + math.mul(predictData.rotation, new float3(0, 0.2f, 0.8f)) });

                predictData.pickupEntity = Entity.Null;

            });
        }      
    }
}