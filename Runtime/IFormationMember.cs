using UnityEngine;

namespace FormationSystem
{
    public interface IFormationMember<T> where T : IFormationMember<T>
    {
        public T Self { get; }

        public int PositionIndex { get; set; }// position_0(leader).....

        public Vector3 Position { get; set; } //The relative position to formation leader.

        public Formation<T> Formation { get; set; }

    }

}
