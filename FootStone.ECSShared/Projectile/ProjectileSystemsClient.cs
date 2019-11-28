﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
//using UnityEditor.Experimental.Rendering;
using UnityEngine.Profiling;
using FootStone.ECS;



// Added to projectiles that are created locally. System attempts to find a mathcing projectile comming from server.
// If no match is found when server version should have been found the predicted projectile is deleted.
public struct PredictedProjectile : IComponentData  
{
    public PredictedProjectile(int startTick)
    {
        this.startTick = startTick;
    }
   
    public int startTick;                    
}

// Added to projectiles that has a clientprojectile 
public struct ClientProjectileOwner : IComponentData
{
    public Entity clientProjectile;
}

[DisableAutoCreation]
public class HandleClientProjectileRequests :ComponentSystem
{
    EntityQuery RequestGroup;
    readonly GameObject m_SystemRoot;
    readonly BundledResourceManager m_resourceSystem;
    readonly ProjectileModuleSettings m_settings;
    ClientProjectileFactory m_clientProjectileFactory;
    List<ProjectileRequest> requestBuffer = new List<ProjectileRequest>(16);

    public HandleClientProjectileRequests(BundledResourceManager resourceSystem, GameObject systemRoot, 
        ClientProjectileFactory clientProjectileFactory) 
    {
        m_resourceSystem = resourceSystem;
        m_SystemRoot = systemRoot;
        m_settings = Resources.Load<ProjectileModuleSettings>("ProjectileModuleSettings");
        m_clientProjectileFactory = clientProjectileFactory;
    }

    protected override void OnCreate()
    {
        base.OnCreateManager();
        RequestGroup = GetEntityQuery(typeof(ProjectileRequest));
    }

    protected override void OnDestroy()
    {
        base.OnDestroyManager();
        Resources.UnloadAsset(m_settings);
    }

    protected override void OnUpdate()
    {
        if (RequestGroup.CalculateEntityCount() == 0)
            return;

        // Copy requests as spawning will invalidate Group 
        requestBuffer.Clear();
        var requestArray = RequestGroup.ToComponentDataArray<ProjectileRequest>(Allocator.Persistent);
        var requestEntityArray = RequestGroup.ToEntityArray(Allocator.Persistent);
        for (var i = 0; i < requestArray.Length; i++)
        {
            requestBuffer.Add(requestArray[i]);
            PostUpdateCommands.DestroyEntity(requestEntityArray[i]);
        }

        // Handle requests
        var projectileRegistry = m_resourceSystem.GetResourceRegistry<ProjectileRegistry>();
        foreach (var request in requestBuffer)
        {
            var registryIndex = projectileRegistry.FindIndex(request.projectileAssetGuid);
            if (registryIndex == -1)
            {
                GameDebug.LogError("Cant find asset guid in registry");
                continue;
            }

            // Create projectile and initialize
            var projectileEntity = m_settings.projectileFactory.Create(EntityManager, m_resourceSystem, null);
            var projectileData = EntityManager.GetComponentData<ProjectileData>(projectileEntity);
            
            projectileData.SetupFromRequest(request, registryIndex);
            projectileData.Initialize( projectileRegistry);
            EntityManager.SetComponentData(projectileEntity, projectileData);
            EntityManager.AddComponentData(projectileEntity, new PredictedProjectile(request.startTick));
            EntityManager.AddComponentData(projectileEntity, new UpdateProjectileFlag());

            if (ProjectileModuleClient.logInfo.IntValue > 0)
                GameDebug.Log("New predicted projectile created: " + projectileEntity);

            // Create client projectile
            var clientProjectileEntity = m_clientProjectileFactory.CreateClientProjectile(projectileEntity);
            EntityManager.AddComponentData(clientProjectileEntity, new UpdateProjectileFlag());
            
            if (ProjectileModuleClient.drawDebug.IntValue == 1)
            {
                Debug.DrawLine(projectileData.startPos, projectileData.endPos, Color.cyan, 1.0f);
            }
        }

        requestArray.Dispose();
        requestEntityArray.Dispose();
    }
}


class ProjectilesSystemsClient
{
    public static void Update(GameWorld world, EntityCommandBuffer commandBuffer, ClientProjectile clientProjectile)
    {
//      //  var worldTime = World
//        var deltaTime = world.frameDuration;
//        if (clientProjectile.impacted)
//            return;
        
//        // When projectile disappears we hide clientprojectile
//        if (clientProjectile.projectile == Entity.Null)
//        {
//            clientProjectile.SetVisible(false);
//            return;
//        }

//        var projectileData = World.Active.EntityManager.GetComponentData<ProjectileData>(clientProjectile.projectile);
//        var aliveDuration = world.worldTime.DurationSinceTick(projectileData.startTick);

//        // Interpolation delay can cause projectiles to be spawned before they should be shown.  
//        if (aliveDuration < 0)
//        {
//            return;
//        }
        
//        if (!clientProjectile.IsVisible)
//        {
//            clientProjectile.SetVisible(true);
//        }

//        var dir = Vector3.Normalize(projectileData.endPos - projectileData.startPos);

        
//        var moveDist = aliveDuration * projectileData.settings.velocity;
//        var pos = (Vector3)projectileData.startPos + dir * moveDist;
//        var rot = Quaternion.LookRotation(dir);

//        var worldOffset = Vector3.zero;
//        if (clientProjectile.offsetScale > 0.0f)
//        {
//            clientProjectile.offsetScale -= deltaTime / clientProjectile.offsetScaleDuration;       
//            worldOffset = rot * clientProjectile.startOffset * clientProjectile.offsetScale;
//        }

//        if (projectileData.impacted == 1 && !clientProjectile.impacted)
//        {
//            clientProjectile.impacted = true;
//            /*
//            if (clientProjectile.impactEffect != null)
//            {
//                world.GetECSWorld().GetExistingManager<HandleSpatialEffectRequests>().Request(clientProjectile.impactEffect, 
//                    projectileData.impactPos, Quaternion.LookRotation(projectileData.impactNormal));
//            }*/

//            clientProjectile.SetVisible(false);
//        }

        
//        clientProjectile.transform.position = pos + worldOffset;
        
////        Debug.DrawLine(projectileData.startPos, pos, Color.red);
////        DebugDraw.Sphere(clientProjectile.transform.position, 0.2f, Color.red);

//        clientProjectile.roll += deltaTime * clientProjectile.rotationSpeed;    
//        var roll = Quaternion.Euler(0, 0, clientProjectile.roll);
//        clientProjectile.transform.rotation = rot * roll;
    
//        //if (ProjectileModuleClient.drawDebug.IntValue == 1)
//        //{
//        //    DebugDraw.Sphere(clientProjectile.transform.position, 0.1f, Color.cyan, 1.0f);
//        //}
    }
}


[DisableAutoCreation]
public class UpdateClientProjectilesPredicted : ComponentSystem
{
    //public UpdateClientProjectilesPredicted() 
    //{
    //    ExtraComponentRequirements = new [] { ComponentType.Create<UpdateProjectileFlag>() };
    //}

    protected override void OnUpdate()
    {
      //  ProjectilesSystemsClient.Update(m_world, PostUpdateCommands, clientProjectile);
    }

    //protected override void Update(Entity entity, ClientProjectile clientProjectile)
    //{
       
    //}
}

[DisableAutoCreation]
public class UpdateClientProjectilesNonPredicted : ComponentSystem
{
    //public UpdateClientProjectilesNonPredicted(GameWorld world) : base(world)
    //{
    //    ExtraComponentRequirements = new [] { ComponentType.Exclude<UpdateProjectileFlag>() };
    //}

    protected override void OnUpdate()
    {
        throw new System.NotImplementedException();
    }

    //protected override void Update(Entity entity, ClientProjectile clientProjectile)
    //{
    //    ProjectilesSystemsClient.Update(m_world, PostUpdateCommands, clientProjectile);
    //}
}


[DisableAutoCreation]
[AlwaysUpdateSystemAttribute]
public class HandleProjectileSpawn : ComponentSystem
{
    readonly GameObject m_SystemRoot;
    readonly BundledResourceManager m_resourceSystem;

    EntityQuery PredictedProjectileGroup;
    EntityQuery IncommingProjectileGroup;

    private ClientProjectileFactory m_clientProjectileFactory;
    private List<Entity> addClientProjArray = new List<Entity>(32);

    public HandleProjectileSpawn(GameObject systemRoot, BundledResourceManager resourceSystem, ClientProjectileFactory projectileFactory) 
    {
        m_SystemRoot = systemRoot;
        m_resourceSystem = resourceSystem;
        m_clientProjectileFactory = projectileFactory;
    }
    
    protected override void OnCreate()
    {
        base.OnCreate();

        
        PredictedProjectileGroup = GetEntityQuery(typeof(ProjectileData), typeof(PredictedProjectile), ComponentType.Exclude<Despawn>());
        IncommingProjectileGroup = GetEntityQuery(typeof(ProjectileData), ComponentType.Exclude<ClientProjectileOwner>());
    }
    
    
    protected override void OnUpdate()
    {

        if (IncommingProjectileGroup.CalculateEntityCount() > 0)
            HandleIncommingProjectiles();
    }
    
    void HandleIncommingProjectiles()
    {
        // Run through all incomming projectiles. Attempt to match with predicted projectiles.
        // If none is found add to addClientProjArray so a client projectile will be created for projectile
        addClientProjArray.Clear();

        var inEntityArray = IncommingProjectileGroup.ToEntityArray(Allocator.Persistent);
        var inProjectileDataArray = IncommingProjectileGroup.ToComponentDataArray<ProjectileData>(Allocator.Persistent);
        
        var predictedProjectileArray = PredictedProjectileGroup.ToComponentDataArray<ProjectileData>(Allocator.Persistent);
        var predictedProjectileEntities = PredictedProjectileGroup.ToEntityArray(Allocator.Persistent);
        for(var j=0;j<inProjectileDataArray.Length;j++)
        {
            var inProjectileData = inProjectileDataArray[j];
            var inProjectileEntity = inEntityArray[j];
            if (ProjectileModuleClient.logInfo.IntValue > 0)
                GameDebug.Log(string.Format("Projectile spawn:" + inProjectileEntity));

//            var inProjectileEntity = inProjectile.GetComponent<GameObjectEntity>().Entity;

            // Initialze new projectile with correct settings
            var projectileRegistry = m_resourceSystem.GetResourceRegistry<ProjectileRegistry>();
            inProjectileData.Initialize( projectileRegistry);
            EntityManager.SetComponentData(inProjectileEntity, inProjectileData);
            
            // I new projectile in not predicted, attempt to find a predicted that that should link to it 
            var matchFound = false;
            if (!EntityManager.HasComponent<PredictedProjectile>(inProjectileEntity))
            {
                for (var i = 0; i < predictedProjectileEntities.Length; i++)
                {
                    var predictedProjetile = predictedProjectileArray[i];
                    
                    // Attempt to find matching 
                    if (predictedProjetile.projectileTypeRegistryIndex !=
                        inProjectileData.projectileTypeRegistryIndex)
                        continue;
                    if (predictedProjetile.projectileOwner != inProjectileData.projectileOwner)
                        continue;
                    if (predictedProjetile.startTick != inProjectileData.startTick)
                        continue;
                    if (math.distance(predictedProjetile.startPos, inProjectileData.startPos) > 0.1f)
                        continue;
                    if (math.distance(predictedProjetile.endPos, inProjectileData.endPos) > 0.1f)
                        continue;

                    // Match found
                    matchFound = true;
                    var predictedProjectileEntity = predictedProjectileEntities[i];
                    if (ProjectileModuleClient.logInfo.IntValue > 0)
                        GameDebug.Log("ProjectileSystemClient. Predicted projectile" + predictedProjectileEntity + " matched with " + inProjectileEntity + " from server. startTick:" +
                                      inProjectileData.startTick);
                    
                    // Reassign clientprojectile to use new projectile
                    var clientProjectileOwner =
                        EntityManager.GetComponentData<ClientProjectileOwner>(predictedProjectileEntity);
                    var clientProjectile = 
                        EntityManager.GetComponentObject<ClientProjectile>(clientProjectileOwner.clientProjectile);
                    clientProjectile.projectile = inProjectileEntity;
                    PostUpdateCommands.AddComponent(inProjectileEntity,clientProjectileOwner);
                    PostUpdateCommands.AddComponent(inProjectileEntity, new UpdateProjectileFlag());
                    
                    // Destroy predicted
                    if (ProjectileModuleClient.logInfo.IntValue > 0)
                        GameDebug.Log("ProjectileSystemClient. Destroying predicted:" + predictedProjectileEntity);

                    PostUpdateCommands.RemoveComponent(predictedProjectileEntity,typeof(ClientProjectileOwner));
                    PostUpdateCommands.RemoveComponent(predictedProjectileEntity,typeof(UpdateProjectileFlag));
                 //   m_world.RequestDespawn(PostUpdateCommands, predictedProjectileEntity);
                    break;
                }
            }
            
            //if (ProjectileModuleClient.drawDebug.IntValue == 1)
            //{
            //    var color = matchFound ? Color.green : Color.yellow;
            //    DebugDraw.Sphere(inProjectileData.startPos, 0.12f, color, 1.0f);
            //    Debug.DrawLine(inProjectileData.startPos, inProjectileData.endPos, color, 1.0f);
            //}
            
            // If match was found the new projectile has already been assigned an existing clientprojectile
            if (!matchFound)
                addClientProjArray.Add(inProjectileEntity);
        }
        
        // Create client projectiles. This is deferred as we cant create
        // clientprojectiles while iterating over componentarray 
        foreach (var projectileEntity in addClientProjArray)
        {
            if (ProjectileModuleClient.logInfo.IntValue > 0)
            {
                var projectileData = EntityManager.GetComponentData<ProjectileData>(projectileEntity);
                GameDebug.Log("Creating clientprojectile for projectile:" + projectileData);
            }
            
            m_clientProjectileFactory.CreateClientProjectile(projectileEntity);
        }

        inEntityArray.Dispose();
        inProjectileDataArray.Dispose();

        predictedProjectileArray.Dispose();
        predictedProjectileEntities.Dispose();

    }
}


[DisableAutoCreation]
[AlwaysUpdateSystemAttribute]
public class RemoveMispredictedProjectiles : ComponentSystem
{
    EntityQuery PredictedProjectileGroup;
   

    protected override void OnCreate()
    {
        base.OnCreate();
        PredictedProjectileGroup = GetEntityQuery(typeof(PredictedProjectile), ComponentType.Exclude<Despawn>());
    }

    protected override void OnUpdate()
    {
        // Remove all predicted projectiles that should have been acknowledged by now
        var predictedProjectileArray = PredictedProjectileGroup.ToComponentDataArray<PredictedProjectile>(Allocator.Persistent);
        var predictedProjectileEntityArray = PredictedProjectileGroup.ToEntityArray(Allocator.Persistent);
        for (var i=0;i<predictedProjectileArray.Length;i++)
        {
            var predictedEntity = predictedProjectileArray[i];

       //     if (predictedEntity.startTick >= m_world.lastServerTick)
          //      continue;

            var entity = predictedProjectileEntityArray[i]; 
            PostUpdateCommands.AddComponent(entity, new Despawn());
            
//            var gameObject = EntityManager.GetComponentObject<Transform>(predictedProjectileEntityArray[i]).gameObject;
//            m_world.RequestDespawn(gameObject, PostUpdateCommands);
                
            if (ProjectileModuleClient.logInfo.IntValue > 0)
                GameDebug.Log(string.Format("<color=red>Predicted projectile {0} destroyed as it was not verified. startTick:{1}]</color>", entity, predictedEntity.startTick));
        }

        predictedProjectileArray.Dispose();
        predictedProjectileEntityArray.Dispose();
    }
}


[DisableAutoCreation]
[AlwaysUpdateSystemAttribute]
public class DespawnClientProjectiles : ComponentSystem
{
    EntityQuery DespawningClientProjectileOwnerGroup;
    ClientProjectileFactory m_clientProjectileFactory;

    public DespawnClientProjectiles( ClientProjectileFactory clientProjectileFactory)        
    {
        m_clientProjectileFactory = clientProjectileFactory;
    }

    protected override void OnCreateManager()
    {
        base.OnCreateManager();

        DespawningClientProjectileOwnerGroup = GetEntityQuery(typeof(ClientProjectileOwner), typeof(Despawn));
    }

    List<Entity> clientProjectiles = new List<Entity>(32);
    protected override void OnUpdate()
    {
        // Remove all client projectiles that are has despawning projectile
        var clientProjectileOwnerArray = DespawningClientProjectileOwnerGroup.ToComponentDataArray<ClientProjectileOwner>(Allocator.Persistent);

        if (clientProjectileOwnerArray.Length > 0)
        {
            clientProjectiles.Clear();
            for(var i=0;i<clientProjectileOwnerArray.Length;i++)
            {
                var clientProjectileOwner = clientProjectileOwnerArray[i];
                clientProjectiles.Add(clientProjectileOwner.clientProjectile);
            }

            for (var i = 0; i < clientProjectiles.Count; i++)
            {
                m_clientProjectileFactory.DestroyClientProjectile(clientProjectiles[i], PostUpdateCommands);
                if (ProjectileModuleClient.logInfo.IntValue > 0)
                    GameDebug.Log(string.Format("Projectile despawned so despawn of clientprojectile requested"));
            }
        }

        clientProjectileOwnerArray.Dispose();
    }
}

public class ClientProjectileFactory
{
    class Pool
    {
        public int poolIndex;
        public GameObject prefab;
        public List<GameObjectEntity> instances = new List<GameObjectEntity>();
        public Queue<int> freeList = new Queue<int>();
    }

    private Pool[] pools;
    
    public ClientProjectileFactory(GameWorld world, EntityManager entityManager, GameObject systemRoot, BundledResourceManager resourceSystem)
    {
        m_world = world;
        m_entityManager = entityManager;
        m_resourceSystem = resourceSystem;
        m_systemRoot = systemRoot;

        var projectileRegistry = m_resourceSystem.GetResourceRegistry<ProjectileRegistry>();
        var typeCount = projectileRegistry.entries.Count;
        pools = new Pool[typeCount];
        for (var i = 0; i < projectileRegistry.entries.Count; i++)
        {
            var pool = new Pool();
            
            var entry = projectileRegistry.entries[i];
            pool.prefab = (GameObject)m_resourceSystem.GetSingleAssetResource(entry.definition.clientProjectilePrefab);
            pool.poolIndex = i;            
            Allocate(pool, entry.definition.clientProjectileBufferSize);
            pools[i] = pool;
        }
    }

    GameWorld m_world;
    EntityManager m_entityManager;
    GameObject m_systemRoot;
    BundledResourceManager m_resourceSystem;
    
    int Reserve(Pool pool)
    {
        if (pool.freeList.Count == 0)
            Allocate(pool, 5);

        var index = pool.freeList.Dequeue();
//        GameDebug.Log("Reserve pool:" + pool.poolIndex + " index:" + index + " count:" + pool.freeList.Count);
        return index;
    }

    void Free(Pool pool, int index)
    {
//        GameDebug.Log("Free pool:" + pool.poolIndex + " index:" + index + " before count:" + pool.freeList.Count);
        GameDebug.Assert(!pool.freeList.Contains(index));
        pool.freeList.Enqueue(index); 
    }
        
    void Allocate(Pool pool, int count)
    {
        var startIndex = pool.instances.Count;
        for (var i = 0; i < count; i++)
        {
            var bufferIndex = startIndex + i;
            var projectile = m_world.Spawn<GameObjectEntity>(pool.prefab); 
            pool.freeList.Enqueue(bufferIndex);
            if(m_systemRoot != null)
                projectile.transform.SetParent(m_systemRoot.transform);

            var entity = projectile.Entity;
            var clientProjectile = m_entityManager.GetComponentObject<ClientProjectile>(entity);
            clientProjectile.poolIndex = pool.poolIndex;
            clientProjectile.bufferIndex = bufferIndex;
            clientProjectile.SetVisible(false);
            clientProjectile.gameObject.SetActive(false);
            
            pool.instances.Add(projectile);
        }
    }
    
    public Entity CreateClientProjectile(Entity projectileEntity)
    {
        var projectileData = m_entityManager.GetComponentData<ProjectileData>(projectileEntity);

        // Create client projectile
        var pool = pools[projectileData.projectileTypeRegistryIndex];
        var instanceIndex = Reserve(pool);
        var gameObjectEntity = pool.instances[instanceIndex];
        gameObjectEntity.gameObject.SetActive(true);
        
        var clientProjectileEntity = gameObjectEntity.Entity;
        var clientProjectile = m_entityManager.GetComponentObject<ClientProjectile>(clientProjectileEntity);
        
        GameDebug.Assert(clientProjectile.projectile == Entity.Null,"Entity not null");
        clientProjectile.projectile = projectileEntity;
        clientProjectile.SetVisible(false);
        
        if (ProjectileModuleClient.logInfo.IntValue > 0)
            GameDebug.Log(string.Format("Creating clientprojectile {0} for projectile {1}]", clientProjectile, projectileEntity));
        
        // Add clientProjectileOwner to projectile
        var clientProjectileOwner = new ClientProjectileOwner
        {
            clientProjectile = clientProjectileEntity
        };
        m_entityManager.AddComponentData(projectileEntity,clientProjectileOwner);

        return clientProjectileEntity;
    }

    public void DestroyClientProjectile(Entity clientProjectileEntity, EntityCommandBuffer commandBuffer)
    {
        var clientProjectile = m_entityManager.GetComponentObject<ClientProjectile>(clientProjectileEntity);
        
        Free(pools[clientProjectile.poolIndex], clientProjectile.bufferIndex);
        
        clientProjectile.gameObject.SetActive(false);
        clientProjectile.Reset();
    }
}

