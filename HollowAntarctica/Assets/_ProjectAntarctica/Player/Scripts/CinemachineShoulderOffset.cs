using UnityEngine;
using Cinemachine.Utility;
using Cinemachine;

[AddComponentMenu("")] // Hide in menu
[ExecuteAlways]
[SaveDuringPlay]
public class CinemachineShoulderOffset : CinemachineExtension
{
    [Header("Shoulder Offset Settings")]
    [Tooltip("Horizontal shoulder offset (left/right)")]
    public float m_ShoulderOffset = 0.5f;

    [Tooltip("Which shoulder to view from (1=right, -1=left)")]
    public float m_CameraSide = 1f;

    [Tooltip("When to apply the offset")]
    public CinemachineCore.Stage m_ApplyAfter = CinemachineCore.Stage.Body;

    // Cache the 3rd person follow component
    private Cinemachine3rdPersonFollow m_3rdPersonFollow;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage != m_ApplyAfter)
            return;

        // Get the 3rd person follow component if we don't have it
        if (m_3rdPersonFollow == null)
        {
            var virtualCamera = vcam as CinemachineVirtualCamera;
            if (virtualCamera != null)
            {
                m_3rdPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            }
        }

        // Apply the settings if we have the component
        if (m_3rdPersonFollow != null)
        {
            m_3rdPersonFollow.ShoulderOffset.x = m_ShoulderOffset;
            m_3rdPersonFollow.CameraSide = m_CameraSide;
        }
    }

    /// <summary>
    /// Set camera side (1 for right, -1 for left)
    /// </summary>
    public void SetCameraSide(float side)
    {
        m_CameraSide = Mathf.Sign(side); // Ensure it's either 1 or -1
    }

    /// <summary>
    /// Toggle between left and right camera sides
    /// </summary>
    public void ToggleCameraSide()
    {
        m_CameraSide *= -1f;
    }

    /// <summary>
    /// Set shoulder offset value
    /// </summary>
    public void SetShoulderOffset(float offset)
    {
        m_ShoulderOffset = offset;
    }
}