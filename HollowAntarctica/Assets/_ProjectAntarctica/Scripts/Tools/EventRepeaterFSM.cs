using UnityEngine;
using UnityEngine.Events;

public class EventRepeaterFSM : MonoBehaviour
{
    public UnityEvent OnActivation;
    public UnityEvent OnDeActivation;

    public void Activation() => OnActivation?.Invoke();
    public void Deactivation() => OnDeActivation?.Invoke();
}
