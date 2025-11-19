using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Audio;

namespace Knockout.Tests.PlayMode.Audio
{
    public class AudioManagerTests
    {
        private GameObject _audioManagerObj;
        private AudioManager _audioManager;

        [SetUp]
        public void SetUp()
        {
            // Destroy any existing AudioManager instances
            var existing = Object.FindObjectOfType<AudioManager>();
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            // Create new AudioManager
            _audioManagerObj = new GameObject("AudioManager");
            _audioManager = _audioManagerObj.AddComponent<AudioManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_audioManagerObj != null)
            {
                Object.DestroyImmediate(_audioManagerObj);
            }
        }

        [UnityTest]
        public IEnumerator AudioManager_IsSingleton()
        {
            // Act
            yield return null;
            var instance1 = AudioManager.Instance;
            var instance2 = AudioManager.Instance;

            // Assert
            Assert.AreSame(instance1, instance2, "AudioManager should be a singleton");
        }

        [UnityTest]
        public IEnumerator PlaySFX_WithNullClip_DoesNotThrowError()
        {
            // Act & Assert
            yield return null;
            Assert.DoesNotThrow(() =>
            {
                AudioManager.Instance.PlaySFX(null, Vector3.zero);
            });
        }

        [UnityTest]
        public IEnumerator SetMasterVolume_UpdatesVolume()
        {
            // Arrange
            yield return null;

            // Act
            AudioManager.Instance.SetMasterVolume(0.5f);

            // Assert
            var volumeField = typeof(AudioManager).GetField("masterVolume",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            float volume = (float)volumeField.GetValue(AudioManager.Instance);

            Assert.AreEqual(0.5f, volume, 0.01f, "Master volume should be set to 0.5");
        }

        [UnityTest]
        public IEnumerator SetSFXVolume_UpdatesVolume()
        {
            // Arrange
            yield return null;

            // Act
            AudioManager.Instance.SetSFXVolume(0.7f);

            // Assert
            var volumeField = typeof(AudioManager).GetField("sfxVolume",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            float volume = (float)volumeField.GetValue(AudioManager.Instance);

            Assert.AreEqual(0.7f, volume, 0.01f, "SFX volume should be set to 0.7");
        }

        [UnityTest]
        public IEnumerator SetMusicVolume_UpdatesVolume()
        {
            // Arrange
            yield return null;

            // Act
            AudioManager.Instance.SetMusicVolume(0.3f);

            // Assert
            var volumeField = typeof(AudioManager).GetField("musicVolume",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            float volume = (float)volumeField.GetValue(AudioManager.Instance);

            Assert.AreEqual(0.3f, volume, 0.01f, "Music volume should be set to 0.3");
        }

        [UnityTest]
        public IEnumerator StopMusic_DoesNotThrowError()
        {
            // Act & Assert
            yield return null;
            Assert.DoesNotThrow(() =>
            {
                AudioManager.Instance.StopMusic();
            });
        }
    }
}
