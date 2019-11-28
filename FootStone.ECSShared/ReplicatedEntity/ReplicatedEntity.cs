using System;
using Unity.Entities;


namespace FootStone.ECS
{
  
    public struct ReplicatedEntityData : IComponentData, IReplicatedState
    {
        public WeakAssetReference AssetGuid; // Guid of asset this entity is created from
        [NonSerialized] public int Id;
        [NonSerialized] public int PredictingPlayerId;

        public ReplicatedEntityData(WeakAssetReference guid)
        {
            AssetGuid = guid;
            Id = -1;
            PredictingPlayerId = -1;
        }

        public static IReplicatedStateSerializerFactory CreateSerializerFactory()
        {
            return new ReplicatedStateSerializerFactory<ReplicatedEntityData>();
        }

        public void Serialize(ref SerializeContext context, ref NetworkWriter writer)
        {
            writer.WriteInt32("predictingPlayerId", PredictingPlayerId);
        }

        public void Deserialize(ref SerializeContext context, ref NetworkReader reader)
        {
            PredictingPlayerId = reader.ReadInt32();
        }
    }


//    [ExecuteAlways]
//    [DisallowMultipleComponent]
//    [RequireComponent(typeof(GameObjectEntity))]
//    public class ReplicatedEntity : ComponentDataProxy<ReplicatedEntityData>
//    {
//        public byte[] netID; // guid of instance. Used for identifying replicated entities from the scene

//        private void Awake()
//        {
//            // Ensure replicatedEntityData is set to default
//            var val = Value;
//            val.Id = -1;
//            val.PredictingPlayerId = -1;
//            Value = val;
//#if UNITY_EDITOR
//            if (!EditorApplication.isPlaying)
//                SetUniqueNetID();
//#endif
//        }

//#if UNITY_EDITOR

//        public static Dictionary<byte[], ReplicatedEntity> netGuidMap =
//            new Dictionary<byte[], ReplicatedEntity>(new ByteArrayComp());

//        private void OnValidate()
//        {
//            if (EditorApplication.isPlaying)
//                return;

//            var prefabType = PrefabUtility.GetPrefabType(this);
//            if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab)
//                netID = null;
//            else
//                SetUniqueNetID();

//            UpdateAssetGuid();
//        }

//        public bool SetAssetGUID(string guidStr)
//        {
//            var guid = new WeakAssetReference(guidStr);
//            var val = Value;
//            var currentGuid = val.AssetGuid;
//            if (!guid.Equals(currentGuid))
//            {
//                val.AssetGuid = guid;
//                Value = val;
//                PrefabUtility.SavePrefabAsset(gameObject);
//                return true;
//            }

//            return false;
//        }

//        public void UpdateAssetGuid()
//        {
//            // Set type guid
//            var stage = PrefabStageUtility.GetPrefabStage(gameObject);
//            if (stage != null)
//            {
//                var guidStr = AssetDatabase.AssetPathToGUID(stage.prefabAssetPath);
//                if (SetAssetGUID(guidStr))
//                    EditorSceneManager.MarkSceneDirty(stage.scene);
//            }
//        }

//        private void SetUniqueNetID()
//        {
//            // Generate new if fresh object
//            if (netID == null || netID.Length == 0)
//            {
//                var guid = Guid.NewGuid();
//                netID = guid.ToByteArray();
//                EditorSceneManager.MarkSceneDirty(gameObject.scene);
//            }

//            // If we are the first add us
//            if (!netGuidMap.ContainsKey(netID))
//            {
//                netGuidMap[netID] = this;
//                return;
//            }


//            // Our guid is known and in use by another object??
//            var oldReg = netGuidMap[netID];
//            if (oldReg != null && oldReg.GetInstanceID() != GetInstanceID() &&
//                ByteArrayComp.instance.Equals(oldReg.netID, netID))
//            {
//                // If actually *is* another ReplEnt that has our netID, *then* we give it up (usually happens because of copy / paste)
//                netID = Guid.NewGuid().ToByteArray();
//                EditorSceneManager.MarkSceneDirty(gameObject.scene);
//            }

//            netGuidMap[netID] = this;
//        }

//#endif
//    }


//    [DisableAutoCreation]
//    public class UpdateReplicatedOwnerFlag : ComponentSystem
//    {
//        private bool m_initialized;
//        //  ComponentGroup RepEntityDataGroup;

//        private int m_localPlayerId;

//        protected override void OnCreate()
//        {
//            base.OnCreate();
//            //    RepEntityDataGroup = GetComponentGroup(typeof(ReplicatedEntityData));
//        }

//        public void SetLocalPlayerId(int playerId)
//        {
//            m_localPlayerId = playerId;
//            m_initialized = true;
//        }

//        protected override void OnUpdate()
//        {
//            //var entityArray = RepEntityDataGroup.GetEntityArray();
//            //var repEntityDataArray = RepEntityDataGroup.GetComponentDataArray<ReplicatedEntityData>();
//            //for (int i = 0; i < entityArray.Length; i++)
//            //{
//            //    var repDataEntity = repEntityDataArray[i];
//            //    var locallyControlled = m_localPlayerId == -1 || repDataEntity.predictingPlayerId == m_localPlayerId;

//            //    SetFlagAndChildFlags(entityArray[i], locallyControlled);
//            //}  
//        }

//        //    void SetFlagAndChildFlags(Entity entity, bool set)
//        //    {
//        //        SetFlag(entity, set);

//        //        if (EntityManager.HasComponent<EntityGroupChildren>(entity))
//        //        {
//        //            var buffer = EntityManager.GetBuffer<EntityGroupChildren>(entity);
//        //            for (int i = 0; i < buffer.Length; i++)
//        //            {
//        //                SetFlag(buffer[i].entity, set);
//        //            }
//        //        } 
//        //    }

//        //    void SetFlag(Entity entity, bool set)
//        //    {
//        //        var flagSet = EntityManager.HasComponent<ServerEntity>(entity);
//        //        if (flagSet != set)
//        //        {
//        //            if (set)
//        //                PostUpdateCommands.AddComponent(entity, new ServerEntity());
//        //            else
//        //                PostUpdateCommands.RemoveComponent<ServerEntity>(entity);
//        //        }  
//        //    }
//    }
}