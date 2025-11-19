using UnityEngine;
using System.Collections.Generic;

namespace Knockout.Audio
{
    /// <summary>
    /// Singleton audio manager that handles all game audio playback.
    /// Provides methods for playing SFX and music with pooling support.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        [Header("Audio Sources")]
        [SerializeField] [Tooltip("Audio source for background music")]
        private AudioSource musicSource;

        [SerializeField] [Range(0f, 1f)] [Tooltip("Master volume multiplier")]
        private float masterVolume = 1f;

        [SerializeField] [Range(0f, 1f)] [Tooltip("SFX volume multiplier")]
        private float sfxVolume = 1f;

        [SerializeField] [Range(0f, 1f)] [Tooltip("Music volume multiplier")]
        private float musicVolume = 0.5f;

        [Header("Audio Source Pool")]
        [SerializeField] [Range(5, 20)] [Tooltip("Number of pooled audio sources for SFX")]
        private int audioSourcePoolSize = 10;

        // Audio source pool for SFX
        private Queue<AudioSource> _audioSourcePool;
        private List<AudioSource> _activeAudioSources;

        #region Singleton

        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AudioManager>();

                    if (_instance == null)
                    {
                        GameObject managerObj = new GameObject("AudioManager");
                        _instance = managerObj.AddComponent<AudioManager>();
                        DontDestroyOnLoad(managerObj);
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSystem();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // Return finished audio sources to pool
            for (int i = _activeAudioSources.Count - 1; i >= 0; i--)
            {
                if (!_activeAudioSources[i].isPlaying)
                {
                    ReturnToPool(_activeAudioSources[i]);
                    _activeAudioSources.RemoveAt(i);
                }
            }
        }

        #endregion

        #region Initialization

        private void InitializeAudioSystem()
        {
            _audioSourcePool = new Queue<AudioSource>();
            _activeAudioSources = new List<AudioSource>();

            // Create music source if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            // Create audio source pool
            for (int i = 0; i < audioSourcePoolSize; i++)
            {
                CreatePooledAudioSource();
            }
        }

        private void CreatePooledAudioSource()
        {
            GameObject sourceObj = new GameObject($"PooledAudioSource_{_audioSourcePool.Count}");
            sourceObj.transform.SetParent(transform);

            AudioSource source = sourceObj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            _audioSourcePool.Enqueue(source);
        }

        #endregion

        #region Audio Playback

        /// <summary>
        /// Plays a sound effect at the specified position.
        /// </summary>
        /// <param name="clip">Audio clip to play</param>
        /// <param name="position">World position for 3D sound</param>
        /// <param name="volumeScale">Volume scale (0-1)</param>
        public void PlaySFX(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Attempted to play null audio clip!");
                return;
            }

            AudioSource source = GetPooledAudioSource();

            if (source == null)
            {
                Debug.LogWarning("AudioManager: No available audio sources in pool!");
                return;
            }

            source.transform.position = position;
            source.clip = clip;
            source.volume = sfxVolume * masterVolume * volumeScale;
            source.spatialBlend = 1f; // Full 3D
            source.Play();

            _activeAudioSources.Add(source);
        }

        /// <summary>
        /// Plays a sound effect without 3D positioning (2D sound).
        /// </summary>
        /// <param name="clip">Audio clip to play</param>
        /// <param name="volumeScale">Volume scale (0-1)</param>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Attempted to play null audio clip!");
                return;
            }

            AudioSource source = GetPooledAudioSource();

            if (source == null)
            {
                Debug.LogWarning("AudioManager: No available audio sources in pool!");
                return;
            }

            source.clip = clip;
            source.volume = sfxVolume * masterVolume * volumeScale;
            source.spatialBlend = 0f; // Full 2D
            source.Play();

            _activeAudioSources.Add(source);
        }

        /// <summary>
        /// Plays background music.
        /// </summary>
        /// <param name="clip">Music clip to play</param>
        /// <param name="loop">Whether to loop the music</param>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Attempted to play null music clip!");
                return;
            }

            if (musicSource == null)
            {
                Debug.LogError("AudioManager: Music source is not assigned!");
                return;
            }

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }

        /// <summary>
        /// Stops currently playing music.
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        #endregion

        #region Audio Source Pool Management

        private AudioSource GetPooledAudioSource()
        {
            if (_audioSourcePool.Count == 0)
            {
                // Pool exhausted, create new source
                Debug.LogWarning("AudioManager: Audio source pool exhausted, creating additional source.");
                CreatePooledAudioSource();
            }

            return _audioSourcePool.Dequeue();
        }

        private void ReturnToPool(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            _audioSourcePool.Enqueue(source);
        }

        #endregion

        #region Volume Control

        /// <summary>
        /// Sets the master volume.
        /// </summary>
        /// <param name="volume">Volume (0-1)</param>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);

            // Update music volume immediately
            if (musicSource != null)
            {
                musicSource.volume = musicVolume * masterVolume;
            }
        }

        /// <summary>
        /// Sets the SFX volume.
        /// </summary>
        /// <param name="volume">Volume (0-1)</param>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Sets the music volume.
        /// </summary>
        /// <param name="volume">Volume (0-1)</param>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);

            // Update music volume immediately
            if (musicSource != null)
            {
                musicSource.volume = musicVolume * masterVolume;
            }
        }

        #endregion
    }
}
