using System;
using UnityEngine;
using UnityEngine.Events;

namespace AdaptivEntityAgent
{
    [Serializable]
    public class AgentEvents
    {
        public UnityEvent<AgentState> OnStateChanged;
    }

    [Serializable]
    public class AgentEventsPerception
    {
        public UnityEvent<GameObject> OnTargetSpotted;
        public UnityEvent<GameObject> OnTargetLost;
        public UnityEvent<GameObject> OnTargetChanged;
        public UnityEvent<Vector3> OnSoundHeard;
    }

    [Serializable]
    public class AgentEventsCombat
    {
        public UnityEvent OnAttackMelle;
        public UnityEvent OnAttackRange;
        public UnityEvent OnFleeAgent;
    }

    [Serializable]
    public class AgentEventsMovement
    {
        public UnityEvent<Vector3> OnMoveToNextPosition;
        public UnityEvent OnStartInteract;
        public UnityEvent OnEndInteract;
    }
}