using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Profiling;
using FootStone.ECS;

public class ProjectileModuleServer 
{
    [ConfigVar(Name = "projectile.drawserverdebug", DefaultValue = "0", Description = "Show projectilesystem debug")]
    public static ConfigVar drawDebug;
    
    public ProjectileModuleServer(GameWorld gameWorld, BundledResourceManager resourceSystem)
    {
        m_GameWorld = gameWorld;

        m_handleRequests = World.DefaultGameObjectInjectionWorld.CreateSystem<HandleServerProjectileRequests>(resourceSystem);
        m_CreateMovementQueries = World.DefaultGameObjectInjectionWorld.CreateSystem<CreateProjectileMovementCollisionQueries>();
        m_HandleMovementQueries = World.DefaultGameObjectInjectionWorld.CreateSystem<HandleProjectileMovementCollisionQuery>();
        m_DespawnProjectiles = World.DefaultGameObjectInjectionWorld.CreateSystem<DespawnProjectiles>();
    }

    public void Shutdown()
    {
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_handleRequests);
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_CreateMovementQueries);
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_HandleMovementQueries);
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_DespawnProjectiles);
    }

    public void HandleRequests()
    {
        Profiler.BeginSample("ProjectileModuleServer.CreateMovementQueries");
        
        m_handleRequests.Update();
        
        Profiler.EndSample();
    }

   
    public void MovementStart()
    {
        Profiler.BeginSample("ProjectileModuleServer.CreateMovementQueries");
        
        m_CreateMovementQueries.Update();
        
        Profiler.EndSample();
    }

    public void MovementResolve()
    {
        Profiler.BeginSample("ProjectileModuleServer.HandleMovementQueries");
        
        m_HandleMovementQueries.Update();
        m_DespawnProjectiles.Update();
        
        Profiler.EndSample();
    }

    readonly GameWorld m_GameWorld;
    readonly HandleServerProjectileRequests m_handleRequests;
    readonly CreateProjectileMovementCollisionQueries m_CreateMovementQueries;
    readonly HandleProjectileMovementCollisionQuery m_HandleMovementQueries;
    readonly DespawnProjectiles m_DespawnProjectiles;

}
