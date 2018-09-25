using UnityEngine;
using System.Collections;

public class AttackState : FSMState
{
    public AttackState(Transform[] wp)
    {
        this.waypoints = wp;
        this.stateID = FSMStateID.Attacking;

        //find next Waypoint position
        this.FindNextPoint();
    }

    public override void Reason(Transform player, Transform npc)
    {
        //Check the distance with the player tank
        float dist = Vector3.Distance(npc.position, player.position);
        if (dist >= npc.GetComponent<NPCTankController>().attackDistance && dist < npc.GetComponent<NPCTankController>().chaseDistance)
        {
            //Rotate to the target point
            Quaternion targetRotation = Quaternion.LookRotation(destPosition - npc.position);
            npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * currentRotationSpeed);

            //Go Forward
            npc.Translate(Vector3.forward * Time.deltaTime * currentSpeed);

            Debug.Log("Switch to Chase State");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
        }
        //Transition to patrol is the tank become too far
        else if (dist >= npc.GetComponent<NPCTankController>().chaseDistance)
        {
            Debug.Log("Switch to Patrol State");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
        }  
    }

    public override void Act(Transform player, Transform npc)
    {
        //Set the target position as the player position
        destPosition = player.position;

        //Always Turn the turret towards the player
        Transform turret = npc.GetComponent<NPCTankController>().turret;
        Quaternion turretRotation = Quaternion.LookRotation(destPosition - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * currentRotationSpeed);

        //Shoot bullet towards the player
        npc.GetComponent<NPCTankController>().ShootBullet();
    }
}
