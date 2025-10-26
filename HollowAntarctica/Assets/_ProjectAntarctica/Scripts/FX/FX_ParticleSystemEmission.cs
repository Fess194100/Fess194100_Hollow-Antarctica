using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParticleSystemEmissionData
{
    public ParticleSystem particleSystem;
    public float emissionRate;
}

public class FX_ParticleSystemEmission : MonoBehaviour
{
    public List<ParticleSystemEmissionData> particleSystems = new List<ParticleSystemEmissionData>();
    [SerializeField] private float resetDelay = 2f;

    private Dictionary<ParticleSystem, float> originalEmissionRates = new Dictionary<ParticleSystem, float>();
    private Coroutine resetCoroutine;

    void Start()
    {
        CacheOriginalEmissionRates();
    }

    public void SetEmission(bool enableEmission)
    {
        if (particleSystems.Count == 0) return;

        if (enableEmission)
        {
            EnableEmission();
        }
        else
        {
            DisableEmission();
        }
    }

    public void SetEmissionRate(float emissionRate)
    {
        foreach (var data in particleSystems)
        {
            if (data.particleSystem != null)
            {
                var emission = data.particleSystem.emission;
                emission.rateOverTime = emissionRate;
            }
        }
        ScheduleEmissionReset();
    }

    private void EnableEmission()
    {
        foreach (var data in particleSystems)
        {
            if (data.particleSystem != null && originalEmissionRates.ContainsKey(data.particleSystem))
            {
                var emission = data.particleSystem.emission;
                emission.rateOverTime = data.emissionRate;
            }
        }
        ScheduleEmissionReset();
    }

    private void DisableEmission()
    {
        foreach (var data in particleSystems)
        {
            if (data.particleSystem != null)
            {
                var emission = data.particleSystem.emission;
                emission.rateOverTime = 0f;
            }
        }
    }

    private void ScheduleEmissionReset()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }
        resetCoroutine = StartCoroutine(ResetEmissionAfterDelay());
    }

    private System.Collections.IEnumerator ResetEmissionAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        DisableEmission();
        resetCoroutine = null;
    }

    private void CacheOriginalEmissionRates()
    {
        originalEmissionRates.Clear();
        foreach (var data in particleSystems)
        {
            if (data.particleSystem != null)
            {
                var emission = data.particleSystem.emission;
                if (emission.rateOverTime.mode == ParticleSystemCurveMode.Constant)
                {
                    originalEmissionRates[data.particleSystem] = emission.rateOverTime.constant;
                }
                else
                {
                    originalEmissionRates[data.particleSystem] = data.emissionRate;
                }
            }
        }
    }

    public void AddParticleSystem(ParticleSystem ps, float emissionRate)
    {
        var newData = new ParticleSystemEmissionData
        {
            particleSystem = ps,
            emissionRate = emissionRate
        };
        particleSystems.Add(newData);

        if (ps != null)
        {
            originalEmissionRates[ps] = emissionRate;
        }
    }

    public void RemoveParticleSystem(ParticleSystem ps)
    {
        particleSystems.RemoveAll(data => data.particleSystem == ps);
        originalEmissionRates.Remove(ps);
    }

    public void ClearAll()
    {
        particleSystems.Clear();
        originalEmissionRates.Clear();
    }

    public float ResetDelay
    {
        get => resetDelay;
        set => resetDelay = value;
    }

    public int ParticleSystemCount => particleSystems.Count;
}