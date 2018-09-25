using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Ulises
{
	public class NPCController : AdvancedFSM, INpcContract
	{
		public string PlayerName;
		public GameObject Bullet;
		public UnityEngine.UI.Image HealthSlideImage;

		public int health;
        private List<Transform> ammoDestinations;
        private Color playerColor;

        private HealthBonus[] healthPoints;
        private InvicibleGameEvent[] inviciblePoints;

		//Initialize the Finite state machine for the NPC tank
		protected override void Initialize()
		{
            this.PlayerName = "Ulises";
			this.health = 100;
			this.elapsedTime = 0.0f;
			this.shootRate = 2.0f;
            //Get the turret of the tank
            this.turret = this.gameObject.transform.FindChild("TankRenderers/TankTurret").transform;
			Debug.Assert(this.turret != null, "Unable to find the child TankTurret");

            this.bulletSpawnPoint = this.gameObject.transform.FindChild("TankRenderers/TankTurret/FireTransform").transform;
			Debug.Assert(this.turret != null, "Unable to find the child FireTransform");

			//Start Doing the Finite State Machine
			this.ConstructFSM();
		}

		//Update each frame
		protected override void FSMUpdate()
		{
			//Check for health
			this.elapsedTime += Time.deltaTime;
		}

		protected override void FSMFixedUpdate()
		{
			this.CurrentState.Update(this.transform);
		}

		public void SetTransition(Transition t)
		{
			this.PerformTransition(t);
		}

		private void ConstructFSM()
		{
			//Get the list of points
			pointList = GameObject.FindGameObjectsWithTag("WandarPoint");

			Transform[] waypoints = new Transform[pointList.Length];
			int i = 0;
			foreach (GameObject obj in pointList)
			{
				waypoints[i] = obj.transform;
				i++;
			}
			
			PatrolState patrol = new PatrolState(this.ammoDestinations, this.transform);
			patrol.AddTransition(Transition.SawPlayer, FSMStateID.Attacking);
			patrol.AddTransition(Transition.NoHealth, FSMStateID.Dead);

			AttackState attack = new AttackState(waypoints, this.transform);
            attack.AddTransition(Transition.LowHealth, FSMStateID.Fleeing);
            attack.AddTransition(Transition.SawInvicible, FSMStateID.Strongering);
            attack.AddTransition(Transition.NoHealth, FSMStateID.Dead);

            FleeState flee = new FleeState(this.transform);
            flee.AddTransition(Transition.HighHealt, FSMStateID.Attacking);
            flee.AddTransition(Transition.SawInvicible, FSMStateID.Strongering);
            flee.AddTransition(Transition.NoHealth, FSMStateID.Dead);

            StrongerState stronger = new StrongerState(this.transform);
            stronger.AddTransition(Transition.SawPlayer, FSMStateID.Attacking);
            stronger.AddTransition(Transition.LowHealth, FSMStateID.Fleeing);
            stronger.AddTransition(Transition.NoHealth, FSMStateID.Dead);

            DeadState dead = new DeadState();
            dead.AddTransition(Transition.NoHealth, FSMStateID.Dead);

            this.AddFSMState(patrol);
			this.AddFSMState(attack);
            this.AddFSMState(flee);
            this.AddFSMState(stronger);
            this.AddFSMState(dead);
		}

		// Check collision with the bullet.
		void OnCollisionEnter(Collision collision)
		{
			//Reduce health
			if (collision.gameObject.CompareTag("Bullet"))
			{
				int bulletDamage = collision.gameObject.GetComponent<Bullet>().Damage;

				if (this.GetComponent<InvicibleEffect>() == null)
				{
					this.health -= bulletDamage;
					this.HealthSlideImage.fillAmount = (float)this.health / 100.0f;
                }

				if (this.health <= 0)
				{
					Debug.Log("Switch to Dead State");

                    GameManager.Instance.RemovePlayer(this.transform);

                    this.SetTransition(Transition.NoHealth);
					this.Explode();
				}
			}
			else if (collision.gameObject.tag == "Ammo")
			{ 
				collision.gameObject.SetActive(false);
			}
		}

		protected void Explode()
		{
			float rndX = UnityEngine.Random.Range(10.0f, 30.0f);
			float rndZ = UnityEngine.Random.Range(10.0f, 30.0f);

			Rigidbody npcRigidbody = this.GetComponent<Rigidbody>();
            for (int i = 0; i < 3; i++)
			{
				npcRigidbody.AddExplosionForce(10000.0f, this.transform.position - new Vector3(rndX, 10.0f, rndZ), 40.0f, 10.0f);
				npcRigidbody.velocity = this.transform.TransformDirection(new Vector3(rndX, 20.0f, rndZ));
			}

			Destroy(gameObject, 1.5f);
		}

		public bool IsAlive()
		{
			return health > 0;
		}

		/// <summary>
		/// Shoot the bullet from the turret
		/// </summary>
		public void ShootBullet()
		{
			if (this.elapsedTime >= this.shootRate)
			{
				if (this.Bullet == null)
				{
					Debug.LogError("No Bullet are attached to the NPC !");
					return;
				}

				GameObject bulletClone = Instantiate(this.Bullet, this.bulletSpawnPoint.position, this.bulletSpawnPoint.rotation) as GameObject;
				bulletClone.GetComponent<MeshRenderer>().material.color = this.playerColor;

				if (this.GetComponent<InvicibleEffect>() != null)
				{
					bulletClone.GetComponent<Bullet>().Damage *= 3;
				}

				this.elapsedTime = 0.0f;
			}
		}

		public string GetName()
		{
			return this.PlayerName;
		}

		public void SetColor(Color playerColor)
		{
			this.playerColor = playerColor;

			// TEMP.
			GameObject chassis = this.gameObject.transform.FindChild("TankRenderers/TankChassis").gameObject;
			chassis.GetComponent<MeshRenderer>().materials[0] = new Material(Shader.Find("Standard"));
			chassis.GetComponent<MeshRenderer>().materials[0].color = playerColor;

			GameObject turret = this.gameObject.transform.FindChild("TankRenderers/TankTurret").gameObject;
			turret.GetComponent<MeshRenderer>().materials[0] = new Material(Shader.Find("Standard"));
			turret.GetComponent<MeshRenderer>().materials[0].color = playerColor;

			GameObject trackLeft = this.gameObject.transform.FindChild("TankRenderers/TankTracksLeft").gameObject;
			trackLeft.GetComponent<MeshRenderer>().materials[0] = new Material(Shader.Find("Standard"));
			trackLeft.GetComponent<MeshRenderer>().materials[0].color = playerColor;

			GameObject trackRight = this.gameObject.transform.FindChild("TankRenderers/TankTracksRight").gameObject;
			trackRight.GetComponent<MeshRenderer>().materials[0] = new Material(Shader.Find("Standard"));
			trackRight.GetComponent<MeshRenderer>().materials[0].color = playerColor;
		}

		public void AddAmmoDestination(Transform newDestination)
		{
			if (this.ammoDestinations == null)
				this.ammoDestinations = new List<Transform>();

			this.ammoDestinations.Add(newDestination);
		}

		public void AddHealth(object sender, int value)
		{
			if (sender is HealthBonus)
			{
				this.health += value;
				if (this.health > 100)
					health = 100;

				this.HealthSlideImage.fillAmount = (float)this.health / 100.0f;
			}
		}

        public HealthBonus[] FindHealth()
        {
            healthPoints = new HealthBonus[0];
            healthPoints = FindObjectsOfType<HealthBonus>();
            if (healthPoints.Length > 0)
            {
                return healthPoints;
            }
            return null;
        }
        
        public InvicibleGameEvent[] FindInvicible()
        {
            inviciblePoints = new InvicibleGameEvent[0];
            inviciblePoints = FindObjectsOfType<InvicibleGameEvent>();
            if (inviciblePoints.Length > 0)
            {
                Debug.Log(inviciblePoints.Length);
                return inviciblePoints;
            }
            return null;
        }
    }
}