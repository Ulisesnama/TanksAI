using UnityEngine;
using System.Collections;

public class ChaseState : FSMState
{
    public ChaseState(Transform[] wp, Transform playerTransform)
    {
        waypoints = wp;
        stateID = FSMStateID.Chasing;

        currentRotationSpeed = 1.0f;
        currentSpeed = 100.0f;

        //find next Waypoint position
        FindNextPoint();

        this.SetNavMeshAgent(playerTransform);
    }

    public float FieldOfView = 80.0f;
    private int ViewDistance = 200;
    private Vector3 rayFromAItoPlayer;

    public override void Reason(Transform player, Transform npc)
    {
        //Set the target position as the player position
        destPosition = player.position;

        //Check the distance with player tank
        //When the distance is near, transition to attack state
        float dist = Vector3.Distance(npc.position, destPosition);
        if (dist <= npc.GetComponent<NPCTankController>().attackDistance)
        {
            Debug.Log("Switch to Attack state");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.ReachPlayer);
        }
        //Go back to patrol is it become too far
        else if (dist >= npc.GetComponent<NPCTankController>().chaseDistance)
        {
            RaycastHit hit;
            rayFromAItoPlayer = player.transform.position - npc.transform.position;
            if ((Vector3.Angle(rayFromAItoPlayer, npc.transform.forward)) < FieldOfView)
            {
                if (!Physics.Raycast(npc.transform.position, rayFromAItoPlayer, out hit, ViewDistance))
                {
                    Debug.Log("Switch to Patrol state");
                    npc.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
                }
            }
        }
    }

    public override void Act(Transform player, Transform npc)
    {
        //Rotate to the target point
        destPosition = player.position;

        Quaternion targetRotation = Quaternion.LookRotation(destPosition - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * currentRotationSpeed);

        //Go Forward
        if (this.navMeshAgent != null || player.transform != null || npc.transform != null)
        {
            this.navMeshAgent.SetDestination(this.destPosition);
        }
    }
}
