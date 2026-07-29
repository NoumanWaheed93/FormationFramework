using UnityEngine;

namespace FormationSystem
{
    public class Echelon<T> : Formation<T> where T : IFormationMember<T>
    {
        public override Vector3 GetMemberPosition(int memberIndex)
        {
            CheckIndexValidity(memberIndex);

            return new Vector3(0.707f * memberIndex, -1 * memberIndex, -0.707f * memberIndex);
        }
    
    }

}
