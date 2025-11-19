using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Knockout.Characters.Components;

namespace Knockout.UI
{
    /// <summary>
    /// Displays and updates stamina bar UI for a character.
    /// Subscribes to CharacterStamina events and provides contextual display (hidden when full).
    /// </summary>
    public class StaminaBarUI : MonoBehaviour
    {
        [Header("Stamina Bar References")]
        [SerializeField] [Tooltip("The fill image that represents current stamina")]
        private Image staminaBarFill;

        [SerializeField] [Tooltip("Background image for the stamina bar")]
        private Image staminaBarBackground;

        [SerializeField] [Tooltip("Root GameObject for the entire stamina bar (for show/hide)")]
        private GameObject staminaBarRoot;

        [Header("Character Reference")]
        [SerializeField] [Tooltip("The character whose stamina to display")]
        private CharacterStamina characterStamina;

        [Header("Color Settings")]
        [SerializeField] [Tooltip("Color when stamina is high (>50%)")]
        private Color highStaminaColor = new Color(0f, 0.8f, 0f); // Green

        [SerializeField] [Tooltip("Color when stamina is medium (25-50%)")]
        private Color mediumStaminaColor = new Color(1f, 0.9f, 0f); // Yellow

        [SerializeField] [Tooltip("Color when stamina is low (<25%)")]
        private Color lowStaminaColor = new Color(1f, 0f, 0f); // Red

        [Header("Animation Settings")]
        [SerializeField] [Range(0.05f, 0.5f)] [Tooltip("Speed of stamina bar updates (lerp time)")]
        private float updateSpeed = 0.15f;

        [SerializeField] [Range(0.05f, 0.5f)] [Tooltip("Duration of depletion flash effect")]
        private float depletionFlashDuration = 0.2f;

        [SerializeField] [Tooltip("Color to flash when stamina depleted")]
        private Color depletionFlashColor = Color.white;

        [Header("Contextual Display Settings")]
        [SerializeField] [Tooltip("Hide stamina bar when at full (100%)")]
        private bool hideWhenFull = true;

        [SerializeField] [Tooltip("Pulse effect when stamina is low")]
        private bool pulseWhenLow = true;

        [SerializeField] [Range(0.5f, 2f)] [Tooltip("Pulse speed when stamina low")]
        private float pulseSpeed = 1f;

        // Internal state
        private float _targetFillAmount;
        private Coroutine _updateCoroutine;
        private Coroutine _flashCoroutine;
        private Coroutine _pulseCoroutine;
        private bool _isVisible = false;

        private void Awake()
        {
            ValidateReferences();

            // Initialize hidden if hideWhenFull is enabled
            if (hideWhenFull && staminaBarRoot != null)
            {
                staminaBarRoot.SetActive(false);
                _isVisible = false;
            }
        }

        private void Start()
        {
            if (characterStamina != null)
            {
                // Subscribe to stamina events
                characterStamina.OnStaminaChanged += OnStaminaChanged;
                characterStamina.OnStaminaDepleted += OnStaminaDepleted;

                // Initialize stamina bar to current stamina
                UpdateStaminaBar(characterStamina.CurrentStamina, characterStamina.MaxStamina);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (characterStamina != null)
            {
                characterStamina.OnStaminaChanged -= OnStaminaChanged;
                characterStamina.OnStaminaDepleted -= OnStaminaDepleted;
            }

            // Stop all coroutines
            StopAllCoroutines();
        }

        private void ValidateReferences()
        {
            if (staminaBarFill == null)
            {
                Debug.LogError("StaminaBarUI: Stamina bar fill image is not assigned!", this);
            }

            if (characterStamina == null)
            {
                Debug.LogWarning("StaminaBarUI: Character stamina reference is missing!", this);
            }

            if (staminaBarRoot == null && hideWhenFull)
            {
                Debug.LogWarning("StaminaBarUI: Stamina bar root is missing but hideWhenFull is enabled. Using this GameObject instead.", this);
                staminaBarRoot = gameObject;
            }
        }

        /// <summary>
        /// Called when character stamina changes.
        /// </summary>
        private void OnStaminaChanged(float currentStamina, float maxStamina)
        {
            UpdateStaminaBar(currentStamina, maxStamina);
        }

        /// <summary>
        /// Called when character stamina is depleted (reaches 0).
        /// </summary>
        private void OnStaminaDepleted()
        {
            TriggerDepletionFlash();
        }

        /// <summary>
        /// Updates the stamina bar fill amount, color, and visibility.
        /// </summary>
        private void UpdateStaminaBar(float currentStamina, float maxStamina)
        {
            if (staminaBarFill == null || maxStamina <= 0f)
            {
                return;
            }

            // Calculate target fill amount (0 to 1)
            _targetFillAmount = Mathf.Clamp01(currentStamina / maxStamina);

            // Update contextual visibility
            UpdateVisibility(_targetFillAmount);

            // Update color based on stamina percentage
            UpdateStaminaBarColor(_targetFillAmount);

            // Animate fill amount
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
            }
            _updateCoroutine = StartCoroutine(AnimateStaminaUpdate());

            // Handle pulse effect when low
            if (pulseWhenLow && _targetFillAmount < 0.25f && _targetFillAmount > 0f)
            {
                if (_pulseCoroutine == null)
                {
                    _pulseCoroutine = StartCoroutine(AnimateLowStaminaPulse());
                }
            }
            else
            {
                if (_pulseCoroutine != null)
                {
                    StopCoroutine(_pulseCoroutine);
                    _pulseCoroutine = null;

                    // Reset scale
                    if (staminaBarRoot != null)
                    {
                        staminaBarRoot.transform.localScale = Vector3.one;
                    }
                }
            }
        }

        /// <summary>
        /// Updates visibility based on stamina percentage.
        /// </summary>
        private void UpdateVisibility(float staminaPercentage)
        {
            if (!hideWhenFull || staminaBarRoot == null)
            {
                return;
            }

            bool shouldBeVisible = staminaPercentage < 1f;

            if (shouldBeVisible != _isVisible)
            {
                staminaBarRoot.SetActive(shouldBeVisible);
                _isVisible = shouldBeVisible;
            }
        }

        /// <summary>
        /// Animates smooth stamina bar updates.
        /// </summary>
        private IEnumerator AnimateStaminaUpdate()
        {
            float currentFill = staminaBarFill.fillAmount;
            float elapsed = 0f;

            while (elapsed < updateSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / updateSpeed;
                currentFill = Mathf.Lerp(staminaBarFill.fillAmount, _targetFillAmount, t);
                staminaBarFill.fillAmount = currentFill;
                yield return null;
            }

            // Snap to exact target
            staminaBarFill.fillAmount = _targetFillAmount;
        }

        /// <summary>
        /// Updates the stamina bar color based on stamina percentage.
        /// Gradients from green -> yellow -> red as stamina decreases.
        /// </summary>
        private void UpdateStaminaBarColor(float staminaPercentage)
        {
            Color targetColor;

            if (staminaPercentage > 0.5f)
            {
                // High stamina - green
                targetColor = highStaminaColor;
            }
            else if (staminaPercentage > 0.25f)
            {
                // Medium stamina - gradient from yellow to green
                float t = (staminaPercentage - 0.25f) / 0.25f; // Normalize to 0-1
                targetColor = Color.Lerp(mediumStaminaColor, highStaminaColor, t);
            }
            else
            {
                // Low stamina - gradient from red to yellow
                float t = staminaPercentage / 0.25f; // Normalize to 0-1
                targetColor = Color.Lerp(lowStaminaColor, mediumStaminaColor, t);
            }

            staminaBarFill.color = targetColor;
        }

        /// <summary>
        /// Triggers a brief flash effect when stamina is depleted.
        /// </summary>
        private void TriggerDepletionFlash()
        {
            if (staminaBarBackground == null)
            {
                return;
            }

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(AnimateDepletionFlash());
        }

        /// <summary>
        /// Animates the depletion flash effect.
        /// </summary>
        private IEnumerator AnimateDepletionFlash()
        {
            if (staminaBarBackground == null)
            {
                yield break;
            }

            // Store original color
            Color originalColor = staminaBarBackground.color;

            // Flash to white
            staminaBarBackground.color = depletionFlashColor;

            // Wait for flash duration
            yield return new WaitForSeconds(depletionFlashDuration);

            // Fade back to original color
            float elapsed = 0f;
            while (elapsed < depletionFlashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / depletionFlashDuration;
                staminaBarBackground.color = Color.Lerp(depletionFlashColor, originalColor, t);
                yield return null;
            }

            // Ensure we end on original color
            staminaBarBackground.color = originalColor;
        }

        /// <summary>
        /// Animates a pulsing effect when stamina is low.
        /// </summary>
        private IEnumerator AnimateLowStaminaPulse()
        {
            if (staminaBarRoot == null)
            {
                yield break;
            }

            while (true)
            {
                // Pulse from 1.0 to 1.1 scale
                float time = 0f;
                while (time < 1f / pulseSpeed)
                {
                    time += Time.deltaTime;
                    float t = Mathf.PingPong(time * pulseSpeed * 2f, 1f);
                    float scale = Mathf.Lerp(1f, 1.1f, t);
                    staminaBarRoot.transform.localScale = Vector3.one * scale;
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Public method to set character stamina reference at runtime.
        /// Useful for dynamic UI setup.
        /// </summary>
        public void SetCharacterStamina(CharacterStamina stamina)
        {
            // Unsubscribe from old reference
            if (characterStamina != null)
            {
                characterStamina.OnStaminaChanged -= OnStaminaChanged;
                characterStamina.OnStaminaDepleted -= OnStaminaDepleted;
            }

            // Set new reference
            characterStamina = stamina;

            // Subscribe to new reference
            if (characterStamina != null)
            {
                characterStamina.OnStaminaChanged += OnStaminaChanged;
                characterStamina.OnStaminaDepleted += OnStaminaDepleted;

                // Initialize to current stamina
                UpdateStaminaBar(characterStamina.CurrentStamina, characterStamina.MaxStamina);
            }
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate colors are visible
            if (highStaminaColor.a < 0.5f || mediumStaminaColor.a < 0.5f || lowStaminaColor.a < 0.5f)
            {
                Debug.LogWarning("StaminaBarUI: Stamina bar colors have low alpha, may not be visible!", this);
            }

            // Validate root reference if hideWhenFull enabled
            if (hideWhenFull && staminaBarRoot == null)
            {
                Debug.LogWarning("StaminaBarUI: hideWhenFull is enabled but staminaBarRoot is not assigned!", this);
            }
        }
        #endif
    }
}
