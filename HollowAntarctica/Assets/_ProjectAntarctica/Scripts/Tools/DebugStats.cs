using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public class DebugStats : MonoBehaviour
{
    public bool useGUI = true;
    public Text UIText; // Текстовый элемент UI для отображения статистики
    public Vector2 textAreaPosition = new Vector2(10, 10); // Позиция области текста
    public Vector2 textAreaSize = new Vector2(400, 200); // Размер области текста

    private string statsText;
    private ProfilerRecorder setPassCallsRecorder;
    private ProfilerRecorder drawCallsRecorder;
    private ProfilerRecorder verticesRecorder;
    private ProfilerRecorder trianglesRecorder; // Для подсчета треугольников

    // Переменные для расчета FPS
    private float fps;
    private float updateInterval = 0.5f; // Интервал обновления FPS (2 раза в секунду)
    private float accum = 0f; // Сумма FPS за интервал
    private int frames = 0; // Количество кадров за интервал
    private float timeleft; // Оставшееся время до обновления

    void OnEnable()
    {
        // Инициализация статистики
        setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
        trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");

        timeleft = updateInterval; // Инициализация таймера для FPS
    }

    void OnDisable()
    {
        // Освобождение ресурсов
        setPassCallsRecorder.Dispose();
        drawCallsRecorder.Dispose();
        verticesRecorder.Dispose();
        trianglesRecorder.Dispose();
    }

    void Update()
    {
        // Расчет FPS
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        if (timeleft <= 0f)
        {
            fps = accum / frames; // Обновляем FPS
            timeleft = updateInterval;
            accum = 0f;
            frames = 0;
        }

        // Сбор статистики
        var sb = new StringBuilder(500);

        // Добавляем FPS в статистику
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
            sb.AppendLine($"Triangles: {trianglesRecorder.LastValue}"); // Добавляем количество треугольников
        }

        statsText = sb.ToString();

        // Обновляем текстовый элемент UI, если он существует
        if (UIText != null)
        {
            UIText.text = statsText;
        }
    }

    void OnGUI()
    {
        // Отображаем статистику в GUI.TextArea с настраиваемыми параметрами
        if (useGUI) GUI.TextArea(new Rect(textAreaPosition.x, textAreaPosition.y, textAreaSize.x, textAreaSize.y), statsText);
    }
}