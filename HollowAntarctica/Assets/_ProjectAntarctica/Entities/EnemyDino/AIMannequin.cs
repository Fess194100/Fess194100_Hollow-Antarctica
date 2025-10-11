using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Events;

public class AIMannequin : MonoBehaviour
{
    [Header("Настройки патрулирования")]
    public GameObject currentTarget;
    public GameObject[] waypoints; // Массив точек движения
    public float waitTime = 2f;    // Время ожидания на точке

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
            Debug.LogWarning("Не назначены точки патрулирования!");
        }
    }

    void Update()
    {
        // Если агент достиг цели и не ожидает
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

        // Ждем указанное время
        yield return new WaitForSeconds(waitTime);

        // Определяем следующую точку
        if (isMovingForward)
        {
            currentWaypointIndex++;

            // Если достигли последней точки - меняем направление
            if (currentWaypointIndex >= waypoints.Length)
            {
                isMovingForward = false;
                currentWaypointIndex = waypoints.Length - 2;
            }
        }
        else
        {
            currentWaypointIndex--;

            // Если достигли первой точки - меняем направление
            if (currentWaypointIndex < 0)
            {
                isMovingForward = true;
                currentWaypointIndex = 1;
            }
        }

        // Двигаемся к следующей точке
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

    // Визуализация в редакторе
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