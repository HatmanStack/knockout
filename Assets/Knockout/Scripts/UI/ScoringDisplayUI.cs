using System.Collections;
using UnityEngine;
using TMPro;
using Knockout.Characters.Components;
using Knockout.Systems;

namespace Knockout.UI
{
    /// <summary>
    /// Displays current score and scoring breakdown (contextual at round end).
    /// Shows real-time scores during round and detailed breakdown on judge decision.
    /// </summary>
    public class ScoringDisplayUI : MonoBehaviour
    {
        [Header("Real-Time Score References")]
        [SerializeField] [Tooltip("Text for player real-time score")]
        private TextMeshProUGUI playerScoreText;

        [SerializeField] [Tooltip("Text for opponent real-time score")]
        private TextMeshProUGUI opponentScoreText;

        [SerializeField] [Tooltip("Root for real-time scores (can be hidden)")]
        private GameObject realTimeScoreRoot;

        [Header("Breakdown Panel References")]
        [SerializeField] [Tooltip("Root for breakdown panel")]
        private GameObject breakdownPanelRoot;

        [SerializeField] [Tooltip("Text for breakdown title")]
        private TextMeshProUGUI breakdownTitleText;

        [SerializeField] [Tooltip("Text for player stats breakdown")]
        private TextMeshProUGUI playerStatsText;

        [SerializeField] [Tooltip("Text for opponent stats breakdown")]
        private TextMeshProUGUI opponentStatsText;

        [SerializeField] [Tooltip("Text for player total score")]
        private TextMeshProUGUI playerTotalScoreText;

        [SerializeField] [Tooltip("Text for opponent total score")]
        private TextMeshProUGUI opponentTotalScoreText;

        [SerializeField] [Tooltip("Text for winner announcement")]
        private TextMeshProUGUI winnerAnnouncementText;

        [Header("Character References")]
        [SerializeField] [Tooltip("Player scoring component")]
        private CharacterScoring playerScoring;

        [SerializeField] [Tooltip("Opponent scoring component")]
        private CharacterScoring opponentScoring;

        [SerializeField] [Tooltip("Round manager for judge decision events")]
        private RoundManager roundManager;

        [Header("Display Settings")]
        [SerializeField] [Tooltip("Show real-time scores during round")]
        private bool showRealTimeScores = true;

        [SerializeField] [Tooltip("Duration to display breakdown panel (seconds)")]
        [Range(2f, 10f)]
        private float breakdownDisplayDuration = 5f;

        [Header("Color Settings")]
        [SerializeField] [Tooltip("Color for player text")]
        private Color playerColor = new Color(0.3f, 0.6f, 1f); // Blue

        [SerializeField] [Tooltip("Color for opponent text")]
        private Color opponentColor = new Color(1f, 0.3f, 0.3f); // Red

        [SerializeField] [Tooltip("Color for winner text")]
        private Color winnerColor = Color.yellow;

        [SerializeField] [Tooltip("Color for neutral text")]
        private Color neutralColor = Color.white;

        // Internal state
        private Coroutine _breakdownCoroutine;
        private CanvasGroup _breakdownCanvasGroup;

        private void Awake()
        {
            ValidateReferences();
            SetupCanvasGroup();

            // Hide breakdown panel initially
            if (breakdownPanelRoot != null)
            {
                breakdownPanelRoot.SetActive(false);
            }

            // Show/hide real-time scores based on setting
            if (realTimeScoreRoot != null)
            {
                realTimeScoreRoot.SetActive(showRealTimeScores);
            }
        }

        private void Start()
        {
            if (playerScoring != null)
            {
                playerScoring.OnScoreChanged += OnPlayerScoreChanged;
            }

            if (opponentScoring != null)
            {
                opponentScoring.OnScoreChanged += OnOpponentScoreChanged;
            }

            if (roundManager != null)
            {
                roundManager.OnJudgeDecision += OnJudgeDecision;
            }

            // Initialize real-time scores
            UpdateRealTimeScores();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (playerScoring != null)
            {
                playerScoring.OnScoreChanged -= OnPlayerScoreChanged;
            }

            if (opponentScoring != null)
            {
                opponentScoring.OnScoreChanged -= OnOpponentScoreChanged;
            }

            if (roundManager != null)
            {
                roundManager.OnJudgeDecision -= OnJudgeDecision;
            }

            StopAllCoroutines();
        }

        private void ValidateReferences()
        {
            if (playerScoring == null)
            {
                Debug.LogWarning("ScoringDisplayUI: Player scoring reference is missing!", this);
            }

            if (opponentScoring == null)
            {
                Debug.LogWarning("ScoringDisplayUI: Opponent scoring reference is missing!", this);
            }

            if (roundManager == null)
            {
                Debug.LogWarning("ScoringDisplayUI: Round manager reference is missing!", this);
            }

            if (breakdownPanelRoot == null)
            {
                Debug.LogError("ScoringDisplayUI: Breakdown panel root is missing!", this);
            }
        }

        private void SetupCanvasGroup()
        {
            if (breakdownPanelRoot != null)
            {
                _breakdownCanvasGroup = breakdownPanelRoot.GetComponent<CanvasGroup>();
                if (_breakdownCanvasGroup == null)
                {
                    _breakdownCanvasGroup = breakdownPanelRoot.AddComponent<CanvasGroup>();
                }
                _breakdownCanvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Called when player score changes.
        /// </summary>
        private void OnPlayerScoreChanged(float newScore)
        {
            UpdateRealTimeScores();
        }

        /// <summary>
        /// Called when opponent score changes.
        /// </summary>
        private void OnOpponentScoreChanged(float newScore)
        {
            UpdateRealTimeScores();
        }

        /// <summary>
        /// Updates real-time score display.
        /// </summary>
        private void UpdateRealTimeScores()
        {
            if (!showRealTimeScores)
            {
                return;
            }

            if (playerScoreText != null && playerScoring != null)
            {
                float score = playerScoring.CalculateTotalScore();
                playerScoreText.text = $"Score: {score:F0}";
                playerScoreText.color = playerColor;
            }

            if (opponentScoreText != null && opponentScoring != null)
            {
                float score = opponentScoring.CalculateTotalScore();
                opponentScoreText.text = $"Score: {score:F0}";
                opponentScoreText.color = opponentColor;
            }
        }

        /// <summary>
        /// Called when judge decision is made.
        /// </summary>
        private void OnJudgeDecision(bool playerWon, float playerScore, float opponentScore)
        {
            DisplayBreakdownPanel(playerWon, playerScore, opponentScore);
        }

        /// <summary>
        /// Displays the breakdown panel with stats.
        /// </summary>
        private void DisplayBreakdownPanel(bool playerWon, float playerScore, float opponentScore)
        {
            if (breakdownPanelRoot == null)
            {
                return;
            }

            // Stop any existing breakdown
            if (_breakdownCoroutine != null)
            {
                StopCoroutine(_breakdownCoroutine);
            }

            // Start breakdown display
            _breakdownCoroutine = StartCoroutine(ShowBreakdownRoutine(playerWon, playerScore, opponentScore));
        }

        /// <summary>
        /// Coroutine to show breakdown panel.
        /// </summary>
        private IEnumerator ShowBreakdownRoutine(bool playerWon, float playerScore, float opponentScore)
        {
            // Activate panel
            breakdownPanelRoot.SetActive(true);

            // Set title
            if (breakdownTitleText != null)
            {
                breakdownTitleText.text = "JUDGE DECISION";
                breakdownTitleText.color = neutralColor;
            }

            // Set stats
            UpdateBreakdownStats();

            // Set total scores
            if (playerTotalScoreText != null)
            {
                playerTotalScoreText.text = $"Total: {playerScore:F1}";
                playerTotalScoreText.color = playerWon ? winnerColor : playerColor;
            }

            if (opponentTotalScoreText != null)
            {
                opponentTotalScoreText.text = $"Total: {opponentScore:F1}";
                opponentTotalScoreText.color = !playerWon ? winnerColor : opponentColor;
            }

            // Set winner announcement
            if (winnerAnnouncementText != null)
            {
                winnerAnnouncementText.text = playerWon ? "PLAYER WINS!" : "OPPONENT WINS!";
                winnerAnnouncementText.color = winnerColor;
            }

            // Fade in
            if (_breakdownCanvasGroup != null)
            {
                float fadeInTime = 0.3f;
                float elapsed = 0f;
                while (elapsed < fadeInTime)
                {
                    elapsed += Time.deltaTime;
                    _breakdownCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
                    yield return null;
                }
                _breakdownCanvasGroup.alpha = 1f;
            }

            // Display for duration
            yield return new WaitForSeconds(breakdownDisplayDuration);

            // Fade out
            if (_breakdownCanvasGroup != null)
            {
                float fadeOutTime = 0.5f;
                float elapsed = 0f;
                while (elapsed < fadeOutTime)
                {
                    elapsed += Time.deltaTime;
                    _breakdownCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
                    yield return null;
                }
                _breakdownCanvasGroup.alpha = 0f;
            }

            // Hide panel
            breakdownPanelRoot.SetActive(false);
        }

        /// <summary>
        /// Updates the breakdown stats display.
        /// </summary>
        private void UpdateBreakdownStats()
        {
            if (playerStatsText != null && playerScoring != null)
            {
                playerStatsText.text = FormatStatsBreakdown(playerScoring);
                playerStatsText.color = playerColor;
            }

            if (opponentStatsText != null && opponentScoring != null)
            {
                opponentStatsText.text = FormatStatsBreakdown(opponentScoring);
                opponentStatsText.color = opponentColor;
            }
        }

        /// <summary>
        /// Formats stats breakdown string.
        /// </summary>
        private string FormatStatsBreakdown(CharacterScoring scoring)
        {
            // Get stats via reflection (properties are likely public)
            int cleanHits = GetScoringProperty<int>(scoring, "CleanHitsLanded");
            int combos = GetScoringProperty<int>(scoring, "CombosCompleted");
            int parries = GetScoringProperty<int>(scoring, "ParriesSuccessful");
            int specialMoves = GetScoringProperty<int>(scoring, "SpecialMovesLanded");
            int knockdowns = GetScoringProperty<int>(scoring, "KnockdownsInflicted");

            return $"Clean Hits: {cleanHits}\n" +
                   $"Combos: {combos}\n" +
                   $"Parries: {parries}\n" +
                   $"Special Moves: {specialMoves}\n" +
                   $"Knockdowns: {knockdowns}";
        }

        /// <summary>
        /// Gets a scoring property via reflection.
        /// </summary>
        private T GetScoringProperty<T>(CharacterScoring scoring, string propertyName)
        {
            var property = typeof(CharacterScoring).GetProperty(propertyName,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (property != null)
            {
                return (T)property.GetValue(scoring);
            }

            return default(T);
        }

        /// <summary>
        /// Public method to set scoring references at runtime.
        /// </summary>
        public void SetScoringReferences(CharacterScoring player, CharacterScoring opponent, RoundManager manager)
        {
            // Unsubscribe from old references
            if (playerScoring != null)
            {
                playerScoring.OnScoreChanged -= OnPlayerScoreChanged;
            }

            if (opponentScoring != null)
            {
                opponentScoring.OnScoreChanged -= OnOpponentScoreChanged;
            }

            if (roundManager != null)
            {
                roundManager.OnJudgeDecision -= OnJudgeDecision;
            }

            // Set new references
            playerScoring = player;
            opponentScoring = opponent;
            roundManager = manager;

            // Subscribe to new references
            if (playerScoring != null)
            {
                playerScoring.OnScoreChanged += OnPlayerScoreChanged;
            }

            if (opponentScoring != null)
            {
                opponentScoring.OnScoreChanged += OnOpponentScoreChanged;
            }

            if (roundManager != null)
            {
                roundManager.OnJudgeDecision += OnJudgeDecision;
            }

            // Update display
            UpdateRealTimeScores();
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate colors
            if (playerColor.a < 0.5f || opponentColor.a < 0.5f || winnerColor.a < 0.5f)
            {
                Debug.LogWarning("ScoringDisplayUI: Colors have low alpha, may not be visible!", this);
            }

            // Validate display duration
            if (breakdownDisplayDuration < 2f)
            {
                Debug.LogWarning("ScoringDisplayUI: Breakdown display duration is very short!", this);
            }
        }
        #endif
    }
}
