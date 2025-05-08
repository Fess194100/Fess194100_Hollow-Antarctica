using UnityEngine;
using System.Collections.Generic;

public class FX_ParticleSystem_Controller : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    public void SetEmissionRateForAll(float emissionRate)
    {
        foreach (var ps in particleSystems)
        {
            if (ps == null) continue;

            var emission = ps.emission;
            emission.rateOverTime = emissionRate;
        }
    }
}
