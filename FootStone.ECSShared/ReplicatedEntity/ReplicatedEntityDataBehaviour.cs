using System;
using System.Security.Cryptography;
using System.Text;
using Unity.Entities;
using UnityEngine;

namespace FootStone.ECS
{
    public class ReplicatedEntityDataBehaviour : MonoBehaviour, IConvertGameObjectToEntity
    {
        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager,
            GameObjectConversionSystem conversionSystem)
        {
          //  FSLog.Info(" dstManager.AddComponentData(entity, new ReplicatedEntityData");

            var netId = transform.parent==null?gameObject.name:gameObject.name + transform.parent.gameObject.name;
            dstManager.AddComponentData(entity, new ReplicatedEntityData
            {
                Id = -1,
                PredictingPlayerId = -1,
                NetId = ToVid(netId)
            });
        }

        private long ToVid(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var res = md5.ComputeHash(Encoding.UTF8.GetBytes(str), 0, str.Length);
            return Math.Abs(BitConverter.ToInt64(res, 0));
        }
    }
}