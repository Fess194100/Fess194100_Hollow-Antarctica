namespace AdaptivEntityAgent
{
    // Base state agent
    public enum AgentState
    {
        Idle,
        Patrol,
        Investigate,
        Combat,
        Flee,
        Follow,
        Interact,
        Alert
    }

    // Type entity
    public enum EntityType
    {
        Enemy,
        Ally,
        Neutral
    }

    // Type Attack
    public enum AttackType
    {
        Melee,
        Ranged
    }
}