using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Ulises
{
	public class PatrolState : FSMState
	{
        public PatrolState(List<Transform> ammoPoints, Transform playerTransform)
        {
            this.destPoints = ammoPoints;
            this.stateID = FSMStateID.Patrolling;
            this.SetNavMeshAgent(playerTransform);

            this.FindNextPoint();
        }

        public override void Update(Transform npc)
        {
            
            if (Vector3.Distance(npc.position, destPosition) <= 0.8f)
            {
                bool result = this.FindNextPoint();
                if (result == false)
                {
                    Debug.Log("Ulises is switching to Attack State");
                    npc.GetComponent<Ulises.NPCController>().SetTransition(Transition.SawPlayer);
                }
            }

            //Go Forward
            if (this.navMeshAgent != null || npc.transform != null)
            {
                this.navMeshAgent.SetDestination(this.destPosition);
            }
        }
    }
}
