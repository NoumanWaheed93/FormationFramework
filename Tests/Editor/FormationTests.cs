using FormationSystem;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class FormationTests
{
    protected class TestFormationMember : IFormationMember<TestFormationMember>
    {
        public TestFormationMember Self => this;

        public int PositionIndex { get; set; }
        public Vector3 Position { get; set; }
        public Formation<TestFormationMember> Formation { get; set; }

    }

    protected Formation<TestFormationMember> formation;
    protected List<TestFormationMember> formationMembers;

    [Test]
    public void Member_Cannot_Be_Added_Twice()
    {
        CreateAFormation(0);
        TestFormationMember member = new TestFormationMember();
        formation.AddMember(member);
        formation.AddMember(member);
        Assert.AreEqual(1, formation.MemberCount);
    }

    [Test]
    public void Adding_Member_Twice_Does_Not_Change_Member_Position_Index()
    {
        Formation<TestFormationMember> formation = new Trail<TestFormationMember>();
        TestFormationMember member = new TestFormationMember();
        formation.AddMember(member);
        formation.AddMember(member);
        Assert.AreEqual(member.PositionIndex, 0);
    }

    //The two tests above only re-add a lone member, where Count-1 happens to be the index it
    //already had. The bug only shows up once a second member owns the slot Count-1 points at.
    [Test]
    public void Re_Adding_A_Member_Does_Not_Steal_Another_Members_Position_Index()
    {
        CreateAFormation(2);

        formation.AddMember(formationMembers[0]);

        Assert.AreEqual(2, formation.MemberCount, "Re-adding a member changed the member count");
        Assert.AreEqual(0, formationMembers[0].PositionIndex, "Re-added member lost its own index");
        Assert.AreEqual(1, formationMembers[1].PositionIndex, "Re-adding a member stole another member's index");
        Assert.AreEqual(formationMembers[0], formation.leader, "Re-adding a member changed the leader");
    }

    [Test]
    public virtual void Members_Get_Correct_PositionIndex_After_Adding_In_Formation()
    {
        CreateAFormation(5);
        for (int i = 0; i < formationMembers.Count; i++)
        {
            Assert.AreEqual(i, formationMembers[i].PositionIndex, "Incorrect position index for index number " + i);
        }
    }

    [Test]
    public virtual void Members_Shuffle_Positions_Correctly_After_A_Member_Is_Removed()
    {
        CreateAFormation(10);

        formation.RemoveMember(formationMembers[1]);
        int[] indexesAfterRemovingAt1 = { 0, 2, 3, 4, 5, 6, 7, 8, 9 };
        Are_Indexes_At_The_Correct_Position(indexesAfterRemovingAt1, formationMembers);
        formation.RemoveMember(formationMembers[9]);
        int[] indexesAfterRemovingAt9 = { 0, 2, 3, 4, 5, 6, 7, 8 };
        Are_Indexes_At_The_Correct_Position(indexesAfterRemovingAt9, formationMembers);
    }

    [Test]
    public virtual void Removing_The_leader_Refills_The_Spot_Correctly()
    {
        CreateAFormation(6);
        Assert.AreEqual(formationMembers[0], formation.leader);
        formation.RemoveMember(formationMembers[0]);
        int[] newIndexes = { 1, 2, 3, 4, 5 };
        Assert.AreEqual(formationMembers[1], formation.leader);
        Are_Indexes_At_The_Correct_Position(newIndexes, formationMembers);
    }


    protected static void Are_Indexes_At_The_Correct_Position(int[] newIndexes, List<TestFormationMember> formationMembers)
    {
        for (int i = 0; i < newIndexes.Length; i++)
        {
            Assert.AreEqual(i, formationMembers[newIndexes[i]].PositionIndex,
                "Member " + newIndexes[i] + " not at Correct position ");
        }
    }

    protected abstract void CreateAFormation(int count);


}
