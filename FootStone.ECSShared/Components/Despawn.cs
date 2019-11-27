using System;
using Unity.Entities;

namespace FootStone.ECS
{
    [Serializable]
    public struct Despawn : IComponentData
    {
        public int Frame;
    }
}