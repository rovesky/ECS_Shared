using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace FootStone.Kitchen
{
    public class EntityTracker : MonoBehaviour, IReceiveEntity
    {
        public Entity EntityToTrack = Entity.Null;

        public void SetReceivedEntity(Entity entity)
        {
            EntityToTrack = entity;
        }

        // Update is called once per frame
        //void LateUpdate()
        //{
        //    return;
        //    if (EntityToTrack != Entity.Null)
        //    {
        //        try
        //        {
        //            var em = World.Active.EntityManager;

        //            transform.position = em.GetComponentData<Translation>(EntityToTrack).Value;
        //            transform.rotation = em.GetComponentData<Rotation>(EntityToTrack).Value;

        //           //  Debug.Log($"EntityTracker,x:{transform.position.x},y:{transform.position.y},z:{transform.position.z}");
        //        }
        //        catch
        //        {
        //            // Debug.LogError("EntityTracker catch!");
        //            // Dirty way to check for an Entity that no longer exists.
        //            EntityToTrack = Entity.Null;

        //            Destroy(this.gameObject);
        //        }
        //    }
        //}
    }
}