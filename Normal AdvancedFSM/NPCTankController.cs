using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class NPCTankController : AdvancedFSM 
{
    public GameObject Bullet;
    public int health;
    public UnityEngine.AI.NavMeshAgent agent;
    public TextMesh enemyLifeText;
    public float chaseDistance;
    public float attackDistance;
    public GameObject scoreTXT;
    private Score SC;
    public AudioSource ShotSound;
    //Initialize the Finite state machine for the NPC tank
    protected override void Initialize()
    {
		this.health = 100;
        this.chaseDistance = 80;
        this.attackDistance = 40;
        scoreTXT = GameObject.Find("[Score]");
        SC = scoreTXT.GetComponent<Score>();
        enemyLifeText.text = "Life:" + health.ToString();
        this.elapsedTime = 0.0f;
		this.shootRate = 2.0f;


        this.agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();

        //Get the target enemy(Player)
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        this.playerTransform = objPlayer.transform;

        if (!playerTransform)
            print("Player doesn't exist.. Please add one with Tag named 'Player'");

        //Get the turret of the tank
		this.turret = gameObject.transform.GetChild(0).transform;
		this.bulletSpawnPoint = turret.GetChild(0).transform;

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
		this.CurrentState.Reason(playerTransform, transform);
		this.CurrentState.Act(playerTransform, transform);
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
        foreach(GameObject obj in pointList)
        {
            waypoints[i] = obj.transform;
            i++;
        }

        //Get the list of life points
        pointList = GameObject.FindGameObjectsWithTag("LifePoint");
        Transform[] lifepoints = new Transform[pointList.Length];
        int j = 0;
        foreach (GameObject obj in pointList)
        {
            lifepoints[j] = obj.transform;
            j++;
        }

        PatrolState patrol = new PatrolState(waypoints, this.transform);
        patrol.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        patrol.AddTransition(Transition.LowHealth, FSMStateID.Fleeing);
        patrol.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        ChaseState chase = new ChaseState(waypoints, this.transform);
        chase.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        chase.AddTransition(Transition.ReachPlayer, FSMStateID.Attacking);
        chase.AddTransition(Transition.LowHealth, FSMStateID.Fleeing);
        chase.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        AttackState attack = new AttackState(waypoints);
        attack.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        attack.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        attack.AddTransition(Transition.LowHealth, FSMStateID.Fleeing);
        attack.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        FleeState flee = new FleeState(lifepoints, this.transform);
        flee.AddTransition(Transition.FullHealth, FSMStateID.Patrolling);
        flee.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        DeadState dead = new DeadState();
        dead.AddTransition(Transition.NoHealth, FSMStateID.Dead);

		this.AddFSMState(patrol);
		this.AddFSMState(chase);
		this.AddFSMState(attack);
        this.AddFSMState(flee);
		this.AddFSMState(dead);
    }

    /// <summary>
    /// Check the collision with the bullet
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if (collision.gameObject.tag == "Bullet")
        {
            this.health -= 50;
            if (this.health == 50)
            {
                Debug.Log("Switch to Flee State");
                this.SetTransition(Transition.LowHealth);
            }
            else if (this.health <= 0)
            {
                Debug.Log("Switch to Dead State");
                this.SetTransition(Transition.NoHealth);
                this.Explode();
            }
            enemyLifeText.text = "Life:" + health.ToString();

        }
    }
    void OnTriggerStay(Collider coll)
    {
        if (coll.gameObject.tag == "LifePoint")
        {
            if (this.health < 100)
            {
                Debug.Log("Increasing enemy health");
                this.health += 1;
                enemyLifeText.text = "Life:" + health.ToString();
            }
        }
    }
    protected void Explode()
    {
        float rndX = Random.Range(10.0f, 30.0f);
        float rndZ = Random.Range(10.0f, 30.0f);
        for (int i = 0; i < 3; i++)
        {
			this.GetComponent<Rigidbody>().AddExplosionForce(10000.0f, this.transform.position - new Vector3(rndX, 10.0f, rndZ), 40.0f, 10.0f);
			this.GetComponent<Rigidbody>().velocity = this.transform.TransformDirection(new Vector3(rndX, 20.0f, rndZ));
        }

        SC.scoreint += 1;
        SC.scoreText.text = "Score: " + SC.scoreint;
        Destroy(this.gameObject);
        //this.health = 100;
        //this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shoot the bullet from the turret
    /// </summary>
    public void ShootBullet()
    { 
		if (this.elapsedTime >= this.shootRate)
        {
            ShotSound.Play();
			Instantiate(this.Bullet, this.bulletSpawnPoint.position, this.bulletSpawnPoint.rotation);
			this.elapsedTime = 0.0f;
        }
    }

    public float FieldOfView = 80.0f;
    private int ViewDistance = 200;

    void OnDrawGizmos()
    {
        if (this.transform == null)
        {
            return;
        }
        if (this.playerTransform == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, chaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, attackDistance);

        Debug.DrawLine(this.transform.position, this.playerTransform.position, Color.red);

        Vector3 frontRayPoint = this.transform.position + (this.transform.forward * this.ViewDistance);

        //Approximate perspective visualization
        Vector3 leftRayPoint = frontRayPoint;
        leftRayPoint.x += this.FieldOfView * 1.0f;

        Vector3 rightRayPoint = frontRayPoint;
        rightRayPoint.x -= this.FieldOfView * 1.0f;

        Debug.DrawLine(this.transform.position, frontRayPoint, Color.green);
        Debug.DrawLine(this.transform.position, leftRayPoint, Color.green);
        Debug.DrawLine(this.transform.position, rightRayPoint, Color.green);
    }
}
