using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Ulises
{
    public class StrongerState : FSMState
    {
        Ulises.NPCController tank;
        private GameObject turret;

        public StrongerState(Transform playerTransform)
        {
            this.stateID = FSMStateID.Strongering;
            this.SetNavMeshAgent(playerTransform);

            tank = GameObject.FindObjectOfType<Ulises.NPCController>();

            this.turret = playerTransform.FindChild("TankRenderers/TankTurret").gameObject;
        }

        public override void Update(Transform npc)
        {
            Transform[] invicibles = GameManager.Instance.GetInvicibleObjects();

            if (invicibles != null && invicibles.Length > 0 && this.navMeshAgent != null)
            {
                if (this.navMeshAgent.velocity == Vector3.zero)
                    this.navMeshAgent.Resume();

                int position = GetClosest(invicibles, npc);
                this.navMeshAgent.SetDestination(invicibles[position].position);
            }


            if (invicibles == null || Vector3.Distance(npc.position, destPosition) <= 0.5f)
            {
                Debug.Log("Ulises is switching to Attack State");
                npc.GetComponent<Ulises.NPCController>().SetTransition(Transition.SawPlayer);
            }

            Transform[] healthpoints = GameManager.Instance.GetHealthObjects();
            if (healthpoints != null && healthpoints.Length > 0 && this.navMeshAgent != null && tank.health <= 75)
            {
                Debug.Log("Ulises is switching to Flee State");
                npc.GetComponent<Ulises.NPCController>().SetTransition(Transition.LowHealth);
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
