using UnityEngine;
using System.Collections;
using SimpleCharController;
using UnityEditor.Playables;

public class MeleeStrikeController : MonoBehaviour
{
    #region Variables
    [Header("References")]
    [SerializeField] private CharController charController;
    //[SerializeField] private WeaponController weaponController;
    [SerializeField] private MeleeHit meleeHitKick;
    [SerializeField] private MeleeHit meleeHitPunch;
    [SerializeField] private Animator animator;

    [Space(10)]
    [Header("Settings")]
    [SerializeField] private AnimationCurve directionStrike;
    #endregion

    #region Private Variables
    private bool hasInitialized;
    private bool isKick = false;
    private float currentDamage;
    #endregion

    #region System Function
    private void Start()
    {
        hasInitialized = charController != null && animator != null;
    }
    #endregion

    #region Private Methods
    private IEnumerator ReleaseKick(float cost, float duration)
    {
        isKick = true;
        bool previsionCanControl = charController.canControl;
        float cameraAngleX = Camera.main.transform.localEulerAngles.x;
        charController.canControl = false;
        charController.ChangeStamina(-cost);

        if (cameraAngleX > 180f) cameraAngleX -= 360f;

        animator.SetFloat("Direction", directionStrike.Evaluate(cameraAngleX));
        animator.SetTrigger("Kick");
        float timer = 0f;

        while (timer < duration)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
        }

        isKick = false;
        charController.canControl = previsionCanControl;
    }

    private IEnumerator ReleaseAirKick(float cost)
    {
        isKick = true;
        charController.ChangeStamina(-cost*2);
        animator.SetTrigger("Kick");
        StartMelleKick();
        yield return new WaitUntil(() => charController.isGrounded);
        EndMelleKick();
        isKick = false;
    }
    private void MelleKick(bool enable)
    {
        if (meleeHitKick == null) return;

        if (enable)
        {
            meleeHitKick.StartAttack(currentDamage);
        }
        else meleeHitKick.EndAttack();
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

    public void StartMelleKick() => MelleKick(true);
    public void EndMelleKick() => MelleKick(false);
    #endregion
}
