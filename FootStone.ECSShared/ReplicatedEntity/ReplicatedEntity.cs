using System;
using Unity.Entities;


namespace FootStone.ECS
{
  
    public struct ReplicatedEntityData : IComponentData, IReplicatedState
    {
        public WeakAssetReference AssetGuid; // Guid of asset this entity is created from
        public long NetId;
        [NonSerialized] public int Id;
        [NonSerialized] public int PredictingPlayerId;

        public ReplicatedEntityData(WeakAssetReference guid)
        {
            AssetGuid = guid;
            NetId = -1;
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



    [DisableAutoCreation]
    public class UpdateReplicatedOwnerFlag : ComponentSystem
    {
        private int localPlayerId;

        public void SetLocalPlayerId(int playerId)
        {
            localPlayerId = playerId;
        }

        protected override void OnCreate()
        {
            localPlayerId = -1;
        }

        protected override void OnUpdate()
        {
           // FSLog.Info($"UpdateReplicatedOwnerFlag OnUpdate");

            Entities.ForEach((Entity entity,ref ReplicatedEntityData replicatedEntityData) =>
            {
             //   FSLog.Info($"localPlayerId:{localPlayerId},PredictingPlayerId:{replicatedEntityData.PredictingPlayerId}，id:{replicatedEntityData.Id}");
                //var locallyControlled = localPlayerId == -1 ||
                //                        replicatedEntityData.PredictingPlayerId == localPlayerId ||
                //                        replicatedEntityData.PredictingPlayerId == -1 ;

                var locallyControlled = true;

                SetFlagAndChildFlags(entity, locallyControlled);
            });
        }

        private void SetFlagAndChildFlags(Entity entity, bool set)
        {
            SetFlag(entity, set);

            if (!EntityManager.HasComponent<EntityGroupChildren>(entity)) 
                return;

            var buffer = EntityManager.GetBuffer<EntityGroupChildren>(entity);
            for (var i = 0; i < buffer.Length; i++)
            {
                SetFlag(buffer[i].Entity, set);
            }
        }

        private void SetFlag(Entity entity, bool set)
        {
            var flagSet = EntityManager.HasComponent<ServerEntity>(entity);
            if (flagSet == set) return;

            if (set)
                PostUpdateCommands.AddComponent(entity, new ServerEntity());
            else
                PostUpdateCommands.RemoveComponent<ServerEntity>(entity);
        }
    }
}