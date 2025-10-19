using UnityEngine;
using AdaptivEntityAgent;
using SimpleCharController;

public class FlagFaction : MonoBehaviour
{
    public EntityType flagFaction = EntityType.Neutral;

    [Space(10)]
    [Header("Helper References")]
    [SerializeField] private EssenceHealth targetHealth;

    public EssenceHealth EssenceHealth => targetHealth;
}
