﻿using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Profiling;
using Unity.Mathematics;
using FootStone.ECS;


[DisableAutoCreation]
public class CreateProjectileMovementCollisionQueries : ComponentSystem
{
    EntityQuery ProjectileGroup;

    public CreateProjectileMovementCollisionQueries()  {
    }

    protected override void OnCreate()
    {
        base.OnCreateManager();
        ProjectileGroup = GetEntityQuery(typeof(UpdateProjectileFlag), typeof(ProjectileData), 
            ComponentType.Exclude<Despawn>());
    }

    protected override void OnUpdate()
    {
        var entityArray = ProjectileGroup.ToEntityArray(Allocator.Persistent);
        var projectileDataArray = ProjectileGroup.ToComponentDataArray<ProjectileData>(Allocator.Persistent);
        var worldTime = GetSingleton<WorldTime>();
        for (var i = 0; i < projectileDataArray.Length; i++)
        {
            var projectileData = projectileDataArray[i];
            if (projectileData.impactTick > 0)
                continue;

            var entity = entityArray[i];

            var collisionTestTick = (int)worldTime.Tick - projectileData.collisionCheckTickDelay;

            var totalMoveDuration = worldTime.GameTick.DurationSinceTick(projectileData.startTick);
            var totalMoveDist = totalMoveDuration * projectileData.settings.velocity;

            var dir = Vector3.Normalize(projectileData.endPos - projectileData.startPos);
            var newPosition = (Vector3)projectileData.startPos + dir * totalMoveDist;
            var moveDist = math.distance(projectileData.position, newPosition);

            var collisionMask = ~(1U << projectileData.teamId);

            var queryReciever = World.GetExistingSystem<RaySphereQueryReciever>();
            projectileData.rayQueryId = queryReciever.RegisterQuery(new RaySphereQueryReciever.Query()
            {
                hitCollisionTestTick = collisionTestTick,
                origin = projectileData.position,
                direction = dir,
                distance = moveDist,
                radius = projectileData.settings.collisionRadius,
                mask = collisionMask,
                ExcludeOwner = projectileData.projectileOwner,
            });
            PostUpdateCommands.SetComponent(entity,projectileData);
        }
        entityArray.Dispose();
        projectileDataArray.Dispose();
    }
}

[DisableAutoCreation]
public class HandleProjectileMovementCollisionQuery :ComponentSystem
{
    EntityQuery ProjectileGroup;

    public HandleProjectileMovementCollisionQuery() { }

    protected override void OnCreateManager()
    {
        base.OnCreateManager();
        ProjectileGroup = GetEntityQuery(typeof(UpdateProjectileFlag), typeof(ProjectileData), 
            ComponentType.Exclude<Despawn>());
    }
    
    protected override void OnUpdate()
    {
        var entityArray = ProjectileGroup.ToEntityArray(Allocator.Persistent);
        var projectileDataArray = ProjectileGroup.ToComponentDataArray<ProjectileData>(Allocator.Persistent);
        var queryReciever = World.GetExistingSystem<RaySphereQueryReciever>();    
        for (var i = 0; i < projectileDataArray.Length; i++)
        {
            var projectileData = projectileDataArray[i];
            
            if (projectileData.impactTick > 0)
                continue;
            
            RaySphereQueryReciever.Query query;
            RaySphereQueryReciever.QueryResult queryResult;
            queryReciever.GetResult(projectileData.rayQueryId, out query, out queryResult);
            
            var projectileVec = projectileData.endPos - projectileData.startPos;
            var projectileDir = Vector3.Normalize(projectileVec);
            var newPosition = (Vector3)projectileData.position + projectileDir * query.distance;

            var impact = queryResult.hit == 1;
            if (impact)
            {
                projectileData.impacted = 1;
                projectileData.impactPos = queryResult.hitPoint;
                projectileData.impactNormal = queryResult.hitNormal;
                projectileData.impactTick = (int)GetSingleton<WorldTime>().Tick;

                // Owner can despawn while projectile is in flight, so we need to make sure we dont send non existing instigator
                var damageInstigator = EntityManager.Exists(projectileData.projectileOwner) ? projectileData.projectileOwner : Entity.Null;

                var collisionHit = queryResult.hitCollisionOwner != Entity.Null;
                if (collisionHit)
                {
                    /*
                    if (damageInstigator != Entity.Null)
                    {
                        if (EntityManager.HasComponent<DamageEvent>(queryResult.hitCollisionOwner))
                        {
                            var damageEventBuffer = EntityManager.GetBuffer<DamageEvent>(queryResult.hitCollisionOwner);
                            DamageEvent.AddEvent(damageEventBuffer, damageInstigator, projectileData.settings.impactDamage, projectileDir, projectileData.settings.impactImpulse);
                        }
                    }*/
                }
                /*
                if (projectileData.settings.splashDamage.radius > 0)
                {
                    if (damageInstigator != Entity.Null)
                    
                        var collisionMask = ~(1 << projectileData.teamId);
                        SplashDamageRequest.Create(PostUpdateCommands, query.hitCollisionTestTick, damageInstigator, queryResult.hitPoint, collisionMask, projectileData.settings.splashDamage);
                    
                   }  */


                newPosition = queryResult.hitPoint;
            }

            if (ProjectileModuleServer.drawDebug.IntValue == 1)
            {
                var color = impact ? Color.red : Color.green;
                Debug.DrawLine(projectileData.position, newPosition, color, 2);
            //    DebugDraw.Sphere(newPosition, 0.1f, color, impact ? 2 : 0);
            }

            projectileData.position = newPosition;
            PostUpdateCommands.SetComponent(entityArray[i],projectileData);
        }

        entityArray.Dispose();
        projectileDataArray.Dispose();
    }
}


[DisableAutoCreation]
public class DespawnProjectiles :ComponentSystem
{
    EntityQuery ProjectileGroup;

    public DespawnProjectiles() { }

    protected override void OnCreate()
    {
        base.OnCreateManager();
        ProjectileGroup = GetEntityQuery(typeof(ProjectileData));
    }
    
    protected override void OnUpdate()
    {
        var worldTime = GetSingleton<WorldTime>();
        var entityArray = ProjectileGroup.ToEntityArray(Allocator.Persistent);
        var projectileDataArray = ProjectileGroup.ToComponentDataArray<ProjectileData>(Allocator.Persistent);
        for (var i = 0; i < projectileDataArray.Length; i++)
        {
            var projectileData = projectileDataArray[i];
            
            if (projectileData.impactTick > 0)
            {
                if (worldTime.GameTick.DurationSinceTick(projectileData.impactTick) > 1.0f)
                {
                    PostUpdateCommands.AddComponent(entityArray[i],new Despawn());
                }
                continue;
            }

            var age = worldTime.GameTick.DurationSinceTick(projectileData.startTick);
            var toOld = age > projectileData.maxAge;
            if (toOld)
            {
                PostUpdateCommands.AddComponent(entityArray[i],new Despawn());
            }
        }
        entityArray.Dispose();
        projectileDataArray.Dispose();
    }
}



