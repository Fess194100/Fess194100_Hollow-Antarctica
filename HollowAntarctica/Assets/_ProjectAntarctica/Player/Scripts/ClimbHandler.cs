using UnityEngine;
using SimpleCharController;

public class ClimbHandler : MonoBehaviour
{
    public ClimbingType climbingType;
    public Transform targetClimbObject;
    public Transform offClimbObject;
    public float boostSpeed = 1.0f;
    public float boostForceJumpOff = 0.2f;

    private CharController controller;

    private void Start()
    {
        //if (targetClimbObject == null) targetClimbObject = transform;
        //if (offClimbObject == null) offClimbObject = transform;

        targetClimbObject ??= transform;
        offClimbObject ??= transform;
    }
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
                if (!controller.isGrounded) controller.isClimbing = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (climbingType == ClimbingType.climbLadder && other.CompareTag("Player"))
        {
            if (controller != null && offClimbObject != null)
            {
                controller.offTargetClimb = offClimbObject;

                if (Vector3.Distance(controller.transform.position, offClimbObject.position) < 2f && !controller.isJumping)
                {
                    controller.isOffClimb = true;
                    StartCoroutine(controller.ClimbIsOff());
                } 
                else
                {
                    controller.isOffClimb = false;
                    controller.ExitModeClimb();
                } 
            }
            else Debug.Log("offClimbObject = null");
        }
        
        if (climbingType == ClimbingType.ropeLadder && other.CompareTag("Player"))
        {
            if (controller != null)
            {
                if (!controller.isJumping)
                {
                    controller.ExitModeClimb();
                }
            }
            else Debug.Log("offClimbObject = null");
        }
    }
}
