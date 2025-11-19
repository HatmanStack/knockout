using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.UI
{
    /// <summary>
    /// Displays special move cooldown indicator showing readiness state.
    /// Updates continuously to show cooldown progress and indicates when ready to use.
    /// </summary>
    public class SpecialMoveCooldownUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] [Tooltip("Background image for the special move icon")]
        private Image iconBackground;

        [SerializeField] [Tooltip("Icon image for the special move")]
        private Image iconImage;

        [SerializeField] [Tooltip("Cooldown fill overlay (radial or linear fill)")]
        private Image cooldownFillOverlay;

        [SerializeField] [Tooltip("Text for cooldown timer (optional)")]
        private TextMeshProUGUI cooldownTimerText;

        [SerializeField] [Tooltip("Ready indicator (glow, text, or image)")]
        private GameObject readyIndicator;

        [Header("Character References")]
        [SerializeField] [Tooltip("The character's special moves component")]
        private CharacterSpecialMoves specialMoves;

        [SerializeField] [Tooltip("The character's stamina component (for dual-gating check)")]
        private CharacterStamina characterStamina;

        [Header("Color Settings")]
        [SerializeField] [Tooltip("Color for icon when ready")]
        private Color readyColor = Color.white;

        [SerializeField] [Tooltip("Color for icon when on cooldown")]
        private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray

        [SerializeField] [Tooltip("Color for icon when insufficient stamina")]
        private Color insufficientStaminaColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Darker gray

        [SerializeField] [Tooltip("Color for cooldown overlay")]
        private Color overlayColor = new Color(0f, 0f, 0f, 0.7f); // Dark semi-transparent

        [SerializeField] [Tooltip("Color for cooldown timer text")]
        private Color timerTextColor = Color.white;

        [Header("Animation Settings")]
        [SerializeField] [Tooltip("Enable pulse animation when ready")]
        private bool pulseWhenReady = true;

        [SerializeField] [Range(0.5f, 3f)] [Tooltip("Pulse speed when ready")]
        private float pulseSpeed = 1.5f;

        [SerializeField] [Range(1f, 1.5f)] [Tooltip("Pulse scale multiplier")]
        private float pulseScale = 1.2f;

        [Header("Display Settings")]
        [SerializeField] [Tooltip("Show numerical countdown timer")]
        private bool showCountdownTimer = true;

        [SerializeField] [Tooltip("Use radial fill for cooldown overlay")]
        private bool useRadialFill = true;

        // Internal state
        private SpecialMoveData _specialMoveData;
        private Coroutine _pulseCoroutine;
        private bool _isReady;
        private bool _hasEnoughStamina;

        private void Awake()
        {
            ValidateReferences();
            SetupCooldownOverlay();

            // Hide ready indicator initially
            if (readyIndicator != null)
            {
                readyIndicator.SetActive(false);
            }

            // Hide timer text if disabled
            if (cooldownTimerText != null && !showCountdownTimer)
            {
                cooldownTimerText.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (specialMoves != null)
            {
                // Subscribe to special move events
                specialMoves.OnSpecialMoveUsed += OnSpecialMoveUsed;
                specialMoves.OnSpecialMoveReady += OnSpecialMoveReady;

                // Get special move data
                _specialMoveData = GetSpecialMoveData();

                // Initialize display
                UpdateCooldownDisplay();
            }

            if (characterStamina != null)
            {
                // Subscribe to stamina events for dual-gating
                characterStamina.OnStaminaChanged += OnStaminaChanged;
            }
        }

        private void Update()
        {
            // Update cooldown display every frame
            if (specialMoves != null)
            {
                UpdateCooldownDisplay();
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (specialMoves != null)
            {
                specialMoves.OnSpecialMoveUsed -= OnSpecialMoveUsed;
                specialMoves.OnSpecialMoveReady -= OnSpecialMoveReady;
            }

            if (characterStamina != null)
            {
                characterStamina.OnStaminaChanged -= OnStaminaChanged;
            }

            // Stop all coroutines
            StopAllCoroutines();
        }

        private void ValidateReferences()
        {
            if (specialMoves == null)
            {
                Debug.LogError("SpecialMoveCooldownUI: Special moves reference is missing!", this);
            }

            if (cooldownFillOverlay == null)
            {
                Debug.LogWarning("SpecialMoveCooldownUI: Cooldown fill overlay is missing!", this);
            }

            if (iconImage == null)
            {
                Debug.LogWarning("SpecialMoveCooldownUI: Icon image is missing!", this);
            }
        }

        private void SetupCooldownOverlay()
        {
            if (cooldownFillOverlay == null)
            {
                return;
            }

            // Configure fill overlay
            if (useRadialFill)
            {
                cooldownFillOverlay.type = Image.Type.Filled;
                cooldownFillOverlay.fillMethod = Image.FillMethod.Radial360;
                cooldownFillOverlay.fillOrigin = (int)Image.Origin360.Top;
                cooldownFillOverlay.fillClockwise = false;
            }
            else
            {
                cooldownFillOverlay.type = Image.Type.Filled;
                cooldownFillOverlay.fillMethod = Image.FillMethod.Vertical;
                cooldownFillOverlay.fillOrigin = (int)Image.OriginVertical.Bottom;
            }

            cooldownFillOverlay.color = overlayColor;
            cooldownFillOverlay.fillAmount = 0f;
        }

        /// <summary>
        /// Gets the special move data from the component (via reflection or public property).
        /// </summary>
        private SpecialMoveData GetSpecialMoveData()
        {
            if (specialMoves == null)
            {
                return null;
            }

            // Try to get via reflection
            var field = typeof(CharacterSpecialMoves).GetField("specialMoveData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(specialMoves) as SpecialMoveData;
        }

        /// <summary>
        /// Called when special move is used.
        /// </summary>
        private void OnSpecialMoveUsed(SpecialMoveData moveData)
        {
            _isReady = false;
            StopPulseAnimation();

            // Show cooldown overlay
            if (cooldownFillOverlay != null)
            {
                cooldownFillOverlay.fillAmount = 1f;
            }

            // Hide ready indicator
            if (readyIndicator != null)
            {
                readyIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// Called when special move is ready.
        /// </summary>
        private void OnSpecialMoveReady()
        {
            _isReady = true;
            UpdateReadyState();
        }

        /// <summary>
        /// Called when character stamina changes.
        /// </summary>
        private void OnStaminaChanged(float currentStamina, float maxStamina)
        {
            UpdateStaminaGating();
        }

        /// <summary>
        /// Updates the cooldown display.
        /// </summary>
        private void UpdateCooldownDisplay()
        {
            if (specialMoves == null)
            {
                return;
            }

            // Update cooldown overlay fill
            if (cooldownFillOverlay != null)
            {
                // CooldownProgress goes from 0 (just used) to 1 (ready)
                // We want overlay to go from 1 (full coverage) to 0 (no coverage)
                float overlayFill = 1f - specialMoves.CooldownProgress;
                cooldownFillOverlay.fillAmount = Mathf.Max(0f, overlayFill);
            }

            // Update timer text
            if (cooldownTimerText != null && showCountdownTimer)
            {
                if (specialMoves.IsOnCooldown)
                {
                    cooldownTimerText.gameObject.SetActive(true);
                    float timeRemaining = specialMoves.CooldownTimeRemaining;
                    cooldownTimerText.text = FormatTime(timeRemaining);
                    cooldownTimerText.color = timerTextColor;
                }
                else
                {
                    cooldownTimerText.gameObject.SetActive(false);
                }
            }

            // Update ready state
            _isReady = !specialMoves.IsOnCooldown;
            UpdateReadyState();
        }

        /// <summary>
        /// Formats time remaining in seconds to display string.
        /// </summary>
        private string FormatTime(float seconds)
        {
            if (seconds >= 60f)
            {
                int minutes = Mathf.FloorToInt(seconds / 60f);
                int secs = Mathf.FloorToInt(seconds % 60f);
                return $"{minutes}:{secs:00}";
            }
            else if (seconds >= 10f)
            {
                return $"{seconds:F0}s";
            }
            else
            {
                return $"{seconds:F1}s";
            }
        }

        /// <summary>
        /// Updates stamina gating check (dual-gating with cooldown).
        /// </summary>
        private void UpdateStaminaGating()
        {
            if (characterStamina == null || _specialMoveData == null)
            {
                _hasEnoughStamina = true; // Default to true if no stamina component
                UpdateReadyState();
                return;
            }

            // Check if character has enough stamina for special move
            _hasEnoughStamina = characterStamina.CurrentStamina >= _specialMoveData.StaminaCost;
            UpdateReadyState();
        }

        /// <summary>
        /// Updates the ready state visuals (icon color, ready indicator, pulse).
        /// </summary>
        private void UpdateReadyState()
        {
            bool fullyReady = _isReady && _hasEnoughStamina;

            // Update icon color
            if (iconImage != null)
            {
                if (fullyReady)
                {
                    iconImage.color = readyColor;
                }
                else if (!_isReady)
                {
                    iconImage.color = cooldownColor;
                }
                else if (!_hasEnoughStamina)
                {
                    iconImage.color = insufficientStaminaColor;
                }
            }

            // Update ready indicator
            if (readyIndicator != null)
            {
                readyIndicator.SetActive(fullyReady);
            }

            // Update pulse animation
            if (fullyReady && pulseWhenReady)
            {
                StartPulseAnimation();
            }
            else
            {
                StopPulseAnimation();
            }
        }

        /// <summary>
        /// Starts pulse animation.
        /// </summary>
        private void StartPulseAnimation()
        {
            if (_pulseCoroutine != null)
            {
                return; // Already pulsing
            }

            _pulseCoroutine = StartCoroutine(AnimatePulse());
        }

        /// <summary>
        /// Stops pulse animation.
        /// </summary>
        private void StopPulseAnimation()
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;

                // Reset scale
                if (iconImage != null)
                {
                    iconImage.transform.localScale = Vector3.one;
                }
                if (readyIndicator != null)
                {
                    readyIndicator.transform.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// Animates pulsing effect when ready.
        /// </summary>
        private IEnumerator AnimatePulse()
        {
            while (true)
            {
                float time = 0f;
                while (time < 1f / pulseSpeed)
                {
                    time += Time.deltaTime;
                    float t = Mathf.PingPong(time * pulseSpeed * 2f, 1f);
                    float scale = Mathf.Lerp(1f, pulseScale, t);

                    if (iconImage != null)
                    {
                        iconImage.transform.localScale = Vector3.one * scale;
                    }
                    if (readyIndicator != null)
                    {
                        readyIndicator.transform.localScale = Vector3.one * scale;
                    }

                    yield return null;
                }
            }
        }

        /// <summary>
        /// Public method to set special moves reference at runtime.
        /// </summary>
        public void SetSpecialMoves(CharacterSpecialMoves moves, CharacterStamina stamina = null)
        {
            // Unsubscribe from old references
            if (specialMoves != null)
            {
                specialMoves.OnSpecialMoveUsed -= OnSpecialMoveUsed;
                specialMoves.OnSpecialMoveReady -= OnSpecialMoveReady;
            }

            if (characterStamina != null)
            {
                characterStamina.OnStaminaChanged -= OnStaminaChanged;
            }

            // Set new references
            specialMoves = moves;
            characterStamina = stamina;

            // Subscribe to new references
            if (specialMoves != null)
            {
                specialMoves.OnSpecialMoveUsed += OnSpecialMoveUsed;
                specialMoves.OnSpecialMoveReady += OnSpecialMoveReady;

                _specialMoveData = GetSpecialMoveData();
            }

            if (characterStamina != null)
            {
                characterStamina.OnStaminaChanged += OnStaminaChanged;
            }

            // Update display
            UpdateCooldownDisplay();
            UpdateStaminaGating();
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate colors
            if (readyColor.a < 0.5f || cooldownColor.a < 0.5f)
            {
                Debug.LogWarning("SpecialMoveCooldownUI: Icon colors have low alpha, may not be visible!", this);
            }

            // Validate pulse settings
            if (pulseWhenReady && (pulseScale < 1f || pulseScale > 2f))
            {
                Debug.LogWarning("SpecialMoveCooldownUI: Pulse scale should be between 1 and 2!", this);
            }
        }
        #endif
    }
}
