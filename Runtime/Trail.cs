using UnityEngine;

namespace FormationSystem
{
    public class Trail<T> : Formation<T> where T : IFormationMember<T>
    {
        public override Vector3 GetMemberPosition(int memberIndex)
        {
            CheckIndexValidity(memberIndex);

            return new Vector3(0, -memberIndex, -memberIndex);
        }
    }
}
