using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Knockout.Systems;

namespace Knockout.UI
{
    /// <summary>
    /// Displays round UI including countdown, round number, and win/loss messages.
    /// Subscribes to RoundManager events to update UI.
    /// </summary>
    public class RoundUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] [Tooltip("Text for countdown (3, 2, 1, Fight!)")]
        private TextMeshProUGUI countdownText;

        [SerializeField] [Tooltip("Text for round number display")]
        private TextMeshProUGUI roundNumberText;

        [SerializeField] [Tooltip("Text for round result (You Win, You Lose)")]
        private TextMeshProUGUI roundResultText;

        [SerializeField] [Tooltip("Text for match result")]
        private TextMeshProUGUI matchResultText;

        [Header("Round Manager Reference")]
        [SerializeField] [Tooltip("The round manager to subscribe to")]
        private RoundManager roundManager;

        [Header("Display Settings")]
        [SerializeField] [Tooltip("Color for countdown numbers")]
        private Color countdownColor = Color.yellow;

        [SerializeField] [Tooltip("Color for 'Fight!' text")]
        private Color fightColor = Color.green;

        [SerializeField] [Tooltip("Color for win messages")]
        private Color winColor = Color.green;

        [SerializeField] [Tooltip("Color for loss messages")]
        private Color lossColor = Color.red;

        private void Awake()
        {
            ValidateReferences();
            HideAllText();
        }

        private void Start()
        {
            if (roundManager != null)
            {
                SubscribeToRoundManager();
            }
        }

        private void OnDestroy()
        {
            if (roundManager != null)
            {
                UnsubscribeFromRoundManager();
            }
        }

        private void ValidateReferences()
        {
            if (roundManager == null)
            {
                Debug.LogWarning("RoundUI: Round manager reference is missing!");
            }

            if (countdownText == null)
            {
                Debug.LogWarning("RoundUI: Countdown text reference is missing!");
            }

            if (roundNumberText == null)
            {
                Debug.LogWarning("RoundUI: Round number text reference is missing!");
            }

            if (roundResultText == null)
            {
                Debug.LogWarning("RoundUI: Round result text reference is missing!");
            }

            if (matchResultText == null)
            {
                Debug.LogWarning("RoundUI: Match result text reference is missing!");
            }
        }

        private void HideAllText()
        {
            SetTextVisible(countdownText, false);
            SetTextVisible(roundNumberText, false);
            SetTextVisible(roundResultText, false);
            SetTextVisible(matchResultText, false);
        }

        private void SetTextVisible(TextMeshProUGUI textComponent, bool visible)
        {
            if (textComponent != null)
            {
                textComponent.gameObject.SetActive(visible);
            }
        }

        #region Event Subscription

        private void SubscribeToRoundManager()
        {
            roundManager.OnStateChanged += OnStateChanged;
            roundManager.OnCountdownTick += OnCountdownTick;
            roundManager.OnRoundStart += OnRoundStart;
            roundManager.OnRoundEnd += OnRoundEnd;
            roundManager.OnMatchEnd += OnMatchEnd;
        }

        private void UnsubscribeFromRoundManager()
        {
            roundManager.OnStateChanged -= OnStateChanged;
            roundManager.OnCountdownTick -= OnCountdownTick;
            roundManager.OnRoundStart -= OnRoundStart;
            roundManager.OnRoundEnd -= OnRoundEnd;
            roundManager.OnMatchEnd -= OnMatchEnd;
        }

        #endregion

        #region Event Handlers

        private void OnStateChanged(RoundManager.RoundState newState)
        {
            // Hide all UI when state changes
            HideAllText();

            // Show appropriate UI for new state
            switch (newState)
            {
                case RoundManager.RoundState.Countdown:
                    SetTextVisible(countdownText, true);
                    break;

                case RoundManager.RoundState.Fighting:
                    SetTextVisible(roundNumberText, true);
                    break;

                case RoundManager.RoundState.RoundOver:
                    SetTextVisible(roundResultText, true);
                    SetTextVisible(roundNumberText, true);
                    break;

                case RoundManager.RoundState.MatchOver:
                    SetTextVisible(matchResultText, true);
                    break;
            }
        }

        private void OnCountdownTick(int countdownValue)
        {
            if (countdownText == null)
            {
                return;
            }

            if (countdownValue > 0)
            {
                // Display number
                countdownText.text = countdownValue.ToString();
                countdownText.color = countdownColor;
            }
            else
            {
                // Display "Fight!"
                countdownText.text = "FIGHT!";
                countdownText.color = fightColor;
            }
        }

        private void OnRoundStart(int roundNumber)
        {
            if (roundNumberText == null)
            {
                return;
            }

            roundNumberText.text = $"Round {roundNumber}";
        }

        private void OnRoundEnd(bool playerWon, int playerWins, int aiWins)
        {
            if (roundResultText == null)
            {
                return;
            }

            if (playerWon)
            {
                roundResultText.text = "YOU WIN!";
                roundResultText.color = winColor;
            }
            else
            {
                roundResultText.text = "YOU LOSE!";
                roundResultText.color = lossColor;
            }
        }

        private void OnMatchEnd(bool playerWon)
        {
            if (matchResultText == null)
            {
                return;
            }

            if (playerWon)
            {
                matchResultText.text = "MATCH WON!\nVICTORY!";
                matchResultText.color = winColor;
            }
            else
            {
                matchResultText.text = "MATCH LOST!\nDEFEAT!";
                matchResultText.color = lossColor;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the round manager reference at runtime.
        /// </summary>
        public void SetRoundManager(RoundManager manager)
        {
            // Unsubscribe from old manager
            if (roundManager != null)
            {
                UnsubscribeFromRoundManager();
            }

            // Set new manager
            roundManager = manager;

            // Subscribe to new manager
            if (roundManager != null)
            {
                SubscribeToRoundManager();
            }
        }

        #endregion
    }
}
