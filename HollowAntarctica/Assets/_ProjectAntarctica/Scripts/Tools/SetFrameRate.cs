using UnityEngine;

public class SetFrameRate : MonoBehaviour
{
    public void FrameRate(int frameRate) => Application.targetFrameRate = frameRate;
}