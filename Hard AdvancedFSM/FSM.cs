using UnityEngine;
using System.Collections;

namespace Ulises
{
    public class FSM : MonoBehaviour
    {
        //Player Transform
        protected Transform playerTransform;

        //Next destination position of the NPC Tank
        protected Vector3 destPos;

        //List of points for patrolling
        protected GameObject[] pointList;

        //Bullet shooting rate
        protected float shootRate;
        protected float elapsedTime;

        //Tank Turret
        public Transform turret { get; set; }
        public Transform bulletSpawnPoint { get; set; }

        protected virtual void Initialize() { }
        protected virtual void FSMUpdate() { }
        protected virtual void FSMFixedUpdate() { }

        // Use this for initialization
        protected virtual void Start()
        {
            this.Initialize();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            this.FSMUpdate();
        }

        protected virtual void FixedUpdate()
        {
            this.FSMFixedUpdate();
        }
    }
}