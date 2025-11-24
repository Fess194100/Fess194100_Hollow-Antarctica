using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleCharController
{
    [System.Serializable]
    public class WeaponColorSet
    {
        public string name;
        public Color baseColor;
        [ColorUsage(true, true)]
        public Color emissionColor;
    }

    public class WeaponVisualController : MonoBehaviour
    {
        #region Variables
        [Header("Color Settings")]
        public List<WeaponColorSet> colorSets = new List<WeaponColorSet>();
        public float colorTransitionTime = 0.5f;

        [Header("Material References")]
        //public Renderer weaponRenderer;
        public Material weaponMaterial;
        #endregion

        #region Private Variables
        private Color _currentBaseColor;
        private Color _currentEmissionColor;
        private Color _targetBaseColor;
        private Color _targetEmissionColor;
        private float _colorTransitionProgress = 1f;
        private Coroutine _colorTransitionCoroutine;

        // Property IDs for better performance
        private int _baseColorID;
        private int _emissionColorID;
        #endregion

        #region System Function
        void Start()
        {
            InitializeMaterial();
        }

        private void InitializeMaterial()
        {

            if (weaponMaterial != null)
            {
                // Cache property IDs
                _baseColorID = Shader.PropertyToID("_BaseColor");
                _emissionColorID = Shader.PropertyToID("_EmissionColor");

                // Initialize current colors from material
                _currentBaseColor = weaponMaterial.GetColor(_baseColorID);
                _currentEmissionColor = weaponMaterial.GetColor(_emissionColorID);
            }
            else
            {
                Debug.LogError("WeaponRendererMaterial not found!");
            }
        }
        #endregion

        #region Private Methods

        private void SetTargetColors(int colorSetIndex)
        {
            if (colorSets == null || colorSets.Count == 0)
            {
                Debug.LogWarning("Color sets list is empty!");
                return;
            }

            if (colorSetIndex < 0 || colorSetIndex >= colorSets.Count)
            {
                Debug.LogWarning($"Color set index {colorSetIndex} is out of range!");
                return;
            }

            var targetSet = colorSets[colorSetIndex];
            _targetBaseColor = targetSet.baseColor;
            _targetEmissionColor = targetSet.emissionColor;

            StartColorTransition();
        }

        private void StartColorTransition()
        {
            if (_colorTransitionCoroutine != null)
                StopCoroutine(_colorTransitionCoroutine);

            _colorTransitionCoroutine = StartCoroutine(ColorTransitionRoutine());
        }

        private void ApplyColorsToMaterial()
        {
            if (weaponMaterial != null)
            {
                weaponMaterial.SetColor(_baseColorID, _currentBaseColor);
                weaponMaterial.SetColor(_emissionColorID, _currentEmissionColor);
            }
        }

        #endregion

        #region Enumerators
        private IEnumerator ColorTransitionRoutine()
        {
            _colorTransitionProgress = 0f;

            Color startBaseColor = _currentBaseColor;
            Color startEmissionColor = _currentEmissionColor;

            while (_colorTransitionProgress < 1f)
            {
                _colorTransitionProgress += Time.deltaTime / colorTransitionTime;
                _colorTransitionProgress = Mathf.Clamp01(_colorTransitionProgress);

                // »нтерпол€ци€ цветов
                _currentBaseColor = Color.Lerp(startBaseColor, _targetBaseColor, _colorTransitionProgress);
                _currentEmissionColor = Color.Lerp(startEmissionColor, _targetEmissionColor, _colorTransitionProgress);

                // ѕрименение цветов к материалу
                ApplyColorsToMaterial();

                yield return null;
            }

            // ‘инальное применение на случай неточности интерпол€ции
            _currentBaseColor = _targetBaseColor;
            _currentEmissionColor = _targetEmissionColor;
            ApplyColorsToMaterial();

            _colorTransitionCoroutine = null;
        }
        #endregion

        #region Public API
        public void BlockedWeapon() => SetTargetColors(0);

        public void HandleProjectileTypeChanged(ProjectileType projectileType)
        {
            int colorIndex = projectileType switch
            {
                ProjectileType.Green => 1,
                ProjectileType.Blue => 2,
                ProjectileType.Orange => 3,
                _ => 1
            };

            SetTargetColors(colorIndex);
        }

        /*public void ChangeColors(int colorSetIndex)
        {
            SetTargetColors(colorSetIndex);
        }

        public void ChangeColorsImmediate(int colorSetIndex)
        {
            if (colorSetIndex >= 0 && colorSetIndex < colorSets.Count)
            {
                var targetSet = colorSets[colorSetIndex];
                _currentBaseColor = targetSet.baseColor;
                _currentEmissionColor = targetSet.emissionColor;
                ApplyColorsToMaterial();
            }
        }*/
        #endregion
    }
}