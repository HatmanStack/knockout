using UnityEngine;
using Knockout.Characters.Components;
using Knockout.Combat.HitDetection;

namespace Knockout.Audio
{
    /// <summary>
    /// Handles audio playback for character combat events.
    /// Subscribes to combat and health events to play appropriate sounds.
    ///
    /// Voice Line Hook Points (for character-specific soundbytes):
    /// - OnAttackStart: Play taunt/battle cry when attacking
    /// - OnHitTaken: Play grunt/exclamation when taking damage
    /// - OnKnockout: Play defeat line when knocked out
    /// - OnRoundWin: Play victory line when winning round (requires RoundManager integration)
    /// - OnRoundStart: Play intro line at round start (requires RoundManager integration)
    ///
    /// For character voice implementation, see CharacterVoiceData ScriptableObject.
    /// </summary>
    public class CharacterAudioPlayer : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] [Tooltip("Character combat component")]
        private CharacterCombat characterCombat;

        [SerializeField] [Tooltip("Character health component")]
        private CharacterHealth characterHealth;

        [Header("Combat Sound Effects")]
        [SerializeField] [Tooltip("Sound played when jab attack starts")]
        private AudioClip jabSound;

        [SerializeField] [Tooltip("Sound played when hook attack starts")]
        private AudioClip hookSound;

        [SerializeField] [Tooltip("Sound played when uppercut attack starts")]
        private AudioClip uppercutSound;

        [SerializeField] [Tooltip("Sound played when hit is taken")]
        private AudioClip hitSound;

        [SerializeField] [Tooltip("Sound played when blocking")]
        private AudioClip blockSound;

        [SerializeField] [Tooltip("Sound played on knockout")]
        private AudioClip knockoutSound;

        [Header("Voice Line Settings (Optional)")]
        [SerializeField] [Tooltip("Character voice data for personality-specific soundbytes")]
        private CharacterVoiceData voiceData;

        [SerializeField] [Range(0f, 1f)] [Tooltip("Chance to play voice line on events (0 = never, 1 = always)")]
        private float voiceLineChance = 0.3f;

        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] [Tooltip("Volume scale for combat sounds")]
        private float combatSoundVolume = 1f;

        [SerializeField] [Range(0f, 1f)] [Tooltip("Volume scale for voice lines")]
        private float voiceLineVolume = 0.8f;

        private void Awake()
        {
            CacheComponents();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void CacheComponents()
        {
            if (characterCombat == null)
            {
                characterCombat = GetComponent<CharacterCombat>();
            }

            if (characterHealth == null)
            {
                characterHealth = GetComponent<CharacterHealth>();
            }
        }

        #region Event Subscription

        private void SubscribeToEvents()
        {
            if (characterCombat != null)
            {
                characterCombat.OnAttackExecuted += OnAttackExecuted;
                characterCombat.OnBlockStarted += OnBlockStarted;
            }

            if (characterHealth != null)
            {
                characterHealth.OnHitTaken += OnHitTaken;
                characterHealth.OnDeath += OnDeath;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (characterCombat != null)
            {
                characterCombat.OnAttackExecuted -= OnAttackExecuted;
                characterCombat.OnBlockStarted -= OnBlockStarted;
            }

            if (characterHealth != null)
            {
                characterHealth.OnHitTaken -= OnHitTaken;
                characterHealth.OnDeath -= OnDeath;
            }
        }

        #endregion

        #region Event Handlers

        private void OnAttackExecuted(int attackType)
        {
            // Play combat sound based on attack type
            AudioClip attackSound = GetAttackSound(attackType);

            if (attackSound != null)
            {
                AudioManager.Instance.PlaySFX(attackSound, transform.position, combatSoundVolume);
            }

            // Optionally play character voice line
            if (voiceData != null && Random.value < voiceLineChance)
            {
                AudioClip voiceLine = voiceData.GetRandomAttackVoiceLine();
                if (voiceLine != null)
                {
                    AudioManager.Instance.PlaySFX(voiceLine, transform.position, voiceLineVolume);
                }
            }
        }

        private void OnBlockStarted()
        {
            if (blockSound != null)
            {
                AudioManager.Instance.PlaySFX(blockSound, transform.position, combatSoundVolume);
            }
        }

        private void OnHitTaken(HitData hitData)
        {
            if (hitSound != null)
            {
                AudioManager.Instance.PlaySFX(hitSound, transform.position, combatSoundVolume);
            }

            // Optionally play character voice line
            if (voiceData != null && Random.value < voiceLineChance)
            {
                AudioClip voiceLine = voiceData.GetRandomHitVoiceLine();
                if (voiceLine != null)
                {
                    AudioManager.Instance.PlaySFX(voiceLine, transform.position, voiceLineVolume);
                }
            }
        }

        private void OnDeath()
        {
            if (knockoutSound != null)
            {
                AudioManager.Instance.PlaySFX(knockoutSound, transform.position, combatSoundVolume);
            }

            // Always play knockout voice line if available
            if (voiceData != null)
            {
                AudioClip voiceLine = voiceData.GetRandomKnockoutVoiceLine();
                if (voiceLine != null)
                {
                    AudioManager.Instance.PlaySFX(voiceLine, transform.position, voiceLineVolume);
                }
            }
        }

        #endregion

        #region Helper Methods

        private AudioClip GetAttackSound(int attackType)
        {
            // Attack types: 0 = jab, 1 = hook, 2 = uppercut
            switch (attackType)
            {
                case 0:
                    return jabSound;
                case 1:
                    return hookSound;
                case 2:
                    return uppercutSound;
                default:
                    return jabSound;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Plays a victory voice line.
        /// Intended to be called by RoundManager on round win.
        /// </summary>
        public void PlayVictoryVoiceLine()
        {
            if (voiceData != null)
            {
                AudioClip voiceLine = voiceData.GetRandomVictoryVoiceLine();
                if (voiceLine != null)
                {
                    AudioManager.Instance.PlaySFX(voiceLine, transform.position, voiceLineVolume);
                }
            }
        }

        /// <summary>
        /// Plays an intro voice line.
        /// Intended to be called by RoundManager at round start.
        /// </summary>
        public void PlayIntroVoiceLine()
        {
            if (voiceData != null)
            {
                AudioClip voiceLine = voiceData.GetRandomIntroVoiceLine();
                if (voiceLine != null)
                {
                    AudioManager.Instance.PlaySFX(voiceLine, transform.position, voiceLineVolume);
                }
            }
        }

        #endregion
    }
}
