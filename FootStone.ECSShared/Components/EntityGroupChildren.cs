using System;
using Unity.Entities;

namespace FootStone.ECS
{

    [InternalBufferCapacity(16)]
    public struct EntityGroupChildren : IBufferElementData
    {
        public Entity Entity;
    }
}