using UnityEngine;
using System.Collections;

namespace Ulises
{
	public class AttackState : FSMState
	{
		private float squareAttackDistance;
		private GameObject turret;
        private Ulises.NPCController tank;

		public AttackState(Transform[] wp, Transform playerTransform)
		{
			this.destPoints.AddRange(wp);
			this.stateID = FSMStateID.Attacking;

			this.SetNavMeshAgent(playerTransform);

			this.squareAttackDistance = 20.0f * 20.0f;

			this.turret = playerTransform.FindChild("TankRenderers/TankTurret").gameObject;

            tank = GameObject.FindObjectOfType<Ulises.NPCController>();
		}

		public override void Update(Transform npc)
		{
            Transform[] healthpoints = GameManager.Instance.GetHealthObjects();
            if (healthpoints != null && healthpoints.Length > 0 && this.navMeshAgent != null && tank.health <= 75)
            {
                Debug.Log("Ulises is switching to Flee State");
                npc.GetComponent<Ulises.NPCController>().SetTransition(Transition.LowHealth);
            }
            
            Transform[] invicibles = GameManager.Instance.GetInvicibleObjects();
            if (invicibles != null && invicibles.Length > 0 && this.navMeshAgent != null)
            {
                int position = GetClosest(invicibles, npc);
                if (Vector3.Distance(npc.position, invicibles[position].position) < 10.0f)
                {
                    Debug.Log("Ulises is switching to Stronger State");
                    npc.GetComponent<Ulises.NPCController>().SetTransition(Transition.SawInvicible);
                }
            }
            
			Transform[] enemies = GameManager.Instance.GetEnemies(npc);

            if (enemies != null && enemies.Length > 0 && this.navMeshAgent != null)
			{
                int position = GetClosest(enemies, npc);

                float sqrDistNpcPlayer = (enemies[position].position - npc.position).sqrMagnitude;
				if (sqrDistNpcPlayer < this.squareAttackDistance)
				{
                    this.navMeshAgent.Stop();
				}
				else
				{
					if (this.navMeshAgent.velocity == Vector3.zero)
						this.navMeshAgent.Resume();

                    this.navMeshAgent.SetDestination(enemies[position].position);
				}

				//Rotate to the target point
				Quaternion targetRotation = Quaternion.LookRotation(enemies[position].position - npc.position);
				this.turret.transform.rotation = Quaternion.Slerp(this.turret.transform.rotation, targetRotation, Time.deltaTime * this.currentRotationSpeed);

                npc.GetComponent<Ulises.NPCController>().ShootBullet();
            }
        }
	}
}

