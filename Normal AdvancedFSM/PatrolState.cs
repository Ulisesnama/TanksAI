using UnityEngine;
using System.Collections;

public class PatrolState : FSMState
{
    public PatrolState(Transform[] wp, Transform playerTransform)
    {
        this.waypoints = wp;
        this.stateID = FSMStateID.Patrolling;

        this.SetNavMeshAgent(playerTransform);
    }

    public float FieldOfView = 80.0f;
    private int ViewDistance = 200;
    private Vector3 rayFromAItoPlayer;

    public override void Reason(Transform player, Transform npc)
    {
        float dist = Vector3.Distance(npc.position, player.position);
        RaycastHit hit;
        rayFromAItoPlayer = player.transform.position - npc.transform.position;
        if ((Vector3.Angle(rayFromAItoPlayer, npc.transform.forward)) < FieldOfView)
        {
            if (Physics.Raycast(npc.transform.position, rayFromAItoPlayer, out hit, ViewDistance))
            {
                Debug.Log("Switch to Chase State");
                npc.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
            }
        }

        else if (dist <= npc.GetComponent<NPCTankController>().chaseDistance)
        {
            Debug.Log("Switch to Chase State");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
        }
    }

    public override void Act(Transform player, Transform npc)
    {
        //Find another random patrol point if the current point is reached
		
        if (Vector3.Distance(npc.position, destPosition) <= 10.0f)
        {
            Debug.Log("Reached to the destination point\ncalculating the next point");
            this.FindNextPoint();
        }

        //Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(destPosition - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * this.currentRotationSpeed);

        //Go Forward
        if (this.navMeshAgent != null || player.transform != null || npc.transform != null)
        {
            this.navMeshAgent.SetDestination(this.destPosition);
        }
    }
}