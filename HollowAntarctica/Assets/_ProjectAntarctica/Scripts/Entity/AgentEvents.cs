using System;
using UnityEngine;
using UnityEngine.Events;

namespace AdaptivEntityAgent
{
    [Serializable]
    public class AgentEvents
    {
        public UnityEvent OnAIActivated;
        public UnityEvent OnAIDeactivated;
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
        //public UnityEvent<GameObject> OnAttackStarted;
        //public UnityEvent<GameObject> OnAttackCompleted;
        //public UnityEvent<float> OnDamageTaken;
    }

    [Serializable]
    public class AgentEventsMovement //?????
    {
        public UnityEvent<AgentState> OnStateChanged;
        public UnityEvent<Vector3> OnDestinationReached;
    }
}