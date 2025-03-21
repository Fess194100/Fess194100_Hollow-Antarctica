using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BlendTextureGround : MonoBehaviour
{
    [Tooltip("Orthographic Camera")]
    public Camera camToDrawWith;
    public RenderTexture tempTex;

    private void Start()
    {
        DrawToMap();
    }
    public void DrawToMap()
    {
        camToDrawWith.enabled = true;
        camToDrawWith.targetTexture = tempTex;
        //camToDrawWith.depthTextureMode = DepthTextureMode.Depth;

        //the total width of the bounding box of our cameras view
        Shader.SetGlobalFloat("TB_SCALE", GetComponent<Camera>().orthographicSize * 2);
        //find the bottom corner of the texture in world scale by subtracting the size of the camera from its x and z position
        Shader.SetGlobalFloat("TB_OFFSET_X", camToDrawWith.transform.position.x - camToDrawWith.orthographicSize);
        Shader.SetGlobalFloat("TB_OFFSET_Z", camToDrawWith.transform.position.z - camToDrawWith.orthographicSize);
        //we'll also need the relative y position of the camera, lets get this by subtracting the far clip plane from the camera y position
        Shader.SetGlobalFloat("TB_OFFSET_Y", camToDrawWith.transform.position.y - camToDrawWith.farClipPlane);
        //we'll also need the far clip plane itself to know the range of y values in the depth texture
        Shader.SetGlobalFloat("TB_FARCLIP", camToDrawWith.farClipPlane);

        //camToDrawWith.Render();
        //RenderPipeline.beginCameraRendering += UpdateCamera; ;

        Shader.SetGlobalTexture("TB_DEPTH", tempTex);
        //camToDrawWith.enabled = false;
    }
}
