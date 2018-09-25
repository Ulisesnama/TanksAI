using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Ulises
{
    public class FleeState : FSMState
    {
        Ulises.NPCController tank;
        private GameObject turret;

        public FleeState(Transform playerTransform)
        {
            this.stateID = FSMStateID.Fleeing;

            this.SetNavMeshAgent(playerTransform);

            tank = GameObject.FindObjectOfType<Ulises.NPCController>();

            this.turret = playerTransform.FindChild("TankRenderers/TankTurret").gameObject;

        }

        public override void Update(Transform npc)
        {
            Transform[] healthpoints = GameManager.Instance.GetHealthObjects();
            
            if (healthpoints != null && healthpoints.Length > 0 && this.navMeshAgent != null)
            {
                if (this.navMeshAgent.velocity == Vector3.zero)
                    this.navMeshAgent.Resume();

                int position = GetClosest(healthpoints, npc);
                this.navMeshAgent.SetDestination(healthpoints[position].position);
            }

            if (healthpoints == null || tank.health > 90)
            {
                Debug.Log("Ulises is switching to Attack State");
                npc.GetComponent<Ulises.NPCController>().SetTransition(Transition.HighHealt);
            }

            Transform[] invicibles = GameManager.Instance.GetInvicibleObjects();
            if (invicibles != null && invicibles.Length > 0 && this.navMeshAgent != null)
            {
                int position = GetClosest(invicibles, npc);
                if (Vector3.Distance(npc.position, invicibles[position].position) > Vector3.Distance(npc.position, this.navMeshAgent.destination))
                {
                    Debug.Log("Ulises is switching to Stronger State");
                    npc.GetComponent<Ulises.NPCController>().SetTransition(Transition.SawInvicible);
                }
            }

            Transform[] enemies = GameManager.Instance.GetEnemies(npc);

            if (enemies != null && enemies.Length > 0 && this.navMeshAgent != null)
            {
                int position = GetClosest(enemies, npc);


                //Rotate to the target point
                Quaternion targetRotation = Quaternion.LookRotation(enemies[position].position - npc.position);
                this.turret.transform.rotation = Quaternion.Slerp(this.turret.transform.rotation, targetRotation, Time.deltaTime * this.currentRotationSpeed);

                npc.GetComponent<Ulises.NPCController>().ShootBullet();
            }
        }
    }
}

