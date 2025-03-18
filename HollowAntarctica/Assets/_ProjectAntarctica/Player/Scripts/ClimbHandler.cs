using UnityEngine;
using SimpleCharController;

public class ClimbHandler : MonoBehaviour
{
    public ClimbingType climbingType;
    public Transform targetClimbObject;
    public float boostSpeed = 1.0f;
    public float boostForceJumpOff = 0.2f;

    private CharController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller = other.GetComponent<CharController>();

            if (controller != null)
            {
                controller.currentTargetClimb = targetClimbObject;
                controller.boostClimbSpeed = boostSpeed;
                controller.jumpForceOffClimb = boostForceJumpOff;
                controller.climbingType = climbingType;
                controller.isClimbing = true;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (climbingType == ClimbingType.ropeLadder && other.CompareTag("Player"))
        {
            if (controller != null)
            {
                controller.isClimbing = true;
            }
        }
    }
}
