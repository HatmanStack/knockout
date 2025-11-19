using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using Knockout.UI;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.Tests.PlayMode.UI
{
    /// <summary>
    /// PlayMode tests for ComboCounterUI component.
    /// Tests contextual display, animations, and sequence completion effects.
    /// </summary>
    public class ComboCounterUITests
    {
        private GameObject _testCharacter;
        private GameObject _uiGameObject;
        private CharacterComboTracker _comboTracker;
        private ComboCounterUI _comboCounterUI;
        private TextMeshProUGUI _comboCountText;
        private TextMeshProUGUI _comboLabelText;
        private TextMeshProUGUI _sequenceNameText;
        private GameObject _rootGameObject;
        private ComboChainData _testChainData;

        [SetUp]
        public void SetUp()
        {
            // Create test chain data
            _testChainData = ScriptableObject.CreateInstance<ComboChainData>();

            // Create test character with combo tracker
            _testCharacter = new GameObject("TestCharacter");
            _comboTracker = _testCharacter.AddComponent<CharacterComboTracker>();

            // Use reflection to set chain data
            var chainDataField = typeof(CharacterComboTracker).GetField("comboChainData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            chainDataField?.SetValue(_comboTracker, _testChainData);

            // Initialize combo tracker
            _comboTracker.Initialize();

            // Create UI hierarchy
            _rootGameObject = new GameObject("ComboCounterRoot");
            _uiGameObject = new GameObject("ComboCounterUI");
            _uiGameObject.transform.SetParent(_rootGameObject.transform);

            // Create UI text elements
            var countObj = new GameObject("Count");
            countObj.transform.SetParent(_uiGameObject.transform);
            _comboCountText = countObj.AddComponent<TextMeshProUGUI>();

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(_uiGameObject.transform);
            _comboLabelText = labelObj.AddComponent<TextMeshProUGUI>();

            var sequenceObj = new GameObject("Sequence");
            sequenceObj.transform.SetParent(_uiGameObject.transform);
            _sequenceNameText = sequenceObj.AddComponent<TextMeshProUGUI>();

            // Create ComboCounterUI component
            _comboCounterUI = _uiGameObject.AddComponent<ComboCounterUI>();

            // Set references using reflection
            SetPrivateField(_comboCounterUI, "comboCountText", _comboCountText);
            SetPrivateField(_comboCounterUI, "comboLabelText", _comboLabelText);
            SetPrivateField(_comboCounterUI, "sequenceNameText", _sequenceNameText);
            SetPrivateField(_comboCounterUI, "comboCounterRoot", _rootGameObject);
            SetPrivateField(_comboCounterUI, "comboTracker", _comboTracker);
            SetPrivateField(_comboCounterUI, "minimumHitsToShow", 2);

            // Trigger Awake() and Start() manually
            _comboCounterUI.SendMessage("Awake");
            _comboCounterUI.SendMessage("Start");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testCharacter);
            Object.DestroyImmediate(_rootGameObject);
            Object.DestroyImmediate(_testChainData);
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        [UnityTest]
        public IEnumerator CounterHiddenWhenNoCombo()
        {
            // Counter should start hidden
            Assert.IsFalse(_rootGameObject.activeSelf, "Combo counter should be hidden initially");
            yield return null;
        }

        [UnityTest]
        public IEnumerator CounterAppearsWhenComboStarts()
        {
            // Simulate combo hits
            var onComboHitLandedEvent = typeof(CharacterComboTracker).GetField("OnComboHitLanded",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var eventDelegate = onComboHitLandedEvent?.GetValue(_comboTracker) as System.MulticastDelegate;

            // First hit (should not show - minimum is 2)
            _comboTracker.GetType().GetField("OnComboHitLanded",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?
                .GetValue(_comboTracker)?.GetType().GetMethod("Invoke")?.Invoke(
                    onComboHitLandedEvent?.GetValue(_comboTracker),
                    new object[] { 1, 10f });

            yield return null;
            Assert.IsFalse(_rootGameObject.activeSelf, "Counter should not show for 1 hit");

            // Second hit (should show)
            _comboTracker.GetType().GetField("OnComboHitLanded",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?
                .GetValue(_comboTracker)?.GetType().GetMethod("Invoke")?.Invoke(
                    onComboHitLandedEvent?.GetValue(_comboTracker),
                    new object[] { 2, 20f });

            yield return new WaitForSeconds(0.1f);
            Assert.IsTrue(_rootGameObject.activeSelf, "Counter should appear for 2+ hits");
        }

        [UnityTest]
        public IEnumerator CountUpdatesOnEachHit()
        {
            // Trigger hits
            var onComboHitLandedEvent = typeof(CharacterComboTracker).GetField("OnComboHitLanded",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            // Hit 2
            onComboHitLandedEvent?.GetValue(_comboTracker)?.GetType().GetMethod("Invoke")?.Invoke(
                onComboHitLandedEvent?.GetValue(_comboTracker),
                new object[] { 2, 20f });

            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual("2", _comboCountText.text, "Count should show 2");

            // Hit 3
            onComboHitLandedEvent?.GetValue(_comboTracker)?.GetType().GetMethod("Invoke")?.Invoke(
                onComboHitLandedEvent?.GetValue(_comboTracker),
                new object[] { 3, 30f });

            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual("3", _comboCountText.text, "Count should show 3");
        }

        [UnityTest]
        public IEnumerator CounterHidesWhenComboEnds()
        {
            // Start combo
            var onComboHitLandedEvent = typeof(CharacterComboTracker).GetField("OnComboHitLanded",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            onComboHitLandedEvent?.GetValue(_comboTracker)?.GetType().GetMethod("Invoke")?.Invoke(
                onComboHitLandedEvent?.GetValue(_comboTracker),
                new object[] { 3, 30f });

            yield return new WaitForSeconds(0.1f);
            Assert.IsTrue(_rootGameObject.activeSelf, "Counter should be visible");

            // End combo
            var onComboEndedEvent = typeof(CharacterComboTracker).GetField("OnComboEnded",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            onComboEndedEvent?.GetValue(_comboTracker)?.GetType().GetMethod("Invoke")?.Invoke(
                onComboEndedEvent?.GetValue(_comboTracker),
                new object[] { 3, 30f });

            // Wait for fade out
            yield return new WaitForSeconds(1.5f);
            Assert.IsFalse(_rootGameObject.activeSelf, "Counter should hide after combo ends");
        }

        [UnityTest]
        public IEnumerator SequenceNameDisplaysOnCompletion()
        {
            // Create test sequence data
            var sequenceData = ScriptableObject.CreateInstance<ComboSequenceData>();
            var nameField = typeof(ComboSequenceData).GetField("sequenceName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nameField?.SetValue(sequenceData, "Test Sequence");

            // Trigger sequence completion
            var onSequenceCompletedEvent = typeof(CharacterComboTracker).GetField("OnComboSequenceCompleted",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            onSequenceCompletedEvent?.GetValue(_comboTracker)?.GetType().GetMethod("Invoke")?.Invoke(
                onSequenceCompletedEvent?.GetValue(_comboTracker),
                new object[] { sequenceData });

            yield return new WaitForSeconds(0.1f);

            // Sequence name should be visible
            Assert.IsTrue(_sequenceNameText.gameObject.activeSelf, "Sequence name should be visible");
            Assert.IsTrue(_sequenceNameText.text.Contains("TEST SEQUENCE"), "Sequence name should display");

            Object.DestroyImmediate(sequenceData);
        }

        [UnityTest]
        public IEnumerator SetComboTrackerUpdatesReference()
        {
            // Create new combo tracker
            var newCharacter = new GameObject("NewCharacter");
            var newTracker = newCharacter.AddComponent<CharacterComboTracker>();

            var chainDataField = typeof(CharacterComboTracker).GetField("comboChainData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            chainDataField?.SetValue(newTracker, _testChainData);
            newTracker.Initialize();

            // Set new tracker
            _comboCounterUI.SetComboTracker(newTracker);

            yield return null;

            // Trigger event on new tracker
            var onComboHitLandedEvent = typeof(CharacterComboTracker).GetField("OnComboHitLanded",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            onComboHitLandedEvent?.GetValue(newTracker)?.GetType().GetMethod("Invoke")?.Invoke(
                onComboHitLandedEvent?.GetValue(newTracker),
                new object[] { 3, 30f });

            yield return new WaitForSeconds(0.1f);

            // UI should update for new tracker
            Assert.IsTrue(_rootGameObject.activeSelf, "UI should respond to new tracker");

            Object.DestroyImmediate(newCharacter);
        }
    }
}
