using UnityEngine;
using System.Collections.Generic;
using System;

namespace FormationSystem
{
    public abstract class Formation<T> where T : IFormationMember<T>
    {
        public int MemberCount { get { return members.Count; } }

        public float spacing { get; set; } //The distance to keep from the adjacent member.

        public float altitudeSpacing { get; set; }

        public virtual T leader { get; protected set; }

        protected HashSet<T> members = new HashSet<T>();
        public HashSet<T> Members { get { return members; } }

        public virtual void AddMember(T member)
        {
            //Add the member's position index before adding the member in the list. 
            //Because, this member's positionIndex is equal to Count before adding the new member
            members.Add(member);
            member.PositionIndex = members.Count-1;
            TryResetLeader(member);
            member.Position = GetMemberPosition(member.PositionIndex);
            member.Formation = this;
        }

        public abstract Vector3 GetMemberPosition(int memberIndex);

        public Vector3 GetMemberPositionSpaced(int memberIndex)
        {
            Vector3 spacedPosition = GetMemberPosition(memberIndex);
            float altitude = spacedPosition.y * altitudeSpacing;
            spacedPosition *= spacing;
            spacedPosition.y = altitude;
            return spacedPosition;
        }

        public virtual void RemoveMember(T memberToRemove)
        {
            int removedMemberIndex = memberToRemove.PositionIndex;
            members.Remove(memberToRemove);
            if (members.Count == 0)
            {
                leader = default(T);
                return;
            }
            foreach (T member in members)
            {
                if (member.PositionIndex > removedMemberIndex)
                {
                    member.PositionIndex--;
                    TryResetLeader(member);
                    member.Position = GetMemberPosition(member.PositionIndex);
                }
            }
        }

        protected void CheckIndexValidity(int memberIndex)
        {
            if (memberIndex < 0)
                throw new ArgumentException("Index cannot be a negative number");
        }

        protected void TryResetLeader(T member)
        {
            if (member.PositionIndex == 0)
            {
                leader = member;
            }
        }
    
    }

}
