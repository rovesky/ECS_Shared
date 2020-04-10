using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace FootStone.ECS
{
    [DisableAutoCreation]
    public class WorldSceneEntitiesSystem : ComponentSystem
    {
        private bool isInit;

        public List<Entity> SceneEntities { get; private set; }

        protected override void OnCreate()
        {
            SceneEntities = new List<Entity>();
          
        }
        

        protected override void OnDestroy()
        {
        }

        protected override void OnUpdate()
        {
            if(isInit)
                return;
            isInit = true;

            var query = GetEntityQuery(typeof(ReplicatedEntityData));
            var entities = query.ToEntityArray(Allocator.TempJob);

           // FSLog.Info($"entities.Length:{entities.Length}");

            var entityList = entities.ToList();
            entityList.Sort((a, b) =>
            {
                var sa = EntityManager.GetComponentData<ReplicatedEntityData>(a).NetId;
                var sb = EntityManager.GetComponentData<ReplicatedEntityData>(b).NetId;

                return sa.CompareTo(sb);
            });


            for (var i = 0; i < entityList.Count; ++i)
            {
                var entity = entityList[i];
                var replicatedEntityData = EntityManager.GetComponentData<ReplicatedEntityData>(entity);
                replicatedEntityData.Id = i;
                EntityManager.SetComponentData(entity, replicatedEntityData);

                var trans = EntityManager.GetComponentData<Translation>(entity);
                FSLog.Info($"SceneEntities,id:{i},netId:{replicatedEntityData.NetId},trans:{trans.Value}");
            }

            SceneEntities.AddRange(entityList);
            FSLog.Info($"SceneEntities.count:{SceneEntities.Count}");

            entities.Dispose();
        }
    }
}