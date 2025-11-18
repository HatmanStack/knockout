using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Knockout.Characters.Components;

namespace Knockout.UI
{
    /// <summary>
    /// Displays and updates health bar UI for a character.
    /// Subscribes to CharacterHealth events and provides visual feedback for damage.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("Health Bar References")]
        [SerializeField] [Tooltip("The fill image that represents current health")]
        private Image healthBarFill;

        [SerializeField] [Tooltip("Background image for the health bar")]
        private Image healthBarBackground;

        [Header("Character Reference")]
        [SerializeField] [Tooltip("The character whose health to display")]
        private CharacterHealth characterHealth;

        [Header("Color Settings")]
        [SerializeField] [Tooltip("Color when health is full (>75%)")]
        private Color healthyColor = new Color(0f, 0.8f, 0f); // Green

        [SerializeField] [Tooltip("Color when health is medium (25-75%)")]
        private Color warningColor = new Color(1f, 0.9f, 0f); // Yellow

        [SerializeField] [Tooltip("Color when health is low (<25%)")]
        private Color criticalColor = new Color(1f, 0f, 0f); // Red

        [Header("Animation Settings")]
        [SerializeField] [Range(0.1f, 1f)] [Tooltip("Speed of health bar depletion animation")]
        private float depletionSpeed = 0.5f;

        [SerializeField] [Range(0.05f, 0.5f)] [Tooltip("Duration of damage flash effect")]
        private float flashDuration = 0.15f;

        [SerializeField] [Tooltip("Color to flash when damage is taken")]
        private Color damageFlashColor = Color.white;

        // Internal state
        private float _targetFillAmount;
        private Coroutine _depletionCoroutine;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            if (characterHealth != null)
            {
                // Subscribe to health changed event
                characterHealth.OnHealthChanged += OnHealthChanged;

                // Initialize health bar to current health
                UpdateHealthBar(characterHealth.CurrentHealth, characterHealth.MaxHealth);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (characterHealth != null)
            {
                characterHealth.OnHealthChanged -= OnHealthChanged;
            }
        }

        private void ValidateReferences()
        {
            if (healthBarFill == null)
            {
                Debug.LogError("HealthBarUI: Health bar fill image is not assigned!", this);
            }

            if (characterHealth == null)
            {
                Debug.LogWarning("HealthBarUI: Character health reference is missing!", this);
            }
        }

        /// <summary>
        /// Called when character health changes.
        /// </summary>
        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            UpdateHealthBar(currentHealth, maxHealth);
            TriggerDamageFlash();
        }

        /// <summary>
        /// Updates the health bar fill amount and color.
        /// </summary>
        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            if (healthBarFill == null || maxHealth <= 0f)
            {
                return;
            }

            // Calculate target fill amount (0 to 1)
            _targetFillAmount = Mathf.Clamp01(currentHealth / maxHealth);

            // Update color based on health percentage
            UpdateHealthBarColor(_targetFillAmount);

            // Animate depletion
            if (_depletionCoroutine != null)
            {
                StopCoroutine(_depletionCoroutine);
            }
            _depletionCoroutine = StartCoroutine(AnimateHealthDepletion());
        }

        /// <summary>
        /// Animates smooth health bar depletion.
        /// </summary>
        private IEnumerator AnimateHealthDepletion()
        {
            float currentFill = healthBarFill.fillAmount;

            while (Mathf.Abs(currentFill - _targetFillAmount) > 0.001f)
            {
                currentFill = Mathf.Lerp(currentFill, _targetFillAmount, Time.deltaTime / depletionSpeed);
                healthBarFill.fillAmount = currentFill;
                yield return null;
            }

            // Snap to exact target
            healthBarFill.fillAmount = _targetFillAmount;
        }

        /// <summary>
        /// Updates the health bar color based on health percentage.
        /// Gradients from green -> yellow -> red as health decreases.
        /// </summary>
        private void UpdateHealthBarColor(float healthPercentage)
        {
            Color targetColor;

            if (healthPercentage > 0.75f)
            {
                // Healthy - green
                targetColor = healthyColor;
            }
            else if (healthPercentage > 0.25f)
            {
                // Warning - gradient from green to yellow
                float t = (healthPercentage - 0.25f) / 0.5f; // Normalize to 0-1
                targetColor = Color.Lerp(warningColor, healthyColor, t);
            }
            else
            {
                // Critical - gradient from yellow to red
                float t = healthPercentage / 0.25f; // Normalize to 0-1
                targetColor = Color.Lerp(criticalColor, warningColor, t);
            }

            healthBarFill.color = targetColor;
        }

        /// <summary>
        /// Triggers a brief flash effect when damage is taken.
        /// </summary>
        private void TriggerDamageFlash()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(AnimateDamageFlash());
        }

        /// <summary>
        /// Animates the damage flash effect.
        /// </summary>
        private IEnumerator AnimateDamageFlash()
        {
            if (healthBarBackground == null)
            {
                yield break;
            }

            // Store original color
            Color originalColor = healthBarBackground.color;

            // Flash to white
            healthBarBackground.color = damageFlashColor;

            // Wait for flash duration
            yield return new WaitForSeconds(flashDuration);

            // Fade back to original color
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                healthBarBackground.color = Color.Lerp(damageFlashColor, originalColor, t);
                yield return null;
            }

            // Ensure we end on original color
            healthBarBackground.color = originalColor;
        }

        /// <summary>
        /// Public method to set character health reference at runtime.
        /// Useful for dynamic UI setup.
        /// </summary>
        public void SetCharacterHealth(CharacterHealth health)
        {
            // Unsubscribe from old reference
            if (characterHealth != null)
            {
                characterHealth.OnHealthChanged -= OnHealthChanged;
            }

            // Set new reference
            characterHealth = health;

            // Subscribe to new reference
            if (characterHealth != null)
            {
                characterHealth.OnHealthChanged += OnHealthChanged;

                // Initialize to current health
                UpdateHealthBar(characterHealth.CurrentHealth, characterHealth.MaxHealth);
            }
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate colors are visible
            if (healthyColor.a < 0.5f || warningColor.a < 0.5f || criticalColor.a < 0.5f)
            {
                Debug.LogWarning("HealthBarUI: Health bar colors have low alpha, may not be visible!", this);
            }
        }
        #endif
    }
}
