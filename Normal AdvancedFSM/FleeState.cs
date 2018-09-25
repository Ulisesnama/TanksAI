using UnityEngine;
using System.Collections;

public class FleeState : FSMState
{
    public FleeState(Transform[] wp, Transform playerTransform)
    {
        this.waypoints = wp;
        this.stateID = FSMStateID.Fleeing;
        this.SetNavMeshAgent(playerTransform);

        this.FindNextPoint();
    }

    public override void Reason(Transform player, Transform npc)
    {
        if (npc.gameObject.GetComponent<NPCTankController>().health >= 80)
        {
            npc.GetComponent<NPCTankController>().SetTransition(Transition.FullHealth);
        }
    }

    public override void Act(Transform player, Transform npc)
    {
        //Find another random patrol point if the current point is reached
        float shorterDistance = 10000;
        foreach (Transform value in waypoints)
        {
            if (Vector3.Distance(npc.position, value.position) <= shorterDistance)
            {
                destPosition = value.position;
            }
        }
        //Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(destPosition - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * this.currentRotationSpeed);

        npc.gameObject.GetComponent<NPCTankController>().agent.SetDestination(destPosition);

        if (this.navMeshAgent != null || player.transform != null || npc.transform != null)
        {
            this.navMeshAgent.SetDestination(this.destPosition);
        }

    }
}
