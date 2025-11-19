using System;
using UnityEngine;
using Knockout.Characters.Data;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// Tracks all combat actions and calculates accumulated score for judge decision.
    /// Comprehensive scoring system for determining round winners when time expires.
    /// </summary>
    public class CharacterScoring : MonoBehaviour
    {
        [Header("Scoring Configuration")]
        [SerializeField]
        [Tooltip("Scoring weights for different actions")]
        private ScoringWeights scoringWeights;

        [Header("Aggression Settings")]
        [SerializeField]
        [Tooltip("Distance threshold for aggression scoring (units)")]
        private float aggressionRange = 3f;

        // Component references
        private CharacterCombat _characterCombat;
        private CharacterComboTracker _characterComboTracker;
        private CharacterDodge _characterDodge;
        private CharacterParry _characterParry;
        private CharacterStamina _characterStamina;
        private CharacterSpecialMoves _characterSpecialMoves;

        // Opponent reference for aggression tracking
        private Transform _opponentTransform;

        // Initialization state
        private bool _isInitialized = false;

        // Statistics tracking
        private int _cleanHitsLanded = 0;
        private float _totalDamageDealt = 0f;
        private int _combosCompleted = 0;
        private int _comboSequencesLanded = 0;
        private int _specialMovesLanded = 0;
        private int _knockdownsInflicted = 0;
        private int _blocksSuccessful = 0;
        private int _parriesSuccessful = 0;
        private int _dodgesSuccessful = 0;
        private float _aggressionTimeAccumulated = 0f;
        private int _exhaustionCount = 0;

        // Current calculated score
        private float _totalScore = 0f;

        #region Events

        /// <summary>
        /// Fired when score changes.
        /// Parameter: new total score
        /// </summary>
        public event Action<float> OnScoreChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the total calculated score.
        /// </summary>
        public float TotalScore => _totalScore;

        /// <summary>
        /// Gets the number of clean hits landed.
        /// </summary>
        public int CleanHitsLanded => _cleanHitsLanded;

        /// <summary>
        /// Gets the total damage dealt.
        /// </summary>
        public float TotalDamageDealt => _totalDamageDealt;

        /// <summary>
        /// Gets the number of combos completed.
        /// </summary>
        public int CombosCompleted => _combosCompleted;

        /// <summary>
        /// Gets the number of combo sequences landed.
        /// </summary>
        public int ComboSequencesLanded => _comboSequencesLanded;

        /// <summary>
        /// Gets the number of special moves landed.
        /// </summary>
        public int SpecialMovesLanded => _specialMovesLanded;

        /// <summary>
        /// Gets the number of knockdowns inflicted.
        /// </summary>
        public int KnockdownsInflicted => _knockdownsInflicted;

        /// <summary>
        /// Gets the number of successful blocks.
        /// </summary>
        public int BlocksSuccessful => _blocksSuccessful;

        /// <summary>
        /// Gets the number of successful parries.
        /// </summary>
        public int ParriesSuccessful => _parriesSuccessful;

        /// <summary>
        /// Gets the number of successful dodges.
        /// </summary>
        public int DodgesSuccessful => _dodgesSuccessful;

        /// <summary>
        /// Gets the aggression time accumulated in seconds.
        /// </summary>
        public float AggressionTimeAccumulated => _aggressionTimeAccumulated;

        /// <summary>
        /// Gets the exhaustion count.
        /// </summary>
        public int ExhaustionCount => _exhaustionCount;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache component references
            _characterCombat = GetComponent<CharacterCombat>();
            _characterComboTracker = GetComponent<CharacterComboTracker>();
            _characterDodge = GetComponent<CharacterDodge>();
            _characterParry = GetComponent<CharacterParry>();
            _characterStamina = GetComponent<CharacterStamina>();
            _characterSpecialMoves = GetComponent<CharacterSpecialMoves>();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!_isInitialized) return;

            // Track aggression time
            TrackAggressionTime();
        }

        private void OnDestroy()
        {
            // Unsubscribe from all events
            UnsubscribeFromEvents();
        }

        private void OnValidate()
        {
            if (scoringWeights == null)
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterScoring: ScoringWeights not assigned!", this);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes scoring system and subscribes to events.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            // Validate configuration
            if (scoringWeights == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterScoring: ScoringWeights is required!", this);
                return;
            }

            // Subscribe to all relevant events
            SubscribeToEvents();

            // Find opponent for aggression tracking
            FindOpponent();

            _isInitialized = true;

            // Calculate initial score (should be 0)
            CalculateTotalScore();
        }

        private void SubscribeToEvents()
        {
            // Combat events
            if (_characterCombat != null)
            {
                _characterCombat.OnHitLanded += HandleHitLanded;
                _characterCombat.OnAttackBlocked += HandleAttackBlocked;
                _characterCombat.OnBlockStarted += HandleBlockStarted;
            }

            // Combo events
            if (_characterComboTracker != null)
            {
                _characterComboTracker.OnComboSequenceCompleted += HandleComboSequenceCompleted;
            }

            // Dodge events
            if (_characterDodge != null)
            {
                _characterDodge.OnDodgeStarted += HandleDodgeStarted;
            }

            // Parry events
            if (_characterParry != null)
            {
                _characterParry.OnParrySuccess += HandleParrySuccess;
            }

            // Stamina events
            if (_characterStamina != null)
            {
                _characterStamina.OnStaminaDepleted += HandleStaminaDepleted;
            }

            // Special move events
            if (_characterSpecialMoves != null)
            {
                _characterSpecialMoves.OnSpecialMoveUsed += HandleSpecialMoveUsed;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_characterCombat != null)
            {
                _characterCombat.OnHitLanded -= HandleHitLanded;
                _characterCombat.OnAttackBlocked -= HandleAttackBlocked;
                _characterCombat.OnBlockStarted -= HandleBlockStarted;
            }

            if (_characterComboTracker != null)
            {
                _characterComboTracker.OnComboSequenceCompleted -= HandleComboSequenceCompleted;
            }

            if (_characterDodge != null)
            {
                _characterDodge.OnDodgeStarted -= HandleDodgeStarted;
            }

            if (_characterParry != null)
            {
                _characterParry.OnParrySuccess -= HandleParrySuccess;
            }

            if (_characterStamina != null)
            {
                _characterStamina.OnStaminaDepleted -= HandleStaminaDepleted;
            }

            if (_characterSpecialMoves != null)
            {
                _characterSpecialMoves.OnSpecialMoveUsed -= HandleSpecialMoveUsed;
            }
        }

        private void FindOpponent()
        {
            // Find opponent by tag or other means
            // For now, simple implementation - find other character
            var allCharacters = FindObjectsOfType<CharacterController>();
            foreach (var character in allCharacters)
            {
                if (character.gameObject != gameObject)
                {
                    _opponentTransform = character.transform;
                    break;
                }
            }
        }

        #endregion

        #region Event Handlers

        private void HandleHitLanded(float damage, bool isSpecialMove)
        {
            // Track clean hit
            _cleanHitsLanded++;
            _totalDamageDealt += damage;

            // Track special move landing
            if (isSpecialMove)
            {
                _specialMovesLanded++;
            }

            RecalculateScore();
        }

        private void HandleAttackBlocked()
        {
            // Opponent blocked our attack - no points for us
            // Could track missed opportunities here if needed
        }

        private void HandleBlockStarted()
        {
            // We started blocking - this will be scored when we successfully block a hit
            // Actual block success is tracked via damage reduction in CharacterHealth
        }

        private void HandleComboSequenceCompleted(ComboSequenceData sequenceData)
        {
            _comboSequencesLanded++;
            RecalculateScore();
        }

        private void HandleDodgeStarted(Combat.DodgeDirection direction)
        {
            // Note: Currently counting all dodges
            // Ideally should only count successful dodges that avoided damage
            // This would require integration with hit detection system
            _dodgesSuccessful++;
            RecalculateScore();
        }

        private void HandleParrySuccess(ParryData parryData)
        {
            _parriesSuccessful++;
            RecalculateScore();
        }

        private void HandleStaminaDepleted()
        {
            _exhaustionCount++;
            RecalculateScore();
        }

        private void HandleSpecialMoveUsed(SpecialMoveData specialMoveData)
        {
            // Note: Special move landing is now tracked in HandleHitLanded
            // This just tracks execution (for potential future use)
        }

        #endregion

        #region Public Methods for External Tracking

        /// <summary>
        /// Records a clean hit landed on opponent.
        /// Called by external hit detection system.
        /// </summary>
        /// <param name="damage">Damage dealt</param>
        public void RecordHitLanded(float damage)
        {
            _cleanHitsLanded++;
            _totalDamageDealt += damage;
            RecalculateScore();
        }

        /// <summary>
        /// Records a successful block.
        /// </summary>
        public void RecordBlockSuccessful()
        {
            _blocksSuccessful++;
            RecalculateScore();
        }

        /// <summary>
        /// Records a knockdown inflicted.
        /// </summary>
        public void RecordKnockdownInflicted()
        {
            _knockdownsInflicted++;
            RecalculateScore();
        }

        /// <summary>
        /// Records a combo completed.
        /// </summary>
        /// <param name="comboLength">Number of hits in combo</param>
        public void RecordComboCompleted(int comboLength)
        {
            if (comboLength >= 2)
            {
                _combosCompleted++;
                RecalculateScore();
            }
        }

        #endregion

        #region Score Calculation

        private void TrackAggressionTime()
        {
            if (_opponentTransform == null || scoringWeights == null) return;

            // Check if within aggression range
            float distanceToOpponent = Vector3.Distance(transform.position, _opponentTransform.position);
            if (distanceToOpponent < aggressionRange)
            {
                // Accumulate aggression time
                _aggressionTimeAccumulated += Time.deltaTime;
            }
        }

        /// <summary>
        /// Calculates total score from all statistics.
        /// </summary>
        public float CalculateTotalScore()
        {
            if (scoringWeights == null) return 0f;

            float score = 0f;

            // Offensive scoring
            score += _cleanHitsLanded * scoringWeights.CleanHitPoints;
            score += _combosCompleted * scoringWeights.ComboHitBonus;
            score += _comboSequencesLanded * scoringWeights.ComboSequencePoints;
            score += _specialMovesLanded * scoringWeights.SpecialMovePoints;
            score += _knockdownsInflicted * scoringWeights.KnockdownPoints;
            score += _totalDamageDealt * scoringWeights.DamageDealtWeight;

            // Defensive scoring
            score += _blocksSuccessful * scoringWeights.BlockPoints;
            score += _parriesSuccessful * scoringWeights.ParryPoints;
            score += _dodgesSuccessful * scoringWeights.DodgePoints;

            // Control scoring
            score += _aggressionTimeAccumulated * scoringWeights.AggressionPointsPerSecond;

            // Penalties
            score -= _exhaustionCount * scoringWeights.ExhaustionPenalty;

            // Ensure non-negative
            score = Mathf.Max(0f, score);

            _totalScore = score;
            return score;
        }

        private void RecalculateScore()
        {
            float previousScore = _totalScore;
            CalculateTotalScore();

            // Fire event if score changed
            if (Mathf.Abs(_totalScore - previousScore) > 0.001f)
            {
                OnScoreChanged?.Invoke(_totalScore);
            }
        }

        /// <summary>
        /// Resets all scoring statistics to zero.
        /// Called at round start.
        /// </summary>
        public void ResetScore()
        {
            _cleanHitsLanded = 0;
            _totalDamageDealt = 0f;
            _combosCompleted = 0;
            _comboSequencesLanded = 0;
            _specialMovesLanded = 0;
            _knockdownsInflicted = 0;
            _blocksSuccessful = 0;
            _parriesSuccessful = 0;
            _dodgesSuccessful = 0;
            _aggressionTimeAccumulated = 0f;
            _exhaustionCount = 0;

            _totalScore = 0f;
            OnScoreChanged?.Invoke(_totalScore);
        }

        #endregion
    }
}
