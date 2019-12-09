using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace FootStone.ECS
{
    public class ReplicatedEntityCollection : IEntityReferenceSerializer
    {
        public static bool SampleHistory;
        public static int HistorySize = 128;
        public static int PredictionSize = 32;

        [ConfigVar(Name = "replicatedentity.showcollectioninfo", DefaultValue = "0",
            Description = "Show replicated system info")]
        private static ConfigVar showInfo;

        private readonly EntityManager entityManager;
        private readonly List<ReplicatedData> replicatedData = new List<ReplicatedData>(512);

        private readonly List<IInterpolatedSerializer> netInterpolated = new List<IInterpolatedSerializer>(32);
        private readonly List<IPredictedSerializer> netPredicted = new List<IPredictedSerializer>(32);
        private readonly List<IReplicatedSerializer> netSerializables = new List<IReplicatedSerializer>(32);

        private readonly DataComponentSerializers serializers = new DataComponentSerializers();

        public ReplicatedEntityCollection(EntityManager entityManager)
        {
            this.entityManager = entityManager;


//#if UNITY_EDITOR
//            historyCommands = new UserCommand[HistorySize];
//            hitstoryTicks = new int[HistorySize];
//            hitstoryLastServerTick = new int[HistorySize];
//#endif
        }

        public void SerializeReference(ref NetworkWriter writer, string name, Entity entity)
        {
            if (entity == Entity.Null || !entityManager.Exists(entity))
            {
                writer.WriteInt32(name, -1);
                return;
            }

            if (entityManager.HasComponent<ReplicatedEntityData>(entity))
            {
                var replicatedDataEntity = entityManager.GetComponentData<ReplicatedEntityData>(entity);
                writer.WriteInt32(name, replicatedDataEntity.Id);
                return;
            }

            GameDebug.LogError("Failed to serialize reference named:" + name + " to entity:" + entity);
        }

        public void DeserializeReference(ref NetworkReader reader, ref Entity entity)
        {
            var replicatedId = reader.ReadInt32();
            if (replicatedId < 0)
            {
                entity = Entity.Null;
                return;
            }

            entity = replicatedData[replicatedId].Entity;
        }

        public void Register(int entityId, Entity entity)
        {
            if (showInfo.IntValue > 0)
            {
                if (entityManager.HasComponent<Transform>(entity))
                    GameDebug.Log("RepEntity REGISTER NetID:" + entityId + " Entity:" + entity + " GameObject:" +
                                  entityManager.GetComponentObject<Transform>(entity).name);
                else
                    GameDebug.Log("RepEntity REGISTER NetID:" + entityId + " Entity:" + entity);
            }


            // Grow to make sure there is room for entity            
            if (entityId >= replicatedData.Count)
            {
                var count = entityId - replicatedData.Count + 1;
                var emptyData = new ReplicatedData();
                for (var i = 0; i < count; i++) replicatedData.Add(emptyData);
            }

            GameDebug.Assert(replicatedData[entityId].Entity == Entity.Null, "ReplicatedData has entity set:{0}",
                replicatedData[entityId].Entity);

            netSerializables.Clear();
            netPredicted.Clear();
            netInterpolated.Clear();

            //var go = entityManager.HasComponent<Transform>(entity)
            //    ? entityManager.GetComponentObject<Transform>(entity).gameObject
            //    : null;


            FindSerializers(entity);

            if (entityManager.HasComponent<EntityGroupChildren>(entity))
            {
                var buffer = entityManager.GetBuffer<EntityGroupChildren>(entity);
                for (var i = 0; i < buffer.Length; i++)
                {
                    var childEntity = buffer[i].Entity;
                    if (showInfo.IntValue > 0)
                        GameDebug.Log(" ReplicatedEntityChildren: " + i + " = " + childEntity);
                    FindSerializers(childEntity);
                }
            }

            var data = new ReplicatedData
            {
                Entity = entity,
                //      gameObject = go,
                SerializableArray = netSerializables.ToArray(),
                PredictedArray = netPredicted.ToArray(),
                InterpolatedArray = netInterpolated.ToArray()
            };

            replicatedData[entityId] = data;
        }

     

        private void FindSerializers(Entity entity)
        {
            // Add entity data handlers
            if (showInfo.IntValue > 0)
                GameDebug.Log("  FindSerializers");
            var componentTypes = entityManager.GetComponentTypes(entity);

            // Sort to ensure order when serializing components
            var typeArray = componentTypes.ToArray();
            Array.Sort(typeArray,(type1, type2) =>
                     string.Compare(type1.GetManagedType().Name, type2.GetManagedType().Name, StringComparison.Ordinal));

            var serializedComponentType = typeof(IReplicatedState);
            var predictedComponentType = typeof(IPredictedStateBase);
            var interpolatedComponentType = typeof(IInterpolatedStateBase);

            foreach (var componentType in typeArray)
            {
                var managedType = componentType.GetManagedType();

                if (!typeof(IComponentData).IsAssignableFrom(managedType))
                    continue;

                if (serializedComponentType.IsAssignableFrom(managedType))
                {
                    if (showInfo.IntValue > 0)
                        GameDebug.Log("   new SerializedComponentDataHandler for:" + managedType.Name);

                    var serializer = serializers.CreateNetSerializer(managedType, entityManager, entity, this);
                    if (serializer != null)
                        netSerializables.Add(serializer);
                }
                else if (predictedComponentType.IsAssignableFrom(managedType))
                {
                    var interfaceTypes = managedType.GetInterfaces();
                    foreach (var it in interfaceTypes)
                        if (it.IsGenericType)
                        {
                            var type = it.GenericTypeArguments[0];
                            if (showInfo.IntValue > 0)
                                GameDebug.Log("   new IPredictedDataHandler for:" + it.Name + " arg type:" + type);

                            var serializer =
                                serializers.CreatePredictedSerializer(managedType, entityManager, entity, this);
                            if (serializer != null)
                                netPredicted.Add(serializer);

                            break;
                        }
                }
                else if (interpolatedComponentType.IsAssignableFrom(managedType))
                {
                    var interfaceTypes = managedType.GetInterfaces();
                    foreach (var it in interfaceTypes)
                        if (it.IsGenericType)
                        {
                            var type = it.GenericTypeArguments[0];
                            if (showInfo.IntValue > 0)
                                GameDebug.Log("   new IInterpolatedDataHandler for:" + it.Name + " arg type:" + type);

                            var serializer =
                                serializers.CreateInterpolatedSerializer(managedType, entityManager, entity, this);
                            if (serializer != null)
                                netInterpolated.Add(serializer);

                            break;
                        }
                }
            }
        }


        public Entity GetEntity(int id)
        {
            return replicatedData[id].Entity;
        }

        public Entity Unregister(int entityId)
        {
            var entity = replicatedData[entityId].Entity;
            GameDebug.Assert(entity != Entity.Null, "Unregister. ReplicatedData has has entity set");

            if (showInfo.IntValue > 0)
            {
                if (entityManager.HasComponent<Transform>(entity))
                    GameDebug.Log("RepEntity UNREGISTER NetID:" + entityId + " Entity:" + entity + " GameObject:" +
                                  entityManager.GetComponentObject<Transform>(entity).name);
                else
                    GameDebug.Log("RepEntity UNREGISTER NetID:" + entityId + " Entity:" + entity);
            }

            replicatedData[entityId] = new ReplicatedData();
            return entity;
        }

        public void ProcessEntityUpdate(int serverTick, int id, ref NetworkReader reader)
        {
            var data = replicatedData[id];

            GameDebug.Assert(data.LastServerTick < serverTick,
                "Failed to apply snapshot. Wrong tick order. entityId:{0} snapshot tick:{1} last server tick:{2}", id,
                serverTick, data.LastServerTick);
            data.LastServerTick = serverTick;

            GameDebug.Assert(data.SerializableArray != null, "Failed to apply snapshot. Serializablearray is null");

            foreach (var entry in data.SerializableArray)
                entry.Deserialize(ref reader, serverTick);

            foreach (var entry in data.PredictedArray)
                entry.Deserialize(ref reader, serverTick);

            foreach (var entry in data.InterpolatedArray)
                entry.Deserialize(ref reader, serverTick);

            replicatedData[id] = data;
        }

        public void GenerateEntitySnapshot(int entityId, ref NetworkWriter writer)
        {
            var data = replicatedData[entityId];

            GameDebug.Assert(data.SerializableArray != null, "Failed to generate snapshot. Serializablearray is null");

            foreach (var entry in data.SerializableArray)
                entry.Serialize(ref writer);

            writer.SetFieldSection(NetworkWriter.FieldSectionType.OnlyPredicting);
            foreach (var entry in data.PredictedArray)
                entry.Serialize(ref writer);
            writer.ClearFieldSection();

            writer.SetFieldSection(NetworkWriter.FieldSectionType.OnlyNotPredicting);
            foreach (var entry in data.InterpolatedArray)
                entry.Serialize(ref writer);
            writer.ClearFieldSection();
        }

        public void Rollback()
        {
            for (var i = 0; i < replicatedData.Count; i++)
            {
                if (replicatedData[i].Entity == Entity.Null)
                    continue;

                if (replicatedData[i].PredictedArray == null)
                    continue;

                if (!entityManager.HasComponent<ServerEntity>(replicatedData[i].Entity))
                    continue;

                foreach (var predicted in replicatedData[i].PredictedArray)
                    predicted.Rollback();
            }
        }

        public void Interpolate(GameTick time)
        {
            for (var i = 0; i < replicatedData.Count; i++)
            {
                if (replicatedData[i].Entity == Entity.Null)
                    continue;

                if (replicatedData[i].InterpolatedArray == null)
                    continue;

                if (entityManager.HasComponent<ServerEntity>(replicatedData[i].Entity))
                    continue;

                foreach (var interpolated in replicatedData[i].InterpolatedArray)
                    interpolated.Interpolate(time);
            }
        }


        public string GenerateName(int entityId)
        {
            var data = replicatedData[entityId];

            var first = true;
            var name = "";
            foreach (var entry in data.SerializableArray)
            {
                if (!first)
                    name += "_";
                if (entry is Component)
                    name += (entry as Component).GetType();
                else
                    name += entry.GetType().ToString();
                first = false;
            }

            return name;
        }

        public struct ReplicatedData
        {
            public Entity Entity;

            //    public GameObject gameObject;
            public IReplicatedSerializer[] SerializableArray;
            public IPredictedSerializer[] PredictedArray;
            public IInterpolatedSerializer[] InterpolatedArray;
            public int LastServerTick;

            public bool VerifyPrediction(int sampleIndex, int tick)
            {
                foreach (var predictedDataHandler in PredictedArray)
                    if (!predictedDataHandler.VerifyPrediction(sampleIndex, tick))
                        return false;

                return true;
            }

            public bool HasState(int tick)
            {
                foreach (var predictedDataHandler in PredictedArray)
                    if (predictedDataHandler.HasServerState(tick))
                        return true;

                return false;
            }
        }


//#if UNITY_EDITOR

//        public int GetSampleCount()
//        {
//            return historyCount;
//        }

//        public int GetSampleTick(int sampleIndex)
//        {
//            var i = (historyFirstIndex + sampleIndex) % hitstoryTicks.Length;
//            return hitstoryTicks[i];
//        }

//        public int GetLastServerTick(int sampleIndex)
//        {
//            var i = (historyFirstIndex + sampleIndex) % hitstoryTicks.Length;
//            return hitstoryLastServerTick[i];
//        }

//        public bool IsPredicted(int entityIndex)
//        {
//            var netId = GetNetIdFromEntityIndex(entityIndex);
//            var replicatedData = m_replicatedData[netId];
//            return m_world.GetEntityManager().HasComponent<ServerEntity>(replicatedData.entity);
//        }

//        public int GetEntityCount()
//        {
//            int entityCount = 0;
//            for (int i = 0; i < m_replicatedData.Count; i++)
//            {
//                if (m_replicatedData[i].entity == Entity.Null)
//                    continue;
//                entityCount++;
//            }

//            return entityCount;
//        }

//        public int GetNetIdFromEntityIndex(int entityIndex)
//        {
//            int entityCount = 0;
//            for (int i = 0; i < m_replicatedData.Count; i++)
//            {
//                if (m_replicatedData[i].entity == Entity.Null)
//                    continue;

//                if (entityCount == entityIndex)
//                    return i;

//                entityCount++;
//            }

//            return -1;
//        }

//        public ReplicatedData GetReplicatedDataForNetId(int netId)
//        {
//            return m_replicatedData[netId];
//        }

//        public void StorePredictedState(int predictedTick, int finalTick)
//        {
//            if (!SampleHistory)
//                return;

//            var predictionIndex = finalTick - predictedTick;
//            var sampleIndex = GetSampleIndex();

//            for (int i = 0; i < m_replicatedData.Count; i++)
//            {
//                if (m_replicatedData[i].entity == Entity.Null)
//                    continue;

//                if (m_replicatedData[i].predictedArray == null)
//                    continue;

//                if (!m_world.GetEntityManager().HasComponent<ServerEntity>(m_replicatedData[i].entity))
//                    continue;

//                foreach (var predicted in m_replicatedData[i].predictedArray)
//                {
//                    predicted.StorePredictedState(sampleIndex, predictionIndex);
//                }
//            }
//        }


//        public void FinalizedStateHistory(int tick, int lastServerTick, ref UserCommand command)
//        {
//            if (!SampleHistory)
//                return;

//            var sampleIndex = (historyFirstIndex + historyCount) % hitstoryTicks.Length;

//            hitstoryTicks[sampleIndex] = tick;
//            historyCommands[sampleIndex] = command;
//            hitstoryLastServerTick[sampleIndex] = lastServerTick;

//            if (historyCount < hitstoryTicks.Length)
//                historyCount++;
//            else
//                historyFirstIndex = (historyFirstIndex + 1) % hitstoryTicks.Length;
//        }

//        int GetSampleIndex()
//        {
//            return (historyFirstIndex + historyCount) % hitstoryTicks.Length;
//        }

//        public int FindSampleIndexForTick(int tick)
//        {
//            for (int i = 0; i < hitstoryTicks.Length; i++)
//            {
//                if (hitstoryTicks[i] == tick)
//                    return i;
//            }

//            return -1;
//        }


//        UserCommand[] historyCommands;
//        int[] hitstoryTicks;
//        int[] hitstoryLastServerTick;
//        int historyFirstIndex;
//        int historyCount;

//#endif
    }
}