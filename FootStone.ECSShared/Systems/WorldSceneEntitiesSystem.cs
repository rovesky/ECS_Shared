using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;

namespace FootStone.ECS
{
    [DisableAutoCreation]
    public class WorldSceneEntitiesSystem : ComponentSystem
    {
        

        protected override void OnCreate()
        {
            SetEntitiesId();
        }

        private void SetEntitiesId()
        {
            var query = GetEntityQuery(typeof(ReplicatedEntityData));
            var entities = query.ToEntityArray(Allocator.TempJob);
            for (var i = 0; i < entities.Length; ++i)
            {
                var entity = entities[i];
                var replicatedEntityData = EntityManager.GetComponentData<ReplicatedEntityData>(entity);
                replicatedEntityData.Id = i;
                EntityManager.SetComponentData(entity, replicatedEntityData);
            }
        }

        protected override void OnDestroy()
        {
        
        }

        protected override void OnUpdate()
        {
          
        }

      
    }
}