using Unity.Entities;

namespace FootStone.ECS
{
   // [DisableAutoCreation]
    public abstract class NoSortComponentSystemGroup : ComponentSystemGroup
    {
        public override void SortSystemUpdateList()
        {
        }
    }
}