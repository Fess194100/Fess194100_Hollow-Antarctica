using SimpleCharController;
using UnityEngine;
using UnityEngine.Events;

public class Sound_ControllerBattleSystem : MonoBehaviour
{
    public AudioSource audioSourceShot;
    public UnityEvent startCharging;
    public UnityEvent stopCharging;
    public UnityEvent Overload;

    public void PlaySound(AudioClip audioClip, Vector2 pitch)
    {
        audioSourceShot.pitch = Random.Range(pitch.x, pitch.y);
        audioSourceShot.PlayOneShot(audioClip);
    }

    public void HandlerWeaponState(WeaponState weaponState )
    {
        switch (weaponState)
        {
            case WeaponState.Charging:
                startCharging.Invoke();
                break;
            case WeaponState.Firing:
                stopCharging.Invoke();
                break;
            case WeaponState.Ready:
                stopCharging.Invoke();
                break;
            case WeaponState.Overloaded:
                Overload.Invoke();
                break;
        }
    }
}
