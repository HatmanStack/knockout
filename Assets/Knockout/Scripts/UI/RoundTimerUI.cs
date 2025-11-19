using UnityEngine;
using TMPro;
using Knockout.Systems;

namespace Knockout.UI
{
    /// <summary>
    /// Displays round timer counting down with visual warnings.
    /// Subscribes to RoundManager timer events and provides color-coded feedback.
    /// </summary>
    public class RoundTimerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] [Tooltip("Text for timer display")]
        private TextMeshProUGUI timerText;

        [SerializeField] [Tooltip("Background for timer (optional, for pulse effect)")]
        private UnityEngine.UI.Image timerBackground;

        [Header("Round Manager Reference")]
        [SerializeField] [Tooltip("The round manager to subscribe to")]
        private RoundManager roundManager;

        [Header("Color Settings")]
        [SerializeField] [Tooltip("Color when time is normal (>30s)")]
        private Color normalColor = Color.white;

        [SerializeField] [Tooltip("Color when time is low (10-30s)")]
        private Color warningColor = Color.yellow;

        [SerializeField] [Tooltip("Color when time is critical (<10s)")]
        private Color criticalColor = Color.red;

        [Header("Warning Thresholds")]
        [SerializeField] [Tooltip("Time threshold for warning state (seconds)")]
        [Range(10f, 60f)]
        private float warningThreshold = 30f;

        [SerializeField] [Tooltip("Time threshold for critical state (seconds)")]
        [Range(5f, 30f)]
        private float criticalThreshold = 10f;

        [Header("Animation Settings")]
        [SerializeField] [Tooltip("Enable pulse animation when time low")]
        private bool pulseWhenLow = true;

        [SerializeField] [Range(0.5f, 3f)] [Tooltip("Pulse speed when critical")]
        private float pulseSpeed = 2f;

        [SerializeField] [Range(1f, 1.5f)] [Tooltip("Pulse scale multiplier")]
        private float pulseScale = 1.15f;

        // Internal state
        private float _currentTimeRemaining;
        private bool _isPulsing;

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            if (roundManager != null)
            {
                // Subscribe to round time events
                roundManager.OnRoundTimeChanged += OnRoundTimeChanged;

                // Initialize display
                UpdateTimerDisplay(roundManager.RoundTimeRemaining);
            }
        }

        private void Update()
        {
            // Handle pulse animation
            if (_isPulsing && timerText != null && pulseWhenLow)
            {
                float scale = 1f + (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) * (pulseScale - 1f));
                timerText.transform.localScale = Vector3.one * scale;

                if (timerBackground != null)
                {
                    timerBackground.transform.localScale = Vector3.one * scale;
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (roundManager != null)
            {
                roundManager.OnRoundTimeChanged -= OnRoundTimeChanged;
            }
        }

        private void ValidateReferences()
        {
            if (timerText == null)
            {
                Debug.LogError("RoundTimerUI: Timer text reference is missing!", this);
            }

            if (roundManager == null)
            {
                Debug.LogWarning("RoundTimerUI: Round manager reference is missing!", this);
            }
        }

        /// <summary>
        /// Called when round time changes.
        /// </summary>
        private void OnRoundTimeChanged(float timeRemaining)
        {
            _currentTimeRemaining = timeRemaining;
            UpdateTimerDisplay(timeRemaining);
        }

        /// <summary>
        /// Updates the timer display with color coding and warnings.
        /// </summary>
        private void UpdateTimerDisplay(float timeRemaining)
        {
            if (timerText == null)
            {
                return;
            }

            // Format time as MM:SS
            string formattedTime = FormatTime(timeRemaining);
            timerText.text = formattedTime;

            // Update color based on time remaining
            Color targetColor;
            bool shouldPulse = false;

            if (timeRemaining > warningThreshold)
            {
                // Normal time - white
                targetColor = normalColor;
                shouldPulse = false;
            }
            else if (timeRemaining > criticalThreshold)
            {
                // Warning time - yellow with pulse
                targetColor = warningColor;
                shouldPulse = true;
            }
            else
            {
                // Critical time - red with faster pulse
                targetColor = criticalColor;
                shouldPulse = true;
            }

            timerText.color = targetColor;

            // Update pulse state
            if (shouldPulse != _isPulsing)
            {
                _isPulsing = shouldPulse;

                if (!_isPulsing)
                {
                    // Reset scale when stopping pulse
                    timerText.transform.localScale = Vector3.one;
                    if (timerBackground != null)
                    {
                        timerBackground.transform.localScale = Vector3.one;
                    }
                }
            }
        }

        /// <summary>
        /// Formats time in seconds to MM:SS format.
        /// </summary>
        private string FormatTime(float timeInSeconds)
        {
            // Clamp to 0 minimum
            timeInSeconds = Mathf.Max(0f, timeInSeconds);

            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

            return $"{minutes}:{seconds:00}";
        }

        /// <summary>
        /// Public method to set round manager reference at runtime.
        /// </summary>
        public void SetRoundManager(RoundManager manager)
        {
            // Unsubscribe from old manager
            if (roundManager != null)
            {
                roundManager.OnRoundTimeChanged -= OnRoundTimeChanged;
            }

            // Set new manager
            roundManager = manager;

            // Subscribe to new manager
            if (roundManager != null)
            {
                roundManager.OnRoundTimeChanged += OnRoundTimeChanged;

                // Update display
                UpdateTimerDisplay(roundManager.RoundTimeRemaining);
            }
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate colors
            if (normalColor.a < 0.5f || warningColor.a < 0.5f || criticalColor.a < 0.5f)
            {
                Debug.LogWarning("RoundTimerUI: Timer colors have low alpha, may not be visible!", this);
            }

            // Validate thresholds
            if (criticalThreshold >= warningThreshold)
            {
                Debug.LogWarning("RoundTimerUI: Critical threshold should be less than warning threshold!", this);
            }
        }
        #endif
    }
}
