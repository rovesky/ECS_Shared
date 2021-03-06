﻿using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace FootStone.ECS
{
    public abstract class ReplicatedEntityFactory : ScriptableObject
    {
        [HideInInspector] public WeakAssetReference guid;

        public abstract Entity Create(EntityManager entityManager, BundledResourceManager resourceManager,
            GameWorld world,ushort type);


#if UNITY_EDITOR

        private void OnValidate()
        {
            UpdateAssetGuid();
        }

        public void SetAssetGUID(string guidStr)
        {
            var assetGuid = new WeakAssetReference(guidStr);
            if (!assetGuid.Equals(guid))
            {
                guid = assetGuid;
                EditorUtility.SetDirty(this);
            }
        }

        public void UpdateAssetGuid()
        {
            var path = AssetDatabase.GetAssetPath(this);
            if (path != null && path != "")
            {
                var guidStr = AssetDatabase.AssetPathToGUID(path);
                SetAssetGUID(guidStr);
            }
        }
#endif
    }


#if UNITY_EDITOR
    public class ReplicatedEntityFactoryEditor<T> : Editor
        where T : ReplicatedEntityFactory
    {
        public override void OnInspectorGUI()
        {
            var factory = target as T;
            GUILayout.Label("GUID:" + factory.guid.GetGuidStr());
        }
    }

#endif


    public class ReplicatedEntityFactoryManager
    {

        private Dictionary<ushort, ReplicatedEntityFactory> factories = new Dictionary<ushort, ReplicatedEntityFactory>();

        public ReplicatedEntityFactoryManager()
        {
            
        }

        public void RegisterFactory(ushort typeId, ReplicatedEntityFactory typeFactory)
        {
           
            factories[typeId] = typeFactory;

            FSLog.Info(($"RegisterFactory，typeId:{typeId},factories size:{factories.Count}"));
        }

        public ReplicatedEntityFactory GetFactory(ushort typeId)
        {
          //  FSLog.Info(($"typeId:{typeId},factories size:{factories.Count}"));
            return factories[typeId];
        }


    }


}

