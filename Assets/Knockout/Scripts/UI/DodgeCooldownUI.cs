using UnityEngine;
using UnityEngine.UI;
using Knockout.Characters.Components;

namespace Knockout.UI
{
    /// <summary>
    /// Displays dodge cooldown indicator (subtle, contextual).
    /// Shows only when dodge is on cooldown. Given very short cooldown (~0.2s), this is minimal.
    /// </summary>
    public class DodgeCooldownUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] [Tooltip("Icon or indicator image")]
        private Image indicatorImage;

        [SerializeField] [Tooltip("Cooldown fill overlay (radial or linear)")]
        private Image cooldownFillOverlay;

        [SerializeField] [Tooltip("Root GameObject for the indicator (for show/hide)")]
        private GameObject indicatorRoot;

        [Header("Character Reference")]
        [SerializeField] [Tooltip("The character's dodge component")]
        private CharacterDodge characterDodge;

        [Header("Display Settings")]
        [SerializeField] [Tooltip("Only show when on cooldown (recommended for short cooldowns)")]
        private bool onlyShowWhenOnCooldown = true;

        [SerializeField] [Tooltip("Use radial fill for cooldown overlay")]
        private bool useRadialFill = true;

        [Header("Color Settings")]
        [SerializeField] [Tooltip("Color for cooldown overlay")]
        private Color overlayColor = new Color(0f, 0f, 0f, 0.6f);

        [SerializeField] [Tooltip("Color for indicator when ready")]
        private Color readyColor = Color.white;

        [SerializeField] [Tooltip("Color for indicator when on cooldown")]
        private Color cooldownColor = new Color(0.7f, 0.7f, 0.7f);

        // Internal state
        private bool _isVisible;

        private void Awake()
        {
            ValidateReferences();
            SetupCooldownOverlay();

            // Hide if onlyShowWhenOnCooldown is enabled
            if (onlyShowWhenOnCooldown && indicatorRoot != null)
            {
                indicatorRoot.SetActive(false);
                _isVisible = false;
            }
        }

        private void Start()
        {
            if (characterDodge != null)
            {
                // Subscribe to dodge ready event
                characterDodge.OnDodgeReady += OnDodgeReady;
            }
        }

        private void Update()
        {
            // Update cooldown display every frame
            if (characterDodge != null)
            {
                UpdateCooldownDisplay();
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (characterDodge != null)
            {
                characterDodge.OnDodgeReady -= OnDodgeReady;
            }
        }

        private void ValidateReferences()
        {
            if (characterDodge == null)
            {
                Debug.LogWarning("DodgeCooldownUI: Character dodge reference is missing!", this);
            }

            if (indicatorRoot == null && onlyShowWhenOnCooldown)
            {
                Debug.LogWarning("DodgeCooldownUI: Indicator root is missing but onlyShowWhenOnCooldown is enabled!", this);
                indicatorRoot = gameObject;
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
        /// Called when dodge is ready (cooldown complete).
        /// </summary>
        private void OnDodgeReady()
        {
            if (onlyShowWhenOnCooldown)
            {
                HideIndicator();
            }
        }

        /// <summary>
        /// Updates the cooldown display.
        /// </summary>
        private void UpdateCooldownDisplay()
        {
            if (characterDodge == null)
            {
                return;
            }

            bool canDodge = characterDodge.CanDodge;
            bool isDodging = characterDodge.IsDodging;

            // Update visibility
            if (onlyShowWhenOnCooldown)
            {
                // Show only when on cooldown (not ready and not currently dodging)
                bool shouldShow = !canDodge && !isDodging;

                if (shouldShow && !_isVisible)
                {
                    ShowIndicator();
                }
                else if (!shouldShow && _isVisible)
                {
                    HideIndicator();
                }
            }

            // Update cooldown overlay fill
            if (cooldownFillOverlay != null)
            {
                // CooldownProgress goes from 0 (just dodged) to 1 (ready)
                // We want overlay to go from 1 (full coverage) to 0 (no coverage)
                float overlayFill = 1f - characterDodge.CooldownProgress;
                cooldownFillOverlay.fillAmount = Mathf.Max(0f, overlayFill);
            }

            // Update indicator color
            if (indicatorImage != null)
            {
                indicatorImage.color = canDodge ? readyColor : cooldownColor;
            }
        }

        /// <summary>
        /// Shows the indicator.
        /// </summary>
        private void ShowIndicator()
        {
            if (indicatorRoot != null)
            {
                indicatorRoot.SetActive(true);
                _isVisible = true;
            }
        }

        /// <summary>
        /// Hides the indicator.
        /// </summary>
        private void HideIndicator()
        {
            if (indicatorRoot != null)
            {
                indicatorRoot.SetActive(false);
                _isVisible = false;
            }
        }

        /// <summary>
        /// Public method to set dodge reference at runtime.
        /// </summary>
        public void SetCharacterDodge(CharacterDodge dodge)
        {
            // Unsubscribe from old reference
            if (characterDodge != null)
            {
                characterDodge.OnDodgeReady -= OnDodgeReady;
            }

            // Set new reference
            characterDodge = dodge;

            // Subscribe to new reference
            if (characterDodge != null)
            {
                characterDodge.OnDodgeReady += OnDodgeReady;
            }

            // Update display
            UpdateCooldownDisplay();
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate colors
            if (overlayColor.a < 0.3f)
            {
                Debug.LogWarning("DodgeCooldownUI: Overlay color has low alpha, may not be visible!", this);
            }
        }
        #endif
    }
}
