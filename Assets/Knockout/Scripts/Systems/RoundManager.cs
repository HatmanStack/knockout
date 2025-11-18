using System;
using System.Collections;
using UnityEngine;
using Knockout.Characters.Components;

namespace Knockout.Systems
{
    /// <summary>
    /// Manages round flow, win/loss conditions, and match progression.
    /// Implements state machine for: Countdown → Fighting → RoundOver → MatchOver
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        [Header("Character References")]
        [SerializeField] [Tooltip("Player character health component")]
        private CharacterHealth playerHealth;

        [SerializeField] [Tooltip("AI character health component")]
        private CharacterHealth aiHealth;

        [Header("Round Settings")]
        [SerializeField] [Range(1, 5)] [Tooltip("Number of rounds to win the match")]
        private int roundsToWin = 2;

        [SerializeField] [Range(1f, 5f)] [Tooltip("Countdown duration before round starts")]
        private float countdownDuration = 3f;

        [SerializeField] [Range(1f, 5f)] [Tooltip("Duration to display round over message")]
        private float roundOverDisplayDuration = 2f;

        // Round state
        public enum RoundState
        {
            Countdown,
            Fighting,
            RoundOver,
            MatchOver
        }

        private RoundState _currentState;
        private int _playerRoundWins;
        private int _aiRoundWins;
        private int _currentRoundNumber;
        private bool _isMatchOver;

        #region Events

        /// <summary>
        /// Fired when round state changes.
        /// Parameter: new state
        /// </summary>
        public event Action<RoundState> OnStateChanged;

        /// <summary>
        /// Fired during countdown with remaining seconds.
        /// Parameter: countdown value (3, 2, 1, 0 for "Fight!")
        /// </summary>
        public event Action<int> OnCountdownTick;

        /// <summary>
        /// Fired when a round ends.
        /// Parameters: (winner: true if player won, false if AI won, playerWins, aiWins)
        /// </summary>
        public event Action<bool, int, int> OnRoundEnd;

        /// <summary>
        /// Fired when the match ends.
        /// Parameter: true if player won match, false if AI won
        /// </summary>
        public event Action<bool> OnMatchEnd;

        /// <summary>
        /// Fired when a new round starts.
        /// Parameter: round number (1-based)
        /// </summary>
        public event Action<int> OnRoundStart;

        #endregion

        #region Public Properties

        public RoundState CurrentState => _currentState;
        public int PlayerRoundWins => _playerRoundWins;
        public int AIRoundWins => _aiRoundWins;
        public int CurrentRoundNumber => _currentRoundNumber;
        public bool IsMatchOver => _isMatchOver;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            InitializeMatch();
        }

        private void OnDestroy()
        {
            UnsubscribeFromHealthEvents();
        }

        #endregion

        #region Initialization

        private void ValidateReferences()
        {
            if (playerHealth == null)
            {
                Debug.LogError("RoundManager: Player health reference is missing!");
            }

            if (aiHealth == null)
            {
                Debug.LogError("RoundManager: AI health reference is missing!");
            }
        }

        private void InitializeMatch()
        {
            _playerRoundWins = 0;
            _aiRoundWins = 0;
            _currentRoundNumber = 0;
            _isMatchOver = false;

            SubscribeToHealthEvents();
            StartNewRound();
        }

        private void SubscribeToHealthEvents()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath += OnPlayerDeath;
            }

            if (aiHealth != null)
            {
                aiHealth.OnDeath += OnAIDeath;
            }
        }

        private void UnsubscribeFromHealthEvents()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath -= OnPlayerDeath;
            }

            if (aiHealth != null)
            {
                aiHealth.OnDeath -= OnAIDeath;
            }
        }

        #endregion

        #region State Management

        private void ChangeState(RoundState newState)
        {
            _currentState = newState;
            OnStateChanged?.Invoke(newState);

            // Execute state entry logic
            switch (newState)
            {
                case RoundState.Countdown:
                    StartCoroutine(CountdownRoutine());
                    break;

                case RoundState.Fighting:
                    EnableCharacters();
                    break;

                case RoundState.RoundOver:
                    DisableCharacters();
                    break;

                case RoundState.MatchOver:
                    DisableCharacters();
                    break;
            }
        }

        #endregion

        #region Round Flow

        private void StartNewRound()
        {
            _currentRoundNumber++;

            // Reset character health
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
            }

            if (aiHealth != null)
            {
                aiHealth.ResetHealth();
            }

            // Fire round start event
            OnRoundStart?.Invoke(_currentRoundNumber);

            // Start countdown
            ChangeState(RoundState.Countdown);
        }

        private IEnumerator CountdownRoutine()
        {
            int countdownValue = Mathf.CeilToInt(countdownDuration);

            while (countdownValue > 0)
            {
                OnCountdownTick?.Invoke(countdownValue);
                yield return new WaitForSeconds(1f);
                countdownValue--;
            }

            // "Fight!" signal
            OnCountdownTick?.Invoke(0);

            // Transition to fighting state
            ChangeState(RoundState.Fighting);
        }

        private void EndRound(bool playerWon)
        {
            if (_currentState != RoundState.Fighting)
            {
                return; // Already ended
            }

            // Update round wins
            if (playerWon)
            {
                _playerRoundWins++;
            }
            else
            {
                _aiRoundWins++;
            }

            // Fire round end event
            OnRoundEnd?.Invoke(playerWon, _playerRoundWins, _aiRoundWins);

            // Check if match is over
            if (_playerRoundWins >= roundsToWin || _aiRoundWins >= roundsToWin)
            {
                StartCoroutine(EndMatchRoutine(playerWon));
            }
            else
            {
                StartCoroutine(RoundOverRoutine());
            }
        }

        private IEnumerator RoundOverRoutine()
        {
            ChangeState(RoundState.RoundOver);

            // Display round over message
            yield return new WaitForSeconds(roundOverDisplayDuration);

            // Start next round
            StartNewRound();
        }

        private IEnumerator EndMatchRoutine(bool playerWon)
        {
            ChangeState(RoundState.MatchOver);
            _isMatchOver = true;

            // Fire match end event
            OnMatchEnd?.Invoke(playerWon);

            yield return null;
        }

        #endregion

        #region Death Handlers

        private void OnPlayerDeath()
        {
            if (_currentState == RoundState.Fighting)
            {
                EndRound(false); // AI won
            }
        }

        private void OnAIDeath()
        {
            if (_currentState == RoundState.Fighting)
            {
                EndRound(true); // Player won
            }
        }

        #endregion

        #region Character Control

        private void EnableCharacters()
        {
            // Enable player input
            if (playerHealth != null)
            {
                var playerInput = playerHealth.GetComponent<CharacterInput>();
                if (playerInput != null)
                {
                    playerInput.EnableInput();
                }
            }

            // Enable AI
            if (aiHealth != null)
            {
                var aiComponent = aiHealth.GetComponent<CharacterAI>();
                if (aiComponent != null)
                {
                    aiComponent.enabled = true;
                }
            }
        }

        private void DisableCharacters()
        {
            // Disable player input
            if (playerHealth != null)
            {
                var playerInput = playerHealth.GetComponent<CharacterInput>();
                if (playerInput != null)
                {
                    playerInput.DisableInput();
                }
            }

            // Disable AI
            if (aiHealth != null)
            {
                var aiComponent = aiHealth.GetComponent<CharacterAI>();
                if (aiComponent != null)
                {
                    aiComponent.enabled = false;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually starts a new match.
        /// Useful for restarting after match is over.
        /// </summary>
        public void RestartMatch()
        {
            StopAllCoroutines();
            InitializeMatch();
        }

        /// <summary>
        /// Sets character health references at runtime.
        /// </summary>
        public void SetCharacters(CharacterHealth player, CharacterHealth ai)
        {
            UnsubscribeFromHealthEvents();

            playerHealth = player;
            aiHealth = ai;

            SubscribeToHealthEvents();
        }

        #endregion
    }
}
