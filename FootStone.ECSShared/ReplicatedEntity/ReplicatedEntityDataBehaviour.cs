using System;
using System.Linq;
using System.Text;
using FootStone.ECS;
using Unity.Entities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FootStone.ECS
{
    public class ReplicatedEntityDataBehaviour : MonoBehaviour, IConvertGameObjectToEntity
    {

        //    public int NetId;
        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager,
            GameObjectConversionSystem conversionSystem)
        {
            FSLog.Info(" dstManager.AddComponentData(entity, new ReplicatedEntityData");
            dstManager.AddComponentData(entity, new ReplicatedEntityData
            {
                Id = -1,
                PredictingPlayerId = -1,
                NetId = ToVid(gameObject.name)
            });
        }

        private long ToVid(string str)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] res = md5.ComputeHash(Encoding.UTF8.GetBytes(str), 0, str.Length);
            return Math.Abs(BitConverter.ToInt64(res, 0));
        }
     

//#if UNITY_EDITOR
//        private void Awake()
//        {
//            if (EditorApplication.isPlaying)
//                return;
//            SetUniqueNetID();

//        }

//        //   public static Dictionary<byte[], ReplicatedEntity> netGuidMap = new Dictionary<byte[], ReplicatedEntity>(new ByteArrayComp());

//        private void OnValidate()
//        {
//            if (EditorApplication.isPlaying)
//                return;

//            SetUniqueNetID();

//            //PrefabType prefabType = PrefabUtility.GetPrefabType(this);
//            //if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab)
//            //{
//            //    netID = null;
//            //}
//            //else
//            //    SetUniqueNetID();

//            //UpdateAssetGuid();
//        }

//        //public bool SetAssetGUID(string guidStr)
//        //{
//        //    var guid = new WeakAssetReference(guidStr);
//        //    var val = Value;
//        //    var currentGuid = val.assetGuid;
//        //    if (!guid.Equals(currentGuid))
//        //    {
//        //        val.assetGuid = guid;
//        //        Value = val;
//        //        PrefabUtility.SavePrefabAsset(gameObject);
//        //        return true;
//        //    }

//        //    return false;
//        //}

//        //public void UpdateAssetGuid()
//        //{
//        //    // Set type guid
//        //    var stage = PrefabStageUtility.GetPrefabStage(gameObject);
//        //    if (stage != null)
//        //    {
//        //        var guidStr = AssetDatabase.AssetPathToGUID(stage.prefabAssetPath);
//        //        if (SetAssetGUID(guidStr))
//        //            EditorSceneManager.MarkSceneDirty(stage.scene);
//        //    }
//        //}

//        private void SetUniqueNetID()
//        {

//            var guid = System.Guid.NewGuid();

//            EditorSceneManager.MarkSceneDirty(gameObject.scene);


//        }

//#endif


    }
}
