using UnityEngine;
using System.Collections;
using System;

namespace Ulises
{
	public class DeadState : FSMState
	{
		public DeadState()
		{
			stateID = FSMStateID.Dead;
		}

		public override void Update(Transform npc)
		{

		}
	}

}