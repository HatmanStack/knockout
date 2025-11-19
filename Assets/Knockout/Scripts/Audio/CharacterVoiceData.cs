using UnityEngine;

namespace Knockout.Audio
{
    /// <summary>
    /// ScriptableObject containing character-specific voice line audio clips.
    /// Used by CharacterAudioPlayer to add personality and humor to characters.
    ///
    /// Voice Line Categories:
    /// - Attack: Played when character throws a punch (taunt/battle cry)
    /// - Hit: Played when character takes damage (grunt/exclamation)
    /// - Knockout: Played when character is knocked out (defeat line)
    /// - Victory: Played when character wins a round (victory line)
    /// - Intro: Played at the start of a round (intro line)
    ///
    /// Usage:
    /// 1. Create ScriptableObject via Assets > Create > Knockout > Character Voice Data
    /// 2. Populate with character-specific voice clips (e.g., from Assets/TrumpSound/)
    /// 3. Assign to CharacterAudioPlayer component
    ///
    /// Note: These are character personality soundbytes, not game sound effects.
    /// Organize voice clips in Assets/Knockout/Audio/Voices/[CharacterName]/
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterVoiceData", menuName = "Knockout/Character Voice Data")]
    public class CharacterVoiceData : ScriptableObject
    {
        [Header("Attack Voice Lines")]
        [Tooltip("Voice lines played when attacking (taunts, battle cries)")]
        [SerializeField]
        private AudioClip[] attackVoiceLines;

        [Header("Hit Voice Lines")]
        [Tooltip("Voice lines played when taking damage (grunts, exclamations)")]
        [SerializeField]
        private AudioClip[] hitVoiceLines;

        [Header("Knockout Voice Lines")]
        [Tooltip("Voice lines played when knocked out (defeat lines)")]
        [SerializeField]
        private AudioClip[] knockoutVoiceLines;

        [Header("Victory Voice Lines")]
        [Tooltip("Voice lines played when winning a round (victory lines)")]
        [SerializeField]
        private AudioClip[] victoryVoiceLines;

        [Header("Intro Voice Lines")]
        [Tooltip("Voice lines played at round start (intro lines)")]
        [SerializeField]
        private AudioClip[] introVoiceLines;

        #region Public Methods

        /// <summary>
        /// Gets a random attack voice line.
        /// </summary>
        public AudioClip GetRandomAttackVoiceLine()
        {
            return GetRandomClip(attackVoiceLines);
        }

        /// <summary>
        /// Gets a random hit voice line.
        /// </summary>
        public AudioClip GetRandomHitVoiceLine()
        {
            return GetRandomClip(hitVoiceLines);
        }

        /// <summary>
        /// Gets a random knockout voice line.
        /// </summary>
        public AudioClip GetRandomKnockoutVoiceLine()
        {
            return GetRandomClip(knockoutVoiceLines);
        }

        /// <summary>
        /// Gets a random victory voice line.
        /// </summary>
        public AudioClip GetRandomVictoryVoiceLine()
        {
            return GetRandomClip(victoryVoiceLines);
        }

        /// <summary>
        /// Gets a random intro voice line.
        /// </summary>
        public AudioClip GetRandomIntroVoiceLine()
        {
            return GetRandomClip(introVoiceLines);
        }

        #endregion

        #region Helper Methods

        private AudioClip GetRandomClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }

            int randomIndex = Random.Range(0, clips.Length);
            return clips[randomIndex];
        }

        #endregion

        #region Validation

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Warn if no clips are assigned
            int totalClips = 0;

            if (attackVoiceLines != null) totalClips += attackVoiceLines.Length;
            if (hitVoiceLines != null) totalClips += hitVoiceLines.Length;
            if (knockoutVoiceLines != null) totalClips += knockoutVoiceLines.Length;
            if (victoryVoiceLines != null) totalClips += victoryVoiceLines.Length;
            if (introVoiceLines != null) totalClips += introVoiceLines.Length;

            if (totalClips == 0)
            {
                Debug.LogWarning($"CharacterVoiceData '{name}': No voice lines assigned!", this);
            }
        }
        #endif

        #endregion
    }
}
