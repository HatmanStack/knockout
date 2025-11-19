using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;
using Knockout.UI;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.Tests.PlayMode.UI
{
    /// <summary>
    /// PlayMode tests for SpecialMoveCooldownUI component.
    /// Tests cooldown display, readiness indication, and dual-gating with stamina.
    /// </summary>
    public class SpecialMoveCooldownUITests
    {
        private GameObject _testCharacter;
        private GameObject _uiGameObject;
        private CharacterSpecialMoves _specialMoves;
        private CharacterStamina _characterStamina;
        private SpecialMoveCooldownUI _cooldownUI;
        private Image _iconImage;
        private Image _cooldownOverlay;
        private GameObject _readyIndicator;
        private SpecialMoveData _testMoveData;
        private StaminaData _testStaminaData;

        [SetUp]
        public void SetUp()
        {
            // Create test data
            _testMoveData = ScriptableObject.CreateInstance<SpecialMoveData>();
            _testStaminaData = ScriptableObject.CreateInstance<StaminaData>();

            // Create test character with components
            _testCharacter = new GameObject("TestCharacter");
            _specialMoves = _testCharacter.AddComponent<CharacterSpecialMoves>();
            _characterStamina = _testCharacter.AddComponent<CharacterStamina>();

            // Set data via reflection
            var moveDataField = typeof(CharacterSpecialMoves).GetField("specialMoveData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            moveDataField?.SetValue(_specialMoves, _testMoveData);

            var staminaDataField = typeof(CharacterStamina).GetField("staminaData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            staminaDataField?.SetValue(_characterStamina, _testStaminaData);

            // Initialize components
            _characterStamina.Initialize();
            _specialMoves.Initialize();

            // Create UI hierarchy
            _uiGameObject = new GameObject("CooldownUI");

            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(_uiGameObject.transform);
            _iconImage = iconObj.AddComponent<Image>();

            var overlayObj = new GameObject("Overlay");
            overlayObj.transform.SetParent(_uiGameObject.transform);
            _cooldownOverlay = overlayObj.AddComponent<Image>();

            _readyIndicator = new GameObject("ReadyIndicator");
            _readyIndicator.transform.SetParent(_uiGameObject.transform);

            // Create UI component
            _cooldownUI = _uiGameObject.AddComponent<SpecialMoveCooldownUI>();

            // Set references via reflection
            SetPrivateField(_cooldownUI, "iconImage", _iconImage);
            SetPrivateField(_cooldownUI, "cooldownFillOverlay", _cooldownOverlay);
            SetPrivateField(_cooldownUI, "readyIndicator", _readyIndicator);
            SetPrivateField(_cooldownUI, "specialMoves", _specialMoves);
            SetPrivateField(_cooldownUI, "characterStamina", _characterStamina);
            SetPrivateField(_cooldownUI, "showCountdownTimer", false);

            // Trigger lifecycle
            _cooldownUI.SendMessage("Awake");
            _cooldownUI.SendMessage("Start");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testCharacter);
            Object.DestroyImmediate(_uiGameObject);
            Object.DestroyImmediate(_testMoveData);
            Object.DestroyImmediate(_testStaminaData);
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        [UnityTest]
        public IEnumerator CooldownOverlayFillsOnUse()
        {
            // Initially should be ready (fill = 0)
            yield return null;
            _cooldownUI.SendMessage("Update");
            Assert.AreEqual(0f, _cooldownOverlay.fillAmount, 0.01f, "Overlay should be empty when ready");

            // Use special move (simulate via event)
            var onUsedEvent = typeof(CharacterSpecialMoves).GetField("OnSpecialMoveUsed",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            onUsedEvent?.GetValue(_specialMoves)?.GetType().GetMethod("Invoke")?.Invoke(
                onUsedEvent?.GetValue(_specialMoves),
                new object[] { _testMoveData });

            yield return null;

            // Overlay should be full
            Assert.AreEqual(1f, _cooldownOverlay.fillAmount, 0.01f, "Overlay should be full on use");
        }

        [UnityTest]
        public IEnumerator ReadyIndicatorAppearsWhenReady()
        {
            // Initially ready
            yield return new WaitForSeconds(0.1f);
            Assert.IsTrue(_readyIndicator.activeSelf, "Ready indicator should be active initially");

            // Use special move
            var onUsedEvent = typeof(CharacterSpecialMoves).GetField("OnSpecialMoveUsed",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            onUsedEvent?.GetValue(_specialMoves)?.GetType().GetMethod("Invoke")?.Invoke(
                onUsedEvent?.GetValue(_specialMoves),
                new object[] { _testMoveData });

            yield return null;

            // Ready indicator should hide
            Assert.IsFalse(_readyIndicator.activeSelf, "Ready indicator should hide when on cooldown");
        }

        [UnityTest]
        public IEnumerator IconDimsWhenInsufficientStamina()
        {
            // Start with full stamina - icon should be bright
            yield return new WaitForSeconds(0.1f);
            Color initialColor = _iconImage.color;

            // Deplete stamina below special move cost
            var consumeMethod = typeof(CharacterStamina).GetMethod("ConsumeStamina",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            consumeMethod?.Invoke(_characterStamina, new object[] { 95f });

            yield return null;
            _cooldownUI.SendMessage("Update");
            yield return null;

            // Icon should dim (color should change)
            Assert.AreNotEqual(initialColor, _iconImage.color, "Icon color should change when insufficient stamina");
        }

        [UnityTest]
        public IEnumerator SetSpecialMovesUpdatesReference()
        {
            // Create new components
            var newCharacter = new GameObject("NewCharacter");
            var newMoves = newCharacter.AddComponent<CharacterSpecialMoves>();
            var newStamina = newCharacter.AddComponent<CharacterStamina>();

            var moveDataField = typeof(CharacterSpecialMoves).GetField("specialMoveData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            moveDataField?.SetValue(newMoves, _testMoveData);

            var staminaDataField = typeof(CharacterStamina).GetField("staminaData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            staminaDataField?.SetValue(newStamina, _testStaminaData);

            newStamina.Initialize();
            newMoves.Initialize();

            // Set new references
            _cooldownUI.SetSpecialMoves(newMoves, newStamina);

            yield return null;

            // Trigger event on new moves
            var onUsedEvent = typeof(CharacterSpecialMoves).GetField("OnSpecialMoveUsed",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            onUsedEvent?.GetValue(newMoves)?.GetType().GetMethod("Invoke")?.Invoke(
                onUsedEvent?.GetValue(newMoves),
                new object[] { _testMoveData });

            yield return null;

            // UI should respond to new reference
            Assert.AreEqual(1f, _cooldownOverlay.fillAmount, 0.01f, "UI should respond to new special moves");

            Object.DestroyImmediate(newCharacter);
        }
    }
}
