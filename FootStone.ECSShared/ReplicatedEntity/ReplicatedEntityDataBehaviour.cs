using System.Linq;
using FootStone.ECS;
using Unity.Entities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FootStone.Kitchen
{
    public class ReplicatedEntityDataBehaviour : MonoBehaviour, IConvertGameObjectToEntity
    {

        public byte[] netID;

        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager,
            GameObjectConversionSystem conversionSystem)
        {

            dstManager.AddComponentData(entity, new ReplicatedEntityData
            {
                Id = -1,
                PredictingPlayerId = -1
            });

        }
/*
#if UNITY_EDITOR
        private void Awake()
        {
            if (EditorApplication.isPlaying)
                return;
            SetUniqueNetID();

        }

        //   public static Dictionary<byte[], ReplicatedEntity> netGuidMap = new Dictionary<byte[], ReplicatedEntity>(new ByteArrayComp());

        private void OnValidate()
        {
            if (EditorApplication.isPlaying)
                return;

            SetUniqueNetID();

            //PrefabType prefabType = PrefabUtility.GetPrefabType(this);
            //if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab)
            //{
            //    netID = null;
            //}
            //else
            //    SetUniqueNetID();

            //UpdateAssetGuid();
        }

        //public bool SetAssetGUID(string guidStr)
        //{
        //    var guid = new WeakAssetReference(guidStr);
        //    var val = Value;
        //    var currentGuid = val.assetGuid;
        //    if (!guid.Equals(currentGuid))
        //    {
        //        val.assetGuid = guid;
        //        Value = val;
        //        PrefabUtility.SavePrefabAsset(gameObject);
        //        return true;
        //    }

        //    return false;
        //}

        //public void UpdateAssetGuid()
        //{
        //    // Set type guid
        //    var stage = PrefabStageUtility.GetPrefabStage(gameObject);
        //    if (stage != null)
        //    {
        //        var guidStr = AssetDatabase.AssetPathToGUID(stage.prefabAssetPath);
        //        if (SetAssetGUID(guidStr))
        //            EditorSceneManager.MarkSceneDirty(stage.scene);
        //    }
        //}

        private void SetUniqueNetID()
        {
            
            // Generate new if fresh object
            if (netID == null || netID.Length == 0)
            {
                var guid = gameObject.name;
                netID = guid.ToArray<byte>();
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

        }

#endif
*/

    }
}
