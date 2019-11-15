using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.ECS
{

    public struct EntityPredictData : IComponentData
    {
        public float3 position;
        public quaternion rotation;
        public Entity pickupEntity;

        //public override bool Equals(object obj)
        //{
        //    var minValue = 0.001;
        //    var other = (EntityPredictData)obj;
        //    return Mathf.Abs(this.position.x - other.position.x) < minValue &&
        //           Mathf.Abs(this.position.y - other.position.y) < minValue &&
        //           Mathf.Abs(this.position.z - other.position.z) < minValue;

        //}

        public void Interpolate(ref EntityPredictData prevState, ref EntityPredictData nextState, float interpVal)
        {
            position = Vector3.Lerp(prevState.position, nextState.position, interpVal);
            rotation = Quaternion.Lerp(prevState.rotation, nextState.rotation, interpVal);

        }
    }

}