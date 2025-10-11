using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Events;

public class AIMannequin : MonoBehaviour
{
    [Header("��������� ��������������")]
    public GameObject currentTarget;
    public GameObject[] waypoints; // ������ ����� ��������
    public float waitTime = 2f;    // ����� �������� �� �����

    [Space(10)]
    public UnityEvent OnNextPoint;

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isMovingForward = true;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (waypoints != null && waypoints.Length > 0)
        {
            MoveToWaypoint(currentWaypointIndex);
        }
        else
        {
            Debug.LogWarning("�� ��������� ����� ��������������!");
        }
    }

    void Update()
    {
        // ���� ����� ������ ���� � �� �������
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                StartCoroutine(WaitAndMoveToNext());
            }
        }
    }

    IEnumerator WaitAndMoveToNext()
    {
        isWaiting = true;

        // ���� ��������� �����
        yield return new WaitForSeconds(waitTime);

        // ���������� ��������� �����
        if (isMovingForward)
        {
            currentWaypointIndex++;

            // ���� �������� ��������� ����� - ������ �����������
            if (currentWaypointIndex >= waypoints.Length)
            {
                isMovingForward = false;
                currentWaypointIndex = waypoints.Length - 2;
            }
        }
        else
        {
            currentWaypointIndex--;

            // ���� �������� ������ ����� - ������ �����������
            if (currentWaypointIndex < 0)
            {
                isMovingForward = true;
                currentWaypointIndex = 1;
            }
        }

        // ��������� � ��������� �����
        MoveToWaypoint(currentWaypointIndex);
        isWaiting = false;
    }

    void MoveToWaypoint(int index)
    {
        if (waypoints != null && index >= 0 && index < waypoints.Length && waypoints[index] != null)
        {
            currentTarget = waypoints[index];
            agent.SetDestination(waypoints[index].transform.position);
            OnNextPoint.Invoke();
        }
    }

    // ������������ � ���������
    void OnDrawGizmosSelected()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].transform.position, 0.5f);

                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position);
                    }
                }
            }
        }
    }
}