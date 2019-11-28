using System;
using System.Collections.Generic;
using Unity.Entities;

namespace FootStone.ECS
{
    public interface IReplicatedStateSerializerFactory
    {
        IReplicatedSerializer CreateSerializer(EntityManager entityManager, Entity entity,
            IEntityReferenceSerializer refSerializer);
    }

    public interface IPredictedStateSerializerFactory
    {
        IPredictedSerializer CreateSerializer(EntityManager entityManager, Entity entity,
            IEntityReferenceSerializer refSerializer);
    }

    public interface IInterpolatedStateSerializerFactory
    {
        IInterpolatedSerializer CreateSerializer(EntityManager entityManager, Entity entity,
            IEntityReferenceSerializer refSerializer);
    }

    public class ReplicatedStateSerializerFactory<T> : IReplicatedStateSerializerFactory
        where T : struct, IReplicatedState, IComponentData
    {
        public IReplicatedSerializer CreateSerializer(EntityManager entityManager, Entity entity,
            IEntityReferenceSerializer refSerializer)
        {
            return new ReplicatedStateSerializer<T>(entityManager, entity, refSerializer);
        }
    }

    public class PredictedStateSerializerFactory<T> : IPredictedStateSerializerFactory
        where T : struct, IPredictedState<T>, IComponentData
    {
        public IPredictedSerializer CreateSerializer(EntityManager entityManager, Entity entity,
            IEntityReferenceSerializer refSerializer)
        {
            return new PredictedStateSerializer<T>(entityManager, entity, refSerializer);
        }
    }

    public class InterpolatedStateSerializerFactory<T> : IInterpolatedStateSerializerFactory
        where T : struct, IInterpolatedState<T>, IComponentData
    {
        public IInterpolatedSerializer CreateSerializer(EntityManager entityManager, Entity entity,
            IEntityReferenceSerializer refSerializer)
        {
            return new InterpolatedStateSerializer<T>(entityManager, entity, refSerializer);
        }
    }


    public interface IReplicatedSerializer
    {
        void Serialize(ref NetworkWriter writer);
        void Deserialize(ref NetworkReader reader, int tick);
    }

    public interface IPredictedSerializer
    {
        void Serialize(ref NetworkWriter writer);
        void Deserialize(ref NetworkReader reader, int tick);
        void Rollback();

        Entity GetEntity();
        bool HasServerState(int tick);
        object GetServerState(int tick);
        void StorePredictedState(int sampleIndex, int predictionIndex);
        object GetPredictedState(int sampleIndex, int predictionIndex);
        bool VerifyPrediction(int sampleIndex, int tick);
    }

    public interface IInterpolatedSerializer
    {
        void Serialize(ref NetworkWriter writer);
        void Deserialize(ref NetworkReader reader, int tick);
        void Interpolate(GameTick time);
    }

    internal class ReplicatedStateSerializer<T> : IReplicatedSerializer
        where T : struct, IReplicatedState, IComponentData
    {
        private SerializeContext context;

        public ReplicatedStateSerializer(EntityManager entityManager, Entity entity,
            IEntityReferenceSerializer refSerializer)
        {
            context.EntityManager = entityManager;
            context.Entity = entity;
            context.RefSerializer = refSerializer;
        }

        public void Serialize(ref NetworkWriter writer)
        {
            var state = context.EntityManager.GetComponentData<T>(context.Entity);
            state.Serialize(ref context, ref writer);
        }

        public void Deserialize(ref NetworkReader reader, int tick)
        {
            var state = context.EntityManager.GetComponentData<T>(context.Entity);
            context.Tick = tick;
            state.Deserialize(ref context, ref reader);
            context.EntityManager.SetComponentData(context.Entity, state);
        }
    }

    internal class PredictedStateSerializer<T> : IPredictedSerializer
        where T : struct, IPredictedState<T>, IComponentData
    {
        private SerializeContext context;
        private T lastServerState;
        private readonly T[] predictedStates;

        private SparseTickBuffer predictedStateTicks;
        private readonly T[] serverStates;


        private readonly SparseTickBuffer serverStateTicks;


        public PredictedStateSerializer(EntityManager entityManager, Entity entity,
            IEntityReferenceSerializer refSerializer)
        {
            context.EntityManager = entityManager;
            context.Entity = entity;
            context.RefSerializer = refSerializer;
            lastServerState = default(T);

            serverStateTicks = new SparseTickBuffer(ReplicatedEntityCollection.HistorySize);
            serverStates = new T[ReplicatedEntityCollection.HistorySize];

            //        predictedStateTicks = new SparseTickBuffer(ReplicatedEntityCollection.HistorySize);
            predictedStates = new T[ReplicatedEntityCollection.HistorySize * ReplicatedEntityCollection.PredictionSize];
        }

        public void Serialize(ref NetworkWriter writer)
        {
            var state = context.EntityManager.GetComponentData<T>(context.Entity);
            state.Serialize(ref context, ref writer);
        }

        public void Deserialize(ref NetworkReader reader, int tick)
        {
            context.Tick = tick;
            lastServerState.Deserialize(ref context, ref reader);

#if UNITY_EDITOR
            //if (ReplicatedEntityCollection.SampleHistory)
            //{
            //    var index = serverStateTicks.GetIndex((uint)tick);
            //    if(index == -1)                
            //        index = serverStateTicks.Register((uint)tick);
            //    serverStates[index] = m_lastServerState;
            //}
#endif
        }

        public void Rollback()
        {
            //        GameDebug.Log("Rollback:" + m_lastServerState); 
            context.EntityManager.SetComponentData(context.Entity, lastServerState);
        }


        public Entity GetEntity()
        {
            return context.Entity;
        }

        public object GetServerState(int tick)
        {
            var index = serverStateTicks.GetIndex((uint) tick);
            if (index == -1)
                return null;

            return serverStates[index];
        }

        public bool HasServerState(int tick)
        {
            var index = serverStateTicks.GetIndex((uint) tick);
            return index != -1;
        }

        public void StorePredictedState(int sampleIndex, int predictionIndex)
        {
            if (!ReplicatedEntityCollection.SampleHistory)
                return;

            if (predictionIndex >= ReplicatedEntityCollection.PredictionSize)
                return;

            var index = sampleIndex * ReplicatedEntityCollection.PredictionSize + predictionIndex;

            var state = context.EntityManager.GetComponentData<T>(context.Entity);
            predictedStates[index] = state;
        }

        public object GetPredictedState(int sampleIndex, int predictionIndex)
        {
            if (predictionIndex >= ReplicatedEntityCollection.PredictionSize)
                return null;

            var index = sampleIndex * ReplicatedEntityCollection.PredictionSize + predictionIndex;
            return predictedStates[index];
        }

        public bool VerifyPrediction(int sampleIndex, int tick)
        {
            var serverIndex = serverStateTicks.GetIndex((uint) tick);
            if (serverIndex == -1)
                return true;

            var predictedIndex = sampleIndex * ReplicatedEntityCollection.PredictionSize;
            return serverStates[serverIndex].VerifyPrediction(ref predictedStates[predictedIndex]);
        }
    }

    internal class InterpolatedStateSerializer<T> : IInterpolatedSerializer
        where T : struct, IInterpolatedState<T>, IComponentData
    {
        private SerializeContext context;
        private readonly TickStateSparseBuffer<T> stateHistory = new TickStateSparseBuffer<T>(32);

        public InterpolatedStateSerializer(EntityManager entityManager, Entity entity,
            IEntityReferenceSerializer refSerializer)
        {
            context.EntityManager = entityManager;
            context.Entity = entity;
            context.RefSerializer = refSerializer;
        }

        public void Serialize(ref NetworkWriter writer)
        {
            var state = context.EntityManager.GetComponentData<T>(context.Entity);
            state.Serialize(ref context, ref writer);
        }

        public void Deserialize(ref NetworkReader reader, int tick)
        {
            context.Tick = tick;
            var state = new T();
            state.Deserialize(ref context, ref reader);
            stateHistory.Add(tick, state);
        }

        public void Interpolate(GameTick interpTime)
        {
            var state = new T();

            if (stateHistory.Count > 0)
            {
                int lowIndex = 0, highIndex = 0;
                float interpVal = 0;
                var interpValid = stateHistory.GetStates((int) interpTime.Tick, interpTime.TickDurationAsFraction,
                    ref lowIndex, ref highIndex, ref interpVal);

                if (interpValid)
                {
                    var prevState = stateHistory[lowIndex];
                    var nextState = stateHistory[highIndex];
                    state.Interpolate(ref context, ref prevState, ref nextState, interpVal);
                }
                else
                {
                    state = stateHistory.Last();
                }
            }

            context.EntityManager.SetComponentData(context.Entity, state);
        }
    }


    public class DataComponentSerializers
    {
        private readonly Dictionary<Type, IInterpolatedStateSerializerFactory> interpolatedSerializerFactories =
            new Dictionary<Type, IInterpolatedStateSerializerFactory>();

        private readonly Dictionary<Type, IReplicatedStateSerializerFactory> netSerializerFactories =
            new Dictionary<Type, IReplicatedStateSerializerFactory>();

        private readonly Dictionary<Type, IPredictedStateSerializerFactory> predictedSerializerFactories =
            new Dictionary<Type, IPredictedStateSerializerFactory>();

        public DataComponentSerializers()
        {
            CreateSerializerFactories();
        }

        public IReplicatedSerializer CreateNetSerializer(Type type, EntityManager manager, Entity entity,
            IEntityReferenceSerializer refSerializer)
        {
            IReplicatedStateSerializerFactory factory;
            if (netSerializerFactories.TryGetValue(type, out factory))
                return factory.CreateSerializer(manager, entity, refSerializer);

            GameDebug.LogError("Failed to find INetSerializer for type:" + type.Name);
            return null;
        }

        public IPredictedSerializer CreatePredictedSerializer(Type type, EntityManager manager, Entity entity,
            IEntityReferenceSerializer refSerializer)
        {
            IPredictedStateSerializerFactory factory;
            if (predictedSerializerFactories.TryGetValue(type, out factory))
                return factory.CreateSerializer(manager, entity, refSerializer);

            GameDebug.LogError("Failed to find IPredictedSerializer for type:" + type.Name);
            return null;
        }

        public IInterpolatedSerializer CreateInterpolatedSerializer(Type type, EntityManager manager, Entity entity,
            IEntityReferenceSerializer refSerializer)
        {
            IInterpolatedStateSerializerFactory factory;
            if (interpolatedSerializerFactories.TryGetValue(type, out factory))
                return factory.CreateSerializer(manager, entity, refSerializer);

            GameDebug.LogError("Failed to find IInterpolatedSerializer for type:" + type.Name);
            return null;
        }


        private void CreateSerializerFactories()
        {
            var componentDataType = typeof(IComponentData);
            var serializedType = typeof(IReplicatedState);
            var predictedType = typeof(IPredictedStateBase);
            var interpolatedType = typeof(IInterpolatedStateBase);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var type in assembly.GetTypes())
            {
                if (!componentDataType.IsAssignableFrom(type))
                    continue;

                if (serializedType.IsAssignableFrom(type))
                {
                    //                    GameDebug.Log("Making serializer factory for type:" + type);

                    var method = type.GetMethod("CreateSerializerFactory");
                    if (method == null)
                    {
                        GameDebug.LogError("Replicated component " + type + " has no CreateSerializerFactory");
                        continue;
                    }

                    if (method.ReturnType != typeof(IReplicatedStateSerializerFactory))
                    {
                        GameDebug.LogError("Replicated component " + type +
                                           " CreateSerializerFactory does not have return type IReplicatedComponentSerializerFactory");
                        continue;
                    }

                    var result = method.Invoke(null, new object[] { });
                    netSerializerFactories.Add(type, (IReplicatedStateSerializerFactory) result);
                }

                if (predictedType.IsAssignableFrom(type))
                {
                    //                    GameDebug.Log("Making predicted serializer factory for type:" + type);

                    var method = type.GetMethod("CreateSerializerFactory");
                    if (method == null)
                    {
                        GameDebug.LogError("Predicted component " + type + " has no CreateSerializerFactory");
                        continue;
                    }

                    if (method.ReturnType != typeof(IPredictedStateSerializerFactory))
                    {
                        GameDebug.LogError("Replicated component " + type +
                                           " CreateSerializerFactory does not have return type IPredictedComponentSerializerFactory");
                        continue;
                    }

                    var result = method.Invoke(null, new object[] { });
                    predictedSerializerFactories.Add(type, (IPredictedStateSerializerFactory) result);
                }

                if (interpolatedType.IsAssignableFrom(type))
                {
                    //                    GameDebug.Log("Making interpolated serializer factory for type:" + type);

                    var method = type.GetMethod("CreateSerializerFactory");
                    if (method == null)
                    {
                        GameDebug.LogError("Interpolated component " + type + " has no CreateSerializerFactory");
                        continue;
                    }

                    if (method.ReturnType != typeof(IInterpolatedStateSerializerFactory))
                    {
                        GameDebug.LogError("Replicated component " + type +
                                           " CreateSerializerFactory does not have return type IInterpolatedComponentSerializerFactory");
                        continue;
                    }


                    var result = method.Invoke(null, new object[] { });
                    interpolatedSerializerFactories.Add(type, (IInterpolatedStateSerializerFactory) result);
                }
            }
        }
    }
}