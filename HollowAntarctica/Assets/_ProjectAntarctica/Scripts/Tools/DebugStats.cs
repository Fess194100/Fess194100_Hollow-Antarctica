using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public class DebugStats : MonoBehaviour
{
    public bool useGUI = true;
    public Text UIText; // ��������� ������� UI ��� ����������� ����������
    public Vector2 textAreaPosition = new Vector2(10, 10); // ������� ������� ������
    public Vector2 textAreaSize = new Vector2(400, 200); // ������ ������� ������

    private string statsText;
    private ProfilerRecorder setPassCallsRecorder;
    private ProfilerRecorder drawCallsRecorder;
    private ProfilerRecorder verticesRecorder;
    private ProfilerRecorder trianglesRecorder; // ��� �������� �������������

    // ���������� ��� ������� FPS
    private float fps;
    private float updateInterval = 0.5f; // �������� ���������� FPS (2 ���� � �������)
    private float accum = 0f; // ����� FPS �� ��������
    private int frames = 0; // ���������� ������ �� ��������
    private float timeleft; // ���������� ����� �� ����������

    void OnEnable()
    {
        // ������������� ����������
        setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
        trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");

        timeleft = updateInterval; // ������������� ������� ��� FPS
    }

    void OnDisable()
    {
        // ������������ ��������
        setPassCallsRecorder.Dispose();
        drawCallsRecorder.Dispose();
        verticesRecorder.Dispose();
        trianglesRecorder.Dispose();
    }

    void Update()
    {
        // ������ FPS
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        if (timeleft <= 0f)
        {
            fps = accum / frames; // ��������� FPS
            timeleft = updateInterval;
            accum = 0f;
            frames = 0;
        }

        // ���� ����������
        var sb = new StringBuilder(500);

        // ��������� FPS � ����������
        sb.AppendLine($"FPS: {fps:0.}");

        if (setPassCallsRecorder.Valid && setPassCallsRecorder.LastValue > 0)
        {
            sb.AppendLine($"SetPass Calls: {setPassCallsRecorder.LastValue}");
        }

        if (drawCallsRecorder.Valid && drawCallsRecorder.LastValue > 0)
        {
            sb.AppendLine($"Draw Calls: {drawCallsRecorder.LastValue}");
        }

        if (verticesRecorder.Valid && verticesRecorder.LastValue > 0)
        {
            sb.AppendLine($"Vertices: {verticesRecorder.LastValue}");
        }

        if (trianglesRecorder.Valid && trianglesRecorder.LastValue > 0)
        {
            sb.AppendLine($"Triangles: {trianglesRecorder.LastValue}"); // ��������� ���������� �������������
        }

        statsText = sb.ToString();

        // ��������� ��������� ������� UI, ���� �� ����������
        if (UIText != null)
        {
            UIText.text = statsText;
        }
    }

    void OnGUI()
    {
        // ���������� ���������� � GUI.TextArea � �������������� �����������
        if (useGUI) GUI.TextArea(new Rect(textAreaPosition.x, textAreaPosition.y, textAreaSize.x, textAreaSize.y), statsText);
    }
}