using NUnit.Framework;
using UnityEngine;
using Knockout.Characters.Data;

namespace Knockout.Tests.EditMode.Combos
{
    /// <summary>
    /// EditMode tests for ComboChainData ScriptableObject.
    /// Tests chain timing, damage scaling, and validation logic.
    /// </summary>
    public class ComboChainDataTests
    {
        [Test]
        public void ComboChainData_Creation_Succeeds()
        {
            // Act
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Assert
            Assert.IsNotNull(data);
        }

        [Test]
        public void ComboChainData_GetChainWindow_ReturnsCorrectFramesForJab()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act
            int jabWindow = data.GetChainWindow(0); // 0 = Jab

            // Assert
            Assert.AreEqual(data.JabChainWindowFrames, jabWindow);
            Assert.Greater(jabWindow, 0, "Jab chain window must be positive");
        }

        [Test]
        public void ComboChainData_GetChainWindow_ReturnsCorrectFramesForHook()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act
            int hookWindow = data.GetChainWindow(1); // 1 = Hook

            // Assert
            Assert.AreEqual(data.HookChainWindowFrames, hookWindow);
            Assert.Greater(hookWindow, 0, "Hook chain window must be positive");
        }

        [Test]
        public void ComboChainData_GetChainWindow_ReturnsCorrectFramesForUppercut()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act
            int uppercutWindow = data.GetChainWindow(2); // 2 = Uppercut

            // Assert
            Assert.AreEqual(data.UppercutChainWindowFrames, uppercutWindow);
            Assert.Greater(uppercutWindow, 0, "Uppercut chain window must be positive");
        }

        [Test]
        public void ComboChainData_GetChainWindow_InvalidIndex_ReturnsDefaultJabWindow()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act
            int invalidWindow = data.GetChainWindow(99); // Invalid index

            // Assert
            Assert.AreEqual(data.JabChainWindowFrames, invalidWindow, "Invalid index should return Jab window as default");
        }

        [Test]
        public void ComboChainData_GetDamageScale_FirstHit_Returns100Percent()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act
            float firstHitScale = data.GetDamageScale(1); // 1st hit

            // Assert
            Assert.AreEqual(1.0f, firstHitScale, 0.001f, "First hit should deal 100% damage");
        }

        [Test]
        public void ComboChainData_GetDamageScale_SecondHit_Returns75Percent()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act
            float secondHitScale = data.GetDamageScale(2); // 2nd hit

            // Assert
            Assert.AreEqual(0.75f, secondHitScale, 0.001f, "Second hit should deal 75% damage");
        }

        [Test]
        public void ComboChainData_GetDamageScale_ThirdHit_Returns50Percent()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act
            float thirdHitScale = data.GetDamageScale(3); // 3rd hit

            // Assert
            Assert.AreEqual(0.5f, thirdHitScale, 0.001f, "Third hit should deal 50% damage");
        }

        [Test]
        public void ComboChainData_GetDamageScale_FourthAndBeyond_Returns50PercentFloor()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act
            float fourthHitScale = data.GetDamageScale(4);
            float fifthHitScale = data.GetDamageScale(5);
            float tenthHitScale = data.GetDamageScale(10);

            // Assert
            Assert.AreEqual(0.5f, fourthHitScale, 0.001f, "Fourth hit should be at floor (50%)");
            Assert.AreEqual(0.5f, fifthHitScale, 0.001f, "Fifth hit should be at floor (50%)");
            Assert.AreEqual(0.5f, tenthHitScale, 0.001f, "Tenth hit should be at floor (50%)");
        }

        [Test]
        public void ComboChainData_GetDamageScale_InvalidHitNumber_ReturnsDefault()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act
            float invalidHitScale = data.GetDamageScale(0); // Invalid (must be >= 1)

            // Assert
            Assert.AreEqual(1.0f, invalidHitScale, 0.001f, "Invalid hit number should return 1.0");
        }

        [Test]
        public void ComboChainData_DamageScaling_IsDescending()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act & Assert - Each value should be <= previous
            for (int i = 1; i < 4; i++)
            {
                float currentScale = data.GetDamageScale(i);
                float nextScale = data.GetDamageScale(i + 1);
                Assert.LessOrEqual(nextScale, currentScale, $"Damage scaling should be descending: hit {i + 1} ({nextScale}) should be <= hit {i} ({currentScale})");
            }
        }

        [Test]
        public void ComboChainData_GlobalComboTimeout_IsPositive()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act & Assert
            Assert.Greater(data.GlobalComboTimeoutFrames, 0, "Global combo timeout must be positive");
        }

        [Test]
        public void ComboChainData_ComboBreakWindow_IsPositive()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act & Assert
            Assert.Greater(data.ComboBreakWindowFrames, 0, "Combo break window must be positive");
        }

        [Test]
        public void ComboChainData_CounterWindowDamageBonus_InValidRange()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboChainData>();

            // Act & Assert
            Assert.GreaterOrEqual(data.CounterWindowDamageBonus, 1.0f, "Counter window bonus must be >= 1.0");
            Assert.LessOrEqual(data.CounterWindowDamageBonus, 2.0f, "Counter window bonus must be <= 2.0");
        }
    }
}
