using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SplitToningController : MonoBehaviour
{
    [SerializeField] private VolumeProfile volumeProfile;
    [SerializeField, Range(-100f, 100f)] private float balance = 0f;

    private SplitToning splitToning;

    private void OnValidate()
    {
        UpdateSplitToningBalance();
    }

    private void Start()
    {
        if (volumeProfile == null)
        {
            Debug.LogError("Volume Profile не назначен!");
            return;
        }

        // Попытка получить компонент SplitToning из Volume Profile
        if (!volumeProfile.TryGet(out splitToning))
        {
            Debug.LogError("SplitToning эффект не найден в Volume Profile!");
            return;
        }

        UpdateSplitToningBalance();
    }

    public void SetBalance(float newBalance)
    {
        balance = Mathf.Clamp(newBalance, -100f, 100f);
        UpdateSplitToningBalance();
    }

    private void UpdateSplitToningBalance()
    {
        if (splitToning != null)
        {
            splitToning.balance.overrideState = true;
            splitToning.balance.value = balance;
        }
    }
}