using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is adapted and modified from the FSM implementation class available on UnifyCommunity website
/// The license for the code is Creative Commons Attribution Share Alike.
/// It's originally the port of C++ FSM implementation mentioned in Chapter01 of Game Programming Gems 1
/// You're free to use, modify and distribute the code in any projects including commercial ones.
/// Please read the link to know more about CCA license @http://creativecommons.org/licenses/by-sa/3.0/

/// This class represents the States in the Finite State System.
/// Each state has a Dictionary with pairs (transition-state) showing
/// which state the FSM should be if a transition is fired while this state
/// is the current state.
/// Reason method is used to determine which transition should be fired .
/// Act method has the code to perform the actions the NPC is supposed to do if it´s on this state.
/// </summary>

namespace Ulises
{
    public abstract class FSMState
    {
        protected float currentRotationSpeed = 4.0f;
        protected float currentSpeed = 100.0f;

        protected Dictionary<Transition, FSMStateID> map = new Dictionary<Transition, FSMStateID>();
        protected FSMStateID stateID;
        public FSMStateID ID
        {
            get
            {
                return stateID;
            }
        }

        protected Vector3 destPosition = Vector3.zero;
        protected Vector3 previousDestPosition = Vector3.zero;

        protected List<Transform> destPoints = new List<Transform>();

        protected NavMeshAgent navMeshAgent;

        protected void SetNavMeshAgent(Transform tankTransform)
        {
            if (tankTransform == null)
                return;

            this.navMeshAgent = tankTransform.GetComponent<NavMeshAgent>();
        }

        public void AddTransition(Transition transition, FSMStateID id)
        {
            // Check if anyone of the args is invallid
            if (transition == Transition.None || id == FSMStateID.None)
            {
                Debug.LogWarning("FSMState : Null transition not allowed");
                return;
            }

            //Since this is a Deterministc FSM,
            //Check if the current transition was already inside the map
            if (this.map.ContainsKey(transition))
            {
                Debug.LogWarning("FSMState ERROR: transition is already inside the map");
                return;
            }

            this.map.Add(transition, id);
            Debug.Log("Added : " + transition + " with ID : " + id);
        }

        /// <summary>
        /// This method deletes a pair transition-state from this state´s map.
        /// If the transition was not inside the state´s map, an ERROR message is printed.
        /// </summary>
        public void DeleteTransition(Transition trans)
        {
            // Check for NullTransition
            if (trans == Transition.None)
            {
                Debug.LogError("FSMState ERROR: NullTransition is not allowed");
                return;
            }

            // Check if the pair is inside the map before deleting
            if (this.map.ContainsKey(trans))
            {
                map.Remove(trans);
                return;
            }

            Debug.LogError("FSMState ERROR: Transition passed was not on this State´s List");
        }


        /// <summary>
        /// This method returns the new state the FSM should be if
        ///    this state receives a transition  
        /// </summary>
        public FSMStateID GetOutputState(Transition trans)
        {
            // Check for NullTransition
            if (trans == Transition.None)
            {
                Debug.LogError("FSMState ERROR: NullTransition is not allowed");
                return FSMStateID.None;
            }

            // Check if the map has this transition
            if (this.map.ContainsKey(trans))
            {
                return map[trans];
            }

            Debug.LogError("FSMState ERROR: " + trans + " Transition passed to the State was not on the list");
            return FSMStateID.None;
        }

        public abstract void Update(Transform npc);

        /// <summary>
        /// Find the next semi-random patrol point
        /// </summary>
        public bool FindNextPoint()
        {
            this.previousDestPosition = this.destPosition;

            if (this.destPoints == null)
                return false;

            if (this.destPoints.Count == 0)
                return false;

            int position = 0;
            Ulises.NPCController Tank = GameObject.FindObjectOfType<Ulises.NPCController>();

            if (Tank == null)
            {
                this.destPosition = this.destPoints[position].position;
                this.destPoints.RemoveAt(position);
                return true;
            }

            for (int i = 0; i < destPoints.Count; i++)
            {
                if (Vector3.Distance(Tank.gameObject.transform.position, destPoints[i].position) < Vector3.Distance(Tank.gameObject.transform.position, destPoints[position].position))
                {
                    position = i;
                }
            }
            this.destPosition = this.destPoints[position].position;
            this.destPoints.RemoveAt(position);

            return true;
        }

        public int GetClosest(Transform[] transforms, Transform npc)
        {
            int position = 0;
            if (transforms != null && transforms.Length > 0 && this.navMeshAgent != null)
            {
                float finalDest = 0;
                for (int i = 0; i < transforms.Length; i++)
                {
                    float DistNpcPlayer = (transforms[i].position - npc.position).sqrMagnitude;
                    if (finalDest == 0)
                    {
                        finalDest = DistNpcPlayer;
                        position = i;
                    }
                    else if (DistNpcPlayer < finalDest)
                    {
                        finalDest = DistNpcPlayer;
                        position = i;
                    }
                }
            }
            return position;
        }

        /// <summary>
        /// Check whether the next random position is the same as current tank position
        /// </summary>
        /// <param name="pos">position to check</param>
        protected bool IsInCurrentRange(Transform trans, Vector3 pos)
        {
            float xPos = Mathf.Abs(pos.x - trans.position.x);
            float zPos = Mathf.Abs(pos.z - trans.position.z);

            if (xPos <= 50.0f && zPos <= 50.0f)
                return true;

            return false;
        }
    }
}
