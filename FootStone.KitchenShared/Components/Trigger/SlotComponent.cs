using Unity.Entities;

namespace Assets.Scripts.ECS
{
	public struct SlotComponent : IComponentData
    {
		// ���
        public Entity SlotEntity;
		// ����Ķ���
        public Entity FiltInEntity;
    }
}