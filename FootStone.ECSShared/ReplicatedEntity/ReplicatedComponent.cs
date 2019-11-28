using Unity.Entities;

namespace FootStone.ECS
{
    public interface IEntityReferenceSerializer
    {
        void SerializeReference(ref NetworkWriter writer, string name, Entity entity);
        void DeserializeReference(ref NetworkReader reader, ref Entity entity);
    }

    public struct SerializeContext
    {
        public EntityManager EntityManager;
        public Entity Entity;
        public IEntityReferenceSerializer RefSerializer;
        public int Tick;
    }

    public interface IPredictedStateBase
    {
    }

    public interface IInterpolatedStateBase
    {
    }

    // Interface for components that are replicated to all clients
    public interface IReplicatedState
    {
        void Serialize(ref SerializeContext context, ref NetworkWriter writer);
        void Deserialize(ref SerializeContext context, ref NetworkReader reader);
    }

    // Interface for components that are replicated only to predicting clients
    public interface IPredictedState<T> : IPredictedStateBase
    {
        void Serialize(ref SerializeContext context, ref NetworkWriter writer);
        void Deserialize(ref SerializeContext context, ref NetworkReader reader);
        bool VerifyPrediction(ref T state);
    }

    // Interface for components that are replicated to all non-predicting clients
    public interface IInterpolatedState<T> : IInterpolatedStateBase
    {
        void Serialize(ref SerializeContext context, ref NetworkWriter writer);
        void Deserialize(ref SerializeContext context, ref NetworkReader reader);
        void Interpolate(ref SerializeContext context, ref T first, ref T last, float t);
    }


}