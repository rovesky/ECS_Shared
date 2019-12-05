using FootStone.ECS;
using Unity.Entities;
using UnityEngine;

namespace FootStone.Kitchen
{
    [DisableAutoCreation]
    public class DespawnSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref Despawn despawn) =>
            {
                if (despawn.Frame <= 0)
                {
                    if (EntityManager.HasComponent<Transform>(entity))
                    {
                        Object.Destroy(EntityManager.GetComponentObject<Transform>(entity).gameObject);
                    }

                    EntityManager.DestroyEntity(entity);
                  
                }

                despawn.Frame--;
            });
        }
    }
}