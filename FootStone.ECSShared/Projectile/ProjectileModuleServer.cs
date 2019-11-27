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

        m_handleRequests = World.Active.CreateSystem<HandleServerProjectileRequests>(resourceSystem);
        m_CreateMovementQueries = World.Active.CreateSystem<CreateProjectileMovementCollisionQueries>();
        m_HandleMovementQueries = World.Active.CreateSystem<HandleProjectileMovementCollisionQuery>();
        m_DespawnProjectiles = World.Active.CreateSystem<DespawnProjectiles>();
    }

    public void Shutdown()
    {
        World.Active.DestroySystem(m_handleRequests);
        World.Active.DestroySystem(m_CreateMovementQueries);
        World.Active.DestroySystem(m_HandleMovementQueries);
        World.Active.DestroySystem(m_DespawnProjectiles);
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
