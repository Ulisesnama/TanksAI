# TanksAI

## AI for tanks made in Unity

### AdvanceFSM: 
This class basically manages all the FSMState(s) implemented, and keeps updated with the transitions and the current state.

### FSMState:
Manages the transitions to other states. It has a dictionary object called map to store the key-value pairs of transitions and states.

### NPCTankController: 
Our tank AI, the NPCTankController class will inherit from AdvanceFSM.
