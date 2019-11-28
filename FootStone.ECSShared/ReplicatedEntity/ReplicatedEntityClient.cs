using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace FootStone.ECS
{
    public class ReplicatedEntityClient : ISnapshotConsumer
    {
        public ReplicatedEntityClient(World world)
        {
            this.world = world;
            m_entityCollection = new ReplicatedEntityCollection(world.EntityManager);
            factoryManager = new ReplicatedEntityFactoryManager();

            //      m_world = world;
            //     m_resourceSystem = resourceSystem;
            //    m_assetRegistry = resourceSystem.GetResourceRegistry<ReplicatedEntityRegistry>();


            //   m_UpdateReplicatedOwnerFlag = m_world.GetECSWorld().CreateManager<UpdateReplicatedOwnerFlag>(m_world);

            // Load all replicated entity resources
            //     m_assetRegistry.LoadAllResources(resourceSystem);

            //if (world.SceneRoot != null)
            //{
            //    m_SystemRoot = new GameObject("ReplicatedEntitySystem");
            //    m_SystemRoot.transform.SetParent(world.SceneRoot.transform);
            //}
        }

        public void Shutdown()
        {
          //  m_world.GetECSWorld().DestroyManager(m_UpdateReplicatedOwnerFlag);

          //  if (m_SystemRoot != null)
             //   GameObject.Destroy(m_SystemRoot);
        }

        public void ProcessEntitySpawn(int servertick, int id, ushort typeId)
        {
            if (m_showInfo.IntValue > 0)
                GameDebug.Log("ProcessEntitySpawns. Server tick:" + servertick + " id:" + id + " typeid:" + typeId);

            Profiler.BeginSample("ReplicatedEntitySystemClient.ProcessEntitySpawns()");

            var factory = factoryManager.GetFactory(typeId);
            if (factory == null)
                return;
            var entity = factory.Create(world.EntityManager, null, null);
            if (entity == Entity.Null)
                return;

            var replicatedDataEntity = world.EntityManager.GetComponentData<ReplicatedEntityData>(entity);
            replicatedDataEntity.Id = id;
            world.EntityManager.SetComponentData(entity, replicatedDataEntity);

            m_entityCollection.Register(id, entity);

            Profiler.EndSample();

        }

        public void ProcessEntityUpdate(int serverTick, int id, ref NetworkReader reader)
        {
            if (m_showInfo.IntValue > 1)
                GameDebug.Log("ApplyEntitySnapshot. ServerTick:" + serverTick + " entityId:" + id);

            m_entityCollection.ProcessEntityUpdate(serverTick, id, ref reader);
        }

        public void ProcessEntityDespawns(int serverTime, List<int> despawns)
        {
            if (m_showInfo.IntValue > 0)
                GameDebug.Log("ProcessEntityDespawns. Server tick:" + serverTime + " ids:" + string.Join(",", despawns));

            //foreach (var id in despawns)
            //{
            //    var entity = m_entityCollection.Unregister(id);

            //    if (m_world.GetEntityManager().HasComponent<ReplicatedEntity>(entity))
            //    {
            //        var replicatedEntity = m_world.GetEntityManager().GetComponentObject<ReplicatedEntity>(entity);
            //        m_world.RequestDespawn(replicatedEntity.gameObject);
            //        continue;
            //    }

            //    m_world.RequestDespawn(entity);
            //}
        }

        public void Rollback()
        {
            m_entityCollection.Rollback();
        }

        public void Interpolate(GameTick time)
        {
            m_entityCollection.Interpolate(time);
        }

        //public void SetLocalPlayerId(int id)
        //{
        //    m_UpdateReplicatedOwnerFlag.SetLocalPlayerId(id);
        //}

        //public void UpdateControlledEntityFlags()
        //{
        //    m_UpdateReplicatedOwnerFlag.Update();
        //}

//#if UNITY_EDITOR

//        public int GetEntityCount()
//        {
//            return m_entityCollection.GetEntityCount();
//        }

//        public int GetSampleCount()
//        {
//            return m_entityCollection.GetSampleCount();
//        }

//        public int GetSampleTick(int sampleIndex)
//        {
//            return m_entityCollection.GetSampleTick(sampleIndex);
//        }

//        public int GetLastServerTick(int sampleIndex)
//        {
//            return m_entityCollection.GetLastServerTick(sampleIndex);
//        }

//        public int GetNetIdFromEntityIndex(int entityIndex)
//        {
//            return m_entityCollection.GetNetIdFromEntityIndex(entityIndex);
//        }

//        public ReplicatedEntityCollection.ReplicatedData GetReplicatedDataForNetId(int netId)
//        {
//            return m_entityCollection.GetReplicatedDataForNetId(netId);
//        }


//        public void StorePredictedState(int predictedTick, int finalTick)
//        {
//            m_entityCollection.StorePredictedState(predictedTick, finalTick);
//        }

//        public void FinalizedStateHistory(int tick, int lastServerTick, ref UserCommand command)
//        {
//            m_entityCollection.FinalizedStateHistory(tick, lastServerTick, ref command);
//        }

//        public int FindSampleIndexForTick(int tick)
//        {
//            return m_entityCollection.FindSampleIndexForTick(tick);
//        }

//        public bool IsPredicted(int entityIndex)
//        {
//            return m_entityCollection.IsPredicted(entityIndex);
//        }

//#endif

        private readonly World world;
        private readonly ReplicatedEntityCollection m_entityCollection;
        private readonly ReplicatedEntityFactoryManager factoryManager;

        //   readonly GameObject m_SystemRoot;
        //  readonly BundledResourceManager m_resourceSystem;
        // readonly ReplicatedEntityRegistry m_assetRegistry;

        //  readonly UpdateReplicatedOwnerFlag m_UpdateReplicatedOwnerFlag;

        [ConfigVar(Name = "replicatedentity.showclientinfo", DefaultValue = "0", Description = "Show replicated system info")]
        static ConfigVar m_showInfo;
    }

}