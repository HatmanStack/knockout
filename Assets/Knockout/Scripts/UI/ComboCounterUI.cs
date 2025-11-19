using System.Collections;
using UnityEngine;
using TMPro;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.UI
{
    /// <summary>
    /// Displays combo counter UI that appears during active combos.
    /// Shows hit count and special effects for sequence completion.
    /// </summary>
    public class ComboCounterUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] [Tooltip("Text for combo hit count")]
        private TextMeshProUGUI comboCountText;

        [SerializeField] [Tooltip("Text for combo label (e.g., 'HIT COMBO!')")]
        private TextMeshProUGUI comboLabelText;

        [SerializeField] [Tooltip("Text for sequence name when completed")]
        private TextMeshProUGUI sequenceNameText;

        [SerializeField] [Tooltip("Root GameObject for the combo counter (for show/hide)")]
        private GameObject comboCounterRoot;

        [Header("Character Reference")]
        [SerializeField] [Tooltip("The character's combo tracker to monitor")]
        private CharacterComboTracker comboTracker;

        [Header("Display Settings")]
        [SerializeField] [Tooltip("Minimum hits to show combo counter")]
        [Range(2, 5)]
        private int minimumHitsToShow = 2;

        [SerializeField] [Tooltip("Duration to display sequence name (seconds)")]
        [Range(0.5f, 3f)]
        private float sequenceNameDuration = 1.5f;

        [SerializeField] [Tooltip("Duration to fade out after combo ends (seconds)")]
        [Range(0.2f, 2f)]
        private float fadeOutDuration = 0.8f;

        [Header("Color Settings")]
        [SerializeField] [Tooltip("Color for combo count text")]
        private Color comboCountColor = new Color(1f, 0.85f, 0f); // Gold

        [SerializeField] [Tooltip("Color for combo label text")]
        private Color comboLabelColor = Color.white;

        [SerializeField] [Tooltip("Color for sequence name text")]
        private Color sequenceNameColor = new Color(1f, 0.3f, 0f); // Orange

        [Header("Animation Settings")]
        [SerializeField] [Tooltip("Scale increase on each hit")]
        [Range(1f, 2f)]
        private float hitScalePunch = 1.3f;

        [SerializeField] [Tooltip("Speed of scale punch animation")]
        [Range(0.05f, 0.3f)]
        private float scalePunchSpeed = 0.15f;

        [SerializeField] [Tooltip("Font size increase per combo hit")]
        [Range(0f, 5f)]
        private float fontSizeIncreasePerHit = 2f;

        [SerializeField] [Tooltip("Maximum font size multiplier")]
        [Range(1f, 3f)]
        private float maxFontSizeMultiplier = 2f;

        [Header("Sequence Completion Settings")]
        [SerializeField] [Tooltip("Scale for sequence completion effect")]
        [Range(1f, 3f)]
        private float sequenceCompletionScale = 1.5f;

        [SerializeField] [Tooltip("Duration of sequence completion animation")]
        [Range(0.1f, 1f)]
        private float sequenceAnimationDuration = 0.4f;

        // Internal state
        private int _currentComboCount;
        private bool _isVisible;
        private float _baseFontSize;
        private Coroutine _scalePunchCoroutine;
        private Coroutine _fadeOutCoroutine;
        private Coroutine _sequenceAnimationCoroutine;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            ValidateReferences();
            SetupCanvasGroup();

            // Store base font size
            if (comboCountText != null)
            {
                _baseFontSize = comboCountText.fontSize;
            }

            // Initialize hidden
            if (comboCounterRoot != null)
            {
                comboCounterRoot.SetActive(false);
                _isVisible = false;
            }

            // Hide sequence name initially
            if (sequenceNameText != null)
            {
                sequenceNameText.gameObject.SetActive(false);
            }

            // Initialize colors
            if (comboCountText != null)
            {
                comboCountText.color = comboCountColor;
            }
            if (comboLabelText != null)
            {
                comboLabelText.color = comboLabelColor;
            }
            if (sequenceNameText != null)
            {
                sequenceNameText.color = sequenceNameColor;
            }
        }

        private void Start()
        {
            if (comboTracker != null)
            {
                // Subscribe to combo events
                comboTracker.OnComboHitLanded += OnComboHitLanded;
                comboTracker.OnComboSequenceCompleted += OnComboSequenceCompleted;
                comboTracker.OnComboEnded += OnComboEnded;
                comboTracker.OnComboBroken += OnComboBroken;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (comboTracker != null)
            {
                comboTracker.OnComboHitLanded -= OnComboHitLanded;
                comboTracker.OnComboSequenceCompleted -= OnComboSequenceCompleted;
                comboTracker.OnComboEnded -= OnComboEnded;
                comboTracker.OnComboBroken -= OnComboBroken;
            }

            // Stop all coroutines
            StopAllCoroutines();
        }

        private void ValidateReferences()
        {
            if (comboCountText == null)
            {
                Debug.LogError("ComboCounterUI: Combo count text is not assigned!", this);
            }

            if (comboTracker == null)
            {
                Debug.LogWarning("ComboCounterUI: Combo tracker reference is missing!", this);
            }

            if (comboCounterRoot == null)
            {
                Debug.LogWarning("ComboCounterUI: Combo counter root is missing. Using this GameObject instead.", this);
                comboCounterRoot = gameObject;
            }
        }

        private void SetupCanvasGroup()
        {
            // Add CanvasGroup for fade effects if not present
            if (comboCounterRoot != null)
            {
                _canvasGroup = comboCounterRoot.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = comboCounterRoot.AddComponent<CanvasGroup>();
                }
                _canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Called when a combo hit lands.
        /// </summary>
        private void OnComboHitLanded(int hitCount, float totalDamage)
        {
            _currentComboCount = hitCount;

            // Show counter if meets minimum
            if (hitCount >= minimumHitsToShow && !_isVisible)
            {
                ShowComboCounter();
            }

            // Update display
            if (_isVisible)
            {
                UpdateComboDisplay(hitCount);
                PlayHitAnimation();
            }
        }

        /// <summary>
        /// Called when a predefined combo sequence is completed.
        /// </summary>
        private void OnComboSequenceCompleted(ComboSequenceData sequenceData)
        {
            if (sequenceData == null || sequenceNameText == null)
            {
                return;
            }

            // Display sequence name with special animation
            DisplaySequenceName(sequenceData.SequenceName);
        }

        /// <summary>
        /// Called when combo ends naturally (timer expired).
        /// </summary>
        private void OnComboEnded(int finalCount, float totalDamage)
        {
            if (_isVisible)
            {
                HideComboCounter();
            }
        }

        /// <summary>
        /// Called when combo is broken (blocked, hit taken, etc.).
        /// </summary>
        private void OnComboBroken(int finalCount)
        {
            if (_isVisible)
            {
                HideComboCounter();
            }
        }

        /// <summary>
        /// Shows the combo counter.
        /// </summary>
        private void ShowComboCounter()
        {
            if (comboCounterRoot == null)
            {
                return;
            }

            // Cancel any ongoing fade out
            if (_fadeOutCoroutine != null)
            {
                StopCoroutine(_fadeOutCoroutine);
                _fadeOutCoroutine = null;
            }

            comboCounterRoot.SetActive(true);
            _isVisible = true;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Hides the combo counter with fade out.
        /// </summary>
        private void HideComboCounter()
        {
            if (comboCounterRoot == null || !_isVisible)
            {
                return;
            }

            // Start fade out
            if (_fadeOutCoroutine != null)
            {
                StopCoroutine(_fadeOutCoroutine);
            }
            _fadeOutCoroutine = StartCoroutine(FadeOutAndHide());
        }

        /// <summary>
        /// Updates the combo count display.
        /// </summary>
        private void UpdateComboDisplay(int hitCount)
        {
            if (comboCountText == null)
            {
                return;
            }

            // Update count text
            comboCountText.text = hitCount.ToString();

            // Update label text
            if (comboLabelText != null)
            {
                comboLabelText.text = hitCount == 2 ? "HIT COMBO!" : "HIT COMBO!!";
            }

            // Increase font size with combo count (capped)
            float fontSizeMultiplier = 1f + Mathf.Min((hitCount - minimumHitsToShow) * (fontSizeIncreasePerHit / 100f), maxFontSizeMultiplier - 1f);
            comboCountText.fontSize = _baseFontSize * fontSizeMultiplier;
        }

        /// <summary>
        /// Plays hit animation (scale punch).
        /// </summary>
        private void PlayHitAnimation()
        {
            if (comboCounterRoot == null)
            {
                return;
            }

            // Stop existing animation
            if (_scalePunchCoroutine != null)
            {
                StopCoroutine(_scalePunchCoroutine);
            }

            _scalePunchCoroutine = StartCoroutine(AnimateScalePunch());
        }

        /// <summary>
        /// Animates scale punch effect.
        /// </summary>
        private IEnumerator AnimateScalePunch()
        {
            Vector3 originalScale = comboCounterRoot.transform.localScale;
            Vector3 punchScale = originalScale * hitScalePunch;

            // Scale up
            float elapsed = 0f;
            while (elapsed < scalePunchSpeed / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (scalePunchSpeed / 2f);
                comboCounterRoot.transform.localScale = Vector3.Lerp(originalScale, punchScale, t);
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < scalePunchSpeed / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (scalePunchSpeed / 2f);
                comboCounterRoot.transform.localScale = Vector3.Lerp(punchScale, originalScale, t);
                yield return null;
            }

            // Ensure exact original scale
            comboCounterRoot.transform.localScale = originalScale;
        }

        /// <summary>
        /// Displays sequence name with special animation.
        /// </summary>
        private void DisplaySequenceName(string sequenceName)
        {
            if (sequenceNameText == null)
            {
                return;
            }

            // Stop existing sequence animation
            if (_sequenceAnimationCoroutine != null)
            {
                StopCoroutine(_sequenceAnimationCoroutine);
            }

            _sequenceAnimationCoroutine = StartCoroutine(AnimateSequenceCompletion(sequenceName));
        }

        /// <summary>
        /// Animates sequence completion effect.
        /// </summary>
        private IEnumerator AnimateSequenceCompletion(string sequenceName)
        {
            if (sequenceNameText == null)
            {
                yield break;
            }

            // Set sequence name
            sequenceNameText.text = sequenceName.ToUpper() + "!";
            sequenceNameText.gameObject.SetActive(true);

            Vector3 originalScale = sequenceNameText.transform.localScale;
            Vector3 largeScale = originalScale * sequenceCompletionScale;

            // Scale up quickly
            float elapsed = 0f;
            while (elapsed < sequenceAnimationDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (sequenceAnimationDuration / 2f);
                sequenceNameText.transform.localScale = Vector3.Lerp(Vector3.zero, largeScale, t);
                yield return null;
            }

            // Hold at large scale
            yield return new WaitForSeconds(sequenceNameDuration);

            // Scale down
            elapsed = 0f;
            while (elapsed < sequenceAnimationDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (sequenceAnimationDuration / 2f);
                sequenceNameText.transform.localScale = Vector3.Lerp(largeScale, Vector3.zero, t);
                yield return null;
            }

            // Hide sequence text
            sequenceNameText.gameObject.SetActive(false);
            sequenceNameText.transform.localScale = originalScale;
        }

        /// <summary>
        /// Fades out and hides the combo counter.
        /// </summary>
        private IEnumerator FadeOutAndHide()
        {
            if (_canvasGroup == null)
            {
                comboCounterRoot.SetActive(false);
                _isVisible = false;
                yield break;
            }

            // Fade out
            float elapsed = 0f;
            float startAlpha = _canvasGroup.alpha;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            _canvasGroup.alpha = 0f;

            // Hide
            comboCounterRoot.SetActive(false);
            _isVisible = false;

            // Reset alpha for next time
            _canvasGroup.alpha = 1f;

            // Reset font size
            if (comboCountText != null)
            {
                comboCountText.fontSize = _baseFontSize;
            }
        }

        /// <summary>
        /// Public method to set combo tracker reference at runtime.
        /// </summary>
        public void SetComboTracker(CharacterComboTracker tracker)
        {
            // Unsubscribe from old reference
            if (comboTracker != null)
            {
                comboTracker.OnComboHitLanded -= OnComboHitLanded;
                comboTracker.OnComboSequenceCompleted -= OnComboSequenceCompleted;
                comboTracker.OnComboEnded -= OnComboEnded;
                comboTracker.OnComboBroken -= OnComboBroken;
            }

            // Set new reference
            comboTracker = tracker;

            // Subscribe to new reference
            if (comboTracker != null)
            {
                comboTracker.OnComboHitLanded += OnComboHitLanded;
                comboTracker.OnComboSequenceCompleted += OnComboSequenceCompleted;
                comboTracker.OnComboEnded += OnComboEnded;
                comboTracker.OnComboBroken += OnComboBroken;
            }
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate minimum hits
            if (minimumHitsToShow < 2)
            {
                Debug.LogWarning("ComboCounterUI: Minimum hits to show should be at least 2!", this);
            }

            // Validate colors are visible
            if (comboCountColor.a < 0.5f || comboLabelColor.a < 0.5f)
            {
                Debug.LogWarning("ComboCounterUI: Combo colors have low alpha, may not be visible!", this);
            }
        }
        #endif
    }
}
