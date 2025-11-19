using System;
using System.Collections.Generic;
using UnityEngine;
using Knockout.Characters.Data;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// Tracks combo state, detects natural chains and predefined sequences, and applies damage scaling.
    /// Manages combo interruption from blocking or getting hit.
    /// </summary>
    public class CharacterComboTracker : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        [Tooltip("Combo chain timing and damage scaling configuration")]
        private ComboChainData comboChainData;

        [SerializeField]
        [Tooltip("Predefined combo sequences for this character")]
        private ComboSequenceData[] comboSequences = new ComboSequenceData[0];

        // Component references
        private CharacterCombat _characterCombat;
        private CharacterHealth _characterHealth;
        private CharacterParry _characterParry;

        // Combo state
        private int _comboCount = 0;
        private List<int> _attackSequenceHistory = new List<int>();
        private int _lastAttackFrame = -1000; // Frame when last attack landed
        private int _lastAttackTypeIndex = -1;
        private bool _isInitialized = false;

        // Counter window tracking
        private bool _isInCounterWindow = false;
        private int _counterWindowEndFrame = -1;

        // Frame tracking (at 60fps)
        private const int TARGET_FRAME_RATE = 60;
        private int _currentFrame = 0;

        #region Events

        /// <summary>
        /// Fired when a combo starts (first hit).
        /// </summary>
        public event Action OnComboStarted;

        /// <summary>
        /// Fired when a combo hit lands.
        /// Parameters: (hitNumber, damageDealt)
        /// </summary>
        public event Action<int, float> OnComboHitLanded;

        /// <summary>
        /// Fired when a predefined combo sequence is completed.
        /// </summary>
        public event Action<ComboSequenceData> OnComboSequenceCompleted;

        /// <summary>
        /// Fired when a combo is broken by block or hit.
        /// Parameter: finalComboCount
        /// </summary>
        public event Action<int> OnComboBroken;

        /// <summary>
        /// Fired when a combo ends naturally (timeout).
        /// Parameters: (finalComboCount, totalDamage)
        /// </summary>
        public event Action<int, float> OnComboEnded;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current combo hit count.
        /// </summary>
        public int ComboCount => _comboCount;

        /// <summary>
        /// Whether currently in an active combo.
        /// </summary>
        public bool IsInCombo => _comboCount > 0;

        /// <summary>
        /// Whether currently in a parry counter window.
        /// </summary>
        public bool IsInCounterWindow => _isInCounterWindow;

        /// <summary>
        /// Current attack sequence being built.
        /// </summary>
        public IReadOnlyList<int> CurrentSequence => _attackSequenceHistory.AsReadOnly();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache component references
            _characterCombat = GetComponent<CharacterCombat>();
            _characterHealth = GetComponent<CharacterHealth>();
            _characterParry = GetComponent<CharacterParry>();

            if (_characterCombat == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterComboTracker requires CharacterCombat component!", this);
            }

            if (_characterHealth == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterComboTracker requires CharacterHealth component!", this);
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void FixedUpdate()
        {
            if (!_isInitialized) return;

            // Increment frame counter
            _currentFrame++;

            // Update counter window
            UpdateCounterWindow();

            // Check for combo timeout
            CheckComboTimeout();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_characterCombat != null)
            {
                _characterCombat.OnAttackExecuted -= HandleAttackExecuted;
                _characterCombat.OnBlockStarted -= HandleBlockStarted;
            }

            if (_characterHealth != null)
            {
                _characterHealth.OnHitTaken -= HandleHitTaken;
            }

            if (_characterParry != null)
            {
                _characterParry.OnCounterWindowOpened -= HandleCounterWindowOpened;
                _characterParry.OnCounterWindowClosed -= HandleCounterWindowClosed;
            }
        }

        private void OnValidate()
        {
            if (comboChainData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterComboTracker: ComboChainData not assigned!", this);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes combo tracking and subscribes to events.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            // Validate required data
            if (comboChainData == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterComboTracker: ComboChainData is required!", this);
                return;
            }

            // Subscribe to combat events
            if (_characterCombat != null)
            {
                _characterCombat.OnAttackExecuted += HandleAttackExecuted;
                _characterCombat.OnBlockStarted += HandleBlockStarted;
            }

            // Subscribe to health events (for getting hit)
            if (_characterHealth != null)
            {
                _characterHealth.OnHitTaken += HandleHitTaken;
            }

            // Subscribe to parry events (for counter window)
            if (_characterParry != null)
            {
                _characterParry.OnCounterWindowOpened += HandleCounterWindowOpened;
                _characterParry.OnCounterWindowClosed += HandleCounterWindowClosed;
            }

            _isInitialized = true;
        }

        #endregion

        #region Combo Tracking

        /// <summary>
        /// Registers a hit landing and updates combo state.
        /// Called externally when this character's attack lands.
        /// </summary>
        /// <param name="attackTypeIndex">Type of attack that landed (0=Jab, 1=Hook, 2=Uppercut)</param>
        /// <param name="baseDamage">Base damage of the attack (before combo scaling)</param>
        /// <returns>Final damage multiplier to apply (combo scaling * sequence bonus)</returns>
        public float RegisterHitLanded(int attackTypeIndex, float baseDamage)
        {
            // Check if this is within chain window from last attack
            bool isWithinChainWindow = IsWithinChainWindow(attackTypeIndex);

            if (isWithinChainWindow)
            {
                // Increment combo
                _comboCount++;
                _attackSequenceHistory.Add(attackTypeIndex);
            }
            else
            {
                // Start new combo
                if (_comboCount > 0)
                {
                    // Previous combo ended
                    OnComboEnded?.Invoke(_comboCount, 0f); // TODO: Track total damage
                }

                _comboCount = 1;
                _attackSequenceHistory.Clear();
                _attackSequenceHistory.Add(attackTypeIndex);

                // Fire combo started event
                OnComboStarted?.Invoke();
            }

            // Update last attack timing
            _lastAttackFrame = _currentFrame;
            _lastAttackTypeIndex = attackTypeIndex;

            // Get damage multiplier
            float damageMultiplier = GetCurrentDamageMultiplier();

            // Check for sequence completion
            ComboSequenceData completedSequence = CheckForSequenceCompletion();
            if (completedSequence != null)
            {
                // Apply sequence bonus
                damageMultiplier *= completedSequence.DamageBonusMultiplier;

                // Fire sequence completed event
                OnComboSequenceCompleted?.Invoke(completedSequence);
            }

            // Calculate final damage
            float finalDamage = baseDamage * damageMultiplier;

            // Fire combo hit event
            OnComboHitLanded?.Invoke(_comboCount, finalDamage);

            return damageMultiplier;
        }

        /// <summary>
        /// Gets the current combo damage multiplier based on combo position.
        /// </summary>
        public float GetCurrentDamageMultiplier()
        {
            if (_comboCount <= 0) return 1.0f;

            float baseMultiplier = comboChainData.GetDamageScale(_comboCount);

            // Apply counter window bonus if active and this is first hit
            if (_isInCounterWindow && _comboCount == 1)
            {
                baseMultiplier *= comboChainData.CounterWindowDamageBonus;
            }

            return baseMultiplier;
        }

        /// <summary>
        /// Resets combo state to zero.
        /// </summary>
        public void ResetCombo()
        {
            if (_comboCount > 0)
            {
                int finalCount = _comboCount;
                _comboCount = 0;
                _attackSequenceHistory.Clear();
                _lastAttackFrame = _currentFrame - 1000; // Reset to far in past

                // Fire combo ended event
                OnComboEnded?.Invoke(finalCount, 0f);
            }
        }

        /// <summary>
        /// Breaks the current combo (from block or hit).
        /// Called externally when attacks are blocked.
        /// </summary>
        public void BreakCombo()
        {
            if (_comboCount > 0)
            {
                int finalCount = _comboCount;
                _comboCount = 0;
                _attackSequenceHistory.Clear();
                _lastAttackFrame = _currentFrame - 1000; // Reset to far in past

                // Fire combo broken event
                OnComboBroken?.Invoke(finalCount);
            }
        }

        #endregion

        #region Timing Checks

        /// <summary>
        /// Checks if the current attack is within the chain window from the last attack.
        /// </summary>
        private bool IsWithinChainWindow(int attackTypeIndex)
        {
            // If no previous attack, not in chain
            if (_lastAttackFrame < 0 || _lastAttackTypeIndex < 0)
            {
                return false;
            }

            // Calculate frames since last attack
            int framesSinceLastAttack = _currentFrame - _lastAttackFrame;

            // Get chain window for the LAST attack type (window starts after previous attack recovers)
            int chainWindow = comboChainData.GetChainWindow(_lastAttackTypeIndex);

            // Check if within chain window
            return framesSinceLastAttack <= chainWindow;
        }

        /// <summary>
        /// Checks if combo has timed out and resets if necessary.
        /// </summary>
        private void CheckComboTimeout()
        {
            if (_comboCount <= 0) return;

            // Calculate frames since last attack
            int framesSinceLastAttack = _currentFrame - _lastAttackFrame;

            // Check against global timeout
            if (framesSinceLastAttack > comboChainData.GlobalComboTimeoutFrames)
            {
                // Combo timed out
                ResetCombo();
            }
        }

        #endregion

        #region Sequence Detection

        /// <summary>
        /// Checks if the current attack sequence matches any predefined combos.
        /// Returns the completed sequence if found, null otherwise.
        /// </summary>
        private ComboSequenceData CheckForSequenceCompletion()
        {
            if (comboSequences == null || comboSequences.Length == 0)
            {
                return null;
            }

            // Check each predefined sequence
            foreach (var sequence in comboSequences)
            {
                if (sequence == null) continue;

                // Check if current history matches this sequence
                if (IsSequenceMatch(sequence))
                {
                    return sequence;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the current attack history matches a specific sequence.
        /// </summary>
        private bool IsSequenceMatch(ComboSequenceData sequence)
        {
            int[] targetSequence = sequence.AttackSequence;
            int sequenceLength = targetSequence.Length;

            // Need at least as many attacks as sequence length
            if (_attackSequenceHistory.Count < sequenceLength)
            {
                return false;
            }

            // Check if the last N attacks match the sequence
            int startIndex = _attackSequenceHistory.Count - sequenceLength;
            for (int i = 0; i < sequenceLength; i++)
            {
                if (_attackSequenceHistory[startIndex + i] != targetSequence[i])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Event Handlers

        private void HandleAttackExecuted(int attackTypeIndex)
        {
            // Attack executed - timing starts here
            // Actual combo registration happens when hit lands (RegisterHitLanded)
        }

        private void HandleBlockStarted()
        {
            // When this character blocks, it doesn't break their own combo
            // Combo breaking from opponent blocking is handled externally
        }

        private void HandleHitTaken(Combat.HitDetection.HitData hitData)
        {
            // Getting hit breaks combo immediately
            BreakCombo();
        }

        private void HandleCounterWindowOpened()
        {
            _isInCounterWindow = true;
            // Counter window duration comes from ParryData, tracked by frame
        }

        private void HandleCounterWindowClosed()
        {
            _isInCounterWindow = false;
        }

        private void UpdateCounterWindow()
        {
            // Counter window is managed by CharacterParry via events
            // This method reserved for future frame-based tracking if needed
        }

        #endregion
    }
}
