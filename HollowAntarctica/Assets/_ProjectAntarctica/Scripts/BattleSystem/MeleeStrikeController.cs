using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleCharController;

public class MeleeStrikeController : MonoBehaviour
{
    #region Variables
    [Header("References")]
    [SerializeField] private CharController charController;
    [SerializeField] private Animator animator;

    [Space(10)]
    [SerializeField] private MeleeHit meleeHitKick;
    [SerializeField] private List <MeleeHit> meleeHitPunch;
    
    [Space(10)]
    [Header("Settings")]
    [SerializeField] private AnimationCurve directionKickAtAngleXCam;
    #endregion

    #region Public Property
    public bool IsKick => isKick;
    #endregion
    #region Private Variables
    private bool hasInitialized;
    private bool isKick = false;
    private bool isPunch = false;
    private float currentDamage;
    #endregion

    #region System Function
    private void Start()
    {
        hasInitialized = charController != null && animator != null;
    }
    #endregion

    #region Private Methods
    
    private void MelleKick(bool enable)
    {
        if (meleeHitKick == null) return;

        if (enable)
        {
            meleeHitKick.StartAttack(currentDamage);
        }
        else meleeHitKick.EndAttack();
    }

    private void MellePunch(bool enable)
    {
        if (meleeHitPunch == null) return;

        foreach (MeleeHit meleeHit in meleeHitPunch)
        {
            if (meleeHit == null) continue;

            if (enable) meleeHit.StartAttack(currentDamage);
            else meleeHit.EndAttack();
        }
    }
    #endregion

    #region IEnumerators
    private IEnumerator ReleaseKick(float cost, float duration)
    {
        isKick = true;
        bool previsionCanControl = charController.canControl;
        float cameraAngleX = Camera.main.transform.localEulerAngles.x;
        charController.canControl = false;
        charController.ChangeStamina(-cost);

        if (cameraAngleX > 180f) cameraAngleX -= 360f;

        animator.SetFloat("Direction", directionKickAtAngleXCam.Evaluate(cameraAngleX));
        animator.SetTrigger("Kick");
        float timer = 0f;

        while (timer < duration)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
        }

        isKick = false;
        if (!charController.isDead) charController.canControl = previsionCanControl;
    }

    private IEnumerator ReleaseAirKick(float cost)
    {
        isKick = true;
        charController.ChangeStamina(-cost * 2);
        animator.SetTrigger("Kick");
        StartMelleKick();
        yield return new WaitUntil(() => charController.isGrounded);
        EndMelleKick();
        isKick = false;
    }

    private IEnumerator ReleasePunch(float cost,float duration, float charged)
    {
        isPunch = true;
        charController.ChangeStamina(-cost);
        animator.SetFloat("Direction", charged);
        animator.SetTrigger("Punch");
        float timer = 0f;

        while (timer < duration)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
        }

        isPunch = false;
    }
    #endregion

    #region API
    public void HandlerKick(float cost, float damage, float duration)
    {
        if (hasInitialized)
        {
            if (!isKick && !charController.isClimbing && charController.canControl && charController.CurrentStamine >= cost)
            {
                currentDamage = damage;
                if (charController.isGrounded) StartCoroutine(ReleaseKick(cost, duration));
                else StartCoroutine(ReleaseAirKick(cost));
            }
        }
    }

    public void HandlerPunch(float cost, float damage, float duration, int charged)
    {
        if (hasInitialized)
        {
            if (!isKick && !isPunch && !charController.isClimbing && charController.canControl && charController.CurrentStamine >= cost)
            {
                currentDamage = damage;
                StartCoroutine(ReleasePunch(cost, duration, (float)charged));
            }
        }
    }
    public void StartMelleKick() => MelleKick(true);
    public void EndMelleKick() => MelleKick(false);
    public void StartMellePunch() => MellePunch(true);
    public void EndMellePunch() => MellePunch(false);
    #endregion
}
