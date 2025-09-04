using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace SimpleCharController
{
    public class VFXChainLightningController : MonoBehaviour
    {
        [Header("Line Renderer Settings")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float updateFrequency = 0.05f; // Частота обновления в секундах

        [Header("Lightning Settings")]
        [SerializeField] private float lightningWidth = 0.1f;
        [SerializeField] private Color lightningColor = Color.cyan;
        [SerializeField] private Material lightningMaterial;

        private List<GameObject> _targets = new List<GameObject>();
        private float _updateTimer;
        private bool _isActive;

        private void Awake()
        {
            InitializeLineRenderer();
        }

        private void InitializeLineRenderer()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    lineRenderer = gameObject.AddComponent<LineRenderer>();
                }
            }

            // Настройка LineRenderer
            /*lineRenderer.startWidth = lightningWidth;
            lineRenderer.endWidth = lightningWidth;
            lineRenderer.material = lightningMaterial;
            lineRenderer.startColor = lightningColor;
            lineRenderer.endColor = lightningColor;*/
            lineRenderer.positionCount = 0; // Изначально нет точек
            lineRenderer.useWorldSpace = true; // Важно! Используем мировые координаты
        }

        public void InitializeChainLightning(List<GameObject> targets)
        {
            _targets = new List<GameObject>(targets);
            _isActive = true;
            _updateTimer = 0f;

            UpdateLineRendererPositions();
        }

        public void StopChainLightning()
        {
            _isActive = false;
            _targets.Clear();
            lineRenderer.positionCount = 0;
        }

        private void FixedUpdate()
        {
            if (!_isActive) return;

            _updateTimer += Time.fixedDeltaTime;

            if (_updateTimer >= updateFrequency)
            {
                UpdateLineRendererPositions();
                _updateTimer = 0f;
            }
        }

        private void UpdateLineRendererPositions()
        {
            if (_targets == null || _targets.Count == 0)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            // Количество позиций = количество целей + начальная точка
            int positionCount = _targets.Count + 1;
            lineRenderer.positionCount = positionCount;

            // Первая позиция - текущая позиция этого объекта (нулевая точка)
            lineRenderer.SetPosition(0, transform.position);

            // Остальные позиции - позиции целей
            for (int i = 0; i < _targets.Count; i++)
            {
                if (_targets[i] != null)
                {
                    lineRenderer.SetPosition(i + 1, _targets[i].transform.position);
                }
                else
                {
                    // Если цель уничтожена, используем последнюю известную позицию
                    lineRenderer.SetPosition(i + 1, lineRenderer.GetPosition(i));
                }
            }
        }

        public void AddTarget(GameObject newTarget)
        {
            if (!_targets.Contains(newTarget))
            {
                _targets.Add(newTarget);
                UpdateLineRendererPositions();
            }
        }

        public void RemoveTarget(GameObject targetToRemove)
        {
            if (_targets.Contains(targetToRemove))
            {
                _targets.Remove(targetToRemove);
                UpdateLineRendererPositions();
            }
        }

        public void ClearAllTargets()
        {
            _targets.Clear();
            lineRenderer.positionCount = 0;
        }

        // Метод для плавного исчезновения молнии
        public IEnumerator FadeOutLightning(float fadeDuration)
        {
            float elapsedTime = 0f;
            float startWidth = lightningWidth;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;

                // Плавно уменьшаем ширину линии
                float currentWidth = Mathf.Lerp(startWidth, 0f, progress);
                lineRenderer.startWidth = currentWidth;
                lineRenderer.endWidth = currentWidth;

                // Плавно уменьшаем альфу
                Color fadedColor = lightningColor;
                fadedColor.a = Mathf.Lerp(1f, 0f, progress);
                lineRenderer.startColor = fadedColor;
                lineRenderer.endColor = fadedColor;

                yield return null;
            }

            StopChainLightning();
        }

        private void OnDestroy()
        {
            StopChainLightning();
        }
    }
}