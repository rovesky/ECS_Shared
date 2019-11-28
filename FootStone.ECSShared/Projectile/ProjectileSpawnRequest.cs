﻿using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using FootStone.ECS;

public struct ProjectileRequest : IComponentData        
{
    public static void Create(EntityCommandBuffer commandBuffer, int tick, int tickDelay, WeakAssetReference projectileAsset, Entity owner, int teamId, float3 startPosition, float3 endPosition)
    {
        var request = new ProjectileRequest
        {
            projectileAssetGuid = projectileAsset,
            startTick = tick,
            startPosition = startPosition,
            endPosition = endPosition,
            owner = owner,
            collisionTestTickDelay = tickDelay,
            teamId = teamId,
        };

        var entity = commandBuffer.CreateEntity();
        commandBuffer.AddComponent(entity, request);
    }
    
    public WeakAssetReference projectileAssetGuid;
    public int startTick;
    public float3 startPosition;
    public float3 endPosition;
    
    public Entity owner;
    public int teamId;
    public int collisionTestTickDelay;
}