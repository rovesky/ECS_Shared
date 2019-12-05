using System;
using Unity.Entities;

namespace FootStone.ECS
{
    public struct Despawn : IComponentData
    {
        public int Frame;
    }
}