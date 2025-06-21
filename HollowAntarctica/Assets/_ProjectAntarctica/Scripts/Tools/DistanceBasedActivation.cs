using UnityEngine;
using System.Collections.Generic;

public class DistanceBasedActivation : MonoBehaviour
{
    public enum CheckModeDistanceActiv
    {
        FixedInterval,
        DynamicInterval
    }

    [Header("Settings")]
    public bool DeBag;

    [Space(10)]
    public CheckModeDistanceActiv checkMode = CheckModeDistanceActiv.FixedInterval;
    public Transform player;
    public float activationDistance = 20f;

    [Space(10)]
    public List<GameObject> targetObjects = new List<GameObject>();

    [Header("Fixed Interval")]
    public float checkInterval = 0.02f;

    [Header("Dynamic Interval")]
    public AnimationCurve intervalOverDistance;

    private float timer;
    private float nextCheckTime;

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            if (player == null)
            {
                Debug.LogError("Player transform not assigned and no GameObject with 'Player' tag found!");
                enabled = false;
                return;
            }
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextCheckTime)
        {
            CheckDistanceAndActivate();
            SetNextCheckTime();
            timer = 0f;
        }
    }

    private void CheckDistanceAndActivate()
    {
        if (player == null || targetObjects.Count == 0) return;

        foreach (GameObject obj in targetObjects)
        {
            if (obj == null) continue;

            float distanceToPlayer = Vector3.Distance(obj.transform.position, player.position);
            bool shouldBeActive = distanceToPlayer <= activationDistance;
            if (obj.activeSelf != shouldBeActive)
            {
                obj.SetActive(shouldBeActive);
            }
        }
    }

    private void SetNextCheckTime()
    {
        if (checkMode == CheckModeDistanceActiv.FixedInterval)
        {
            nextCheckTime = checkInterval;
        }
        else
        {
            if (player == null) return;

            nextCheckTime = intervalOverDistance.Evaluate(Vector3.Distance(transform.position, player.position));
            nextCheckTime = Mathf.Clamp(nextCheckTime, 0.02f, Mathf.Abs(nextCheckTime));
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (DeBag)
        {
            if (player != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, player.position);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, activationDistance);
        }        
    }
}