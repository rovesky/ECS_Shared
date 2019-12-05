using FootStone.ECS;
using Unity.Entities;
using UnityEngine;

namespace FootStone.Kitchen
{
    public class ReplicatedEntityDataBehaviour : MonoBehaviour, IConvertGameObjectToEntity
    {

        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager,
            GameObjectConversionSystem conversionSystem)
        {

            dstManager.AddComponentData(entity, new ReplicatedEntityData
            {
                Id = -1,
                PredictingPlayerId = -1
            });

        }
    }
}
