using FootStone.ECS;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;







public class ProjectileModuleClient 
{
    [ConfigVar(Name = "projectile.logclientinfo", DefaultValue = "0", Description = "Show projectilesystem info")]
    public static ConfigVar logInfo;
    
    [ConfigVar(Name = "projectile.drawclientdebug", DefaultValue = "0", Description = "Show projectilesystem debug")]
    public static ConfigVar drawDebug;
    
    
    public ProjectileModuleClient(GameWorld world, BundledResourceManager resourceSystem)
    {
        m_world = world;
        
        //if (world.SceneRoot != null)
        //{
        //    m_SystemRoot = new GameObject("ProjectileSystem");
        //    m_SystemRoot.transform.SetParent(world.SceneRoot.transform);
        //}
        
        m_settings = Resources.Load<ProjectileModuleSettings>("ProjectileModuleSettings");

        m_clientProjectileFactory = new ClientProjectileFactory(m_world, World.DefaultGameObjectInjectionWorld.EntityManager, m_SystemRoot, resourceSystem);
        
        m_handleRequests = World.DefaultGameObjectInjectionWorld.CreateSystem<HandleClientProjectileRequests>(resourceSystem, m_SystemRoot, m_clientProjectileFactory);
        m_handleProjectileSpawn = World.DefaultGameObjectInjectionWorld.CreateSystem<HandleProjectileSpawn>(m_SystemRoot, resourceSystem, m_clientProjectileFactory);
        m_removeMispredictedProjectiles = World.DefaultGameObjectInjectionWorld.CreateSystem<RemoveMispredictedProjectiles>();
        m_despawnClientProjectiles = World.DefaultGameObjectInjectionWorld.CreateSystem<DespawnClientProjectiles>( m_clientProjectileFactory);
        m_CreateProjectileMovementQueries = World.DefaultGameObjectInjectionWorld.CreateSystem<CreateProjectileMovementCollisionQueries>();
        m_HandleProjectileMovementQueries = World.DefaultGameObjectInjectionWorld.CreateSystem<HandleProjectileMovementCollisionQuery>();
        m_updateClientProjectilesPredicted = World.DefaultGameObjectInjectionWorld.CreateSystem<UpdateClientProjectilesPredicted>();
        m_updateClientProjectilesNonPredicted = World.DefaultGameObjectInjectionWorld.CreateSystem<UpdateClientProjectilesNonPredicted>();
    }

    public void Shutdown()
    {
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_handleRequests);
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_handleProjectileSpawn);
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_removeMispredictedProjectiles);
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_despawnClientProjectiles);
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_CreateProjectileMovementQueries);
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_HandleProjectileMovementQueries);
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_updateClientProjectilesPredicted);
        World.DefaultGameObjectInjectionWorld.DestroySystem(m_updateClientProjectilesNonPredicted);

    
        if(m_SystemRoot != null)
            Object.Destroy(m_SystemRoot);
        
        Resources.UnloadAsset(m_settings);
    }
        
    public void StartPredictedMovement()
    {
        m_CreateProjectileMovementQueries.Update();
    }

    
    public void FinalizePredictedMovement()
    {
        m_HandleProjectileMovementQueries.Update();
    }

    public void HandleProjectileSpawn()
    {
        m_handleProjectileSpawn.Update();
        m_removeMispredictedProjectiles.Update();
    }

    public void HandleProjectileDespawn()
    {
        m_despawnClientProjectiles.Update();
    }

    
    public void HandleProjectileRequests()
    {
        m_handleRequests.Update();
    }
    
    public void UpdateClientProjectilesNonPredicted()
    {
        m_updateClientProjectilesNonPredicted.Update();
    }

    public void UpdateClientProjectilesPredicted()
    {
        m_updateClientProjectilesPredicted.Update();
    }

    readonly GameWorld m_world;
    readonly GameObject m_SystemRoot;
    readonly ProjectileModuleSettings m_settings;

    readonly ClientProjectileFactory m_clientProjectileFactory;
    
    readonly HandleClientProjectileRequests m_handleRequests;
    readonly CreateProjectileMovementCollisionQueries m_CreateProjectileMovementQueries;
    readonly HandleProjectileMovementCollisionQuery m_HandleProjectileMovementQueries;

    readonly HandleProjectileSpawn m_handleProjectileSpawn;
    readonly RemoveMispredictedProjectiles m_removeMispredictedProjectiles;
    readonly DespawnClientProjectiles m_despawnClientProjectiles;
    readonly UpdateClientProjectilesNonPredicted m_updateClientProjectilesNonPredicted;
    readonly UpdateClientProjectilesPredicted m_updateClientProjectilesPredicted;
}
