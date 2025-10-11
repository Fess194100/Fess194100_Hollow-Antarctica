using UnityEngine;

namespace AdaptivEntityAgent
{
    public class SoundEmitter : MonoBehaviour
    {
        [SerializeField] private float soundRange = 10f;
        [SerializeField] private LayerMask listenerMask = ~0;

        public void EmitSound(float volume = 1f)
        {
            Collider[] listeners = Physics.OverlapSphere(transform.position, soundRange * volume, listenerMask);

            foreach (Collider listener in listeners)
            {
                AgentPerception perception = listener.GetComponent<AgentPerception>();
                if (perception != null)
                {
                    perception.RegisterSound(transform.position, volume);
                }
            }
        }

        // Автоматическая эмиссия звука при определенных событиях
        private void OnCollisionEnter(Collision collision)
        {
            float impactVolume = Mathf.Clamp01(collision.relativeVelocity.magnitude * 0.1f);
            if (impactVolume > 0.1f)
            {
                EmitSound(impactVolume);
            }
        }
    }
}