using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Knockout.Characters.Data;

namespace Knockout.Tests.EditMode.Characters
{
    /// <summary>
    /// Tests for CharacterStats and AttackData ScriptableObjects.
    /// </summary>
    public class CharacterDataTests
    {
        [Test]
        public void CharacterStats_CanBeCreated()
        {
            // Arrange & Act
            CharacterStats stats = ScriptableObject.CreateInstance<CharacterStats>();

            // Assert
            Assert.IsNotNull(stats, "CharacterStats should be creatable");
            Assert.AreEqual(100f, stats.MaxHealth, "Default MaxHealth should be 100");
            Assert.AreEqual(5f, stats.MoveSpeed, "Default MoveSpeed should be 5");
            Assert.AreEqual(10f, stats.RotationSpeed, "Default RotationSpeed should be 10");
            Assert.AreEqual(1f, stats.DamageMultiplier, "Default DamageMultiplier should be 1");
            Assert.AreEqual(1f, stats.DamageTakenMultiplier, "Default DamageTakenMultiplier should be 1");

            // Cleanup
            Object.DestroyImmediate(stats);
        }

        [Test]
        public void AttackData_CanBeCreated()
        {
            // Arrange & Act
            AttackData attack = ScriptableObject.CreateInstance<AttackData>();

            // Assert
            Assert.IsNotNull(attack, "AttackData should be creatable");
            Assert.AreEqual("Attack", attack.AttackName, "Default AttackName should be 'Attack'");
            Assert.AreEqual(10f, attack.Damage, "Default Damage should be 10");
            Assert.AreEqual(0.5f, attack.Knockback, "Default Knockback should be 0.5");

            // Cleanup
            Object.DestroyImmediate(attack);
        }

        [Test]
        public void AttackData_CalculatesTotalFrames()
        {
            // Arrange
            AttackData attack = ScriptableObject.CreateInstance<AttackData>();

            // Act
            int totalFrames = attack.TotalFrames;

            // Assert
            // Default: 6 startup + 3 active + 6 recovery = 15 frames
            Assert.AreEqual(15, totalFrames, "TotalFrames should sum startup + active + recovery");

            // Cleanup
            Object.DestroyImmediate(attack);
        }

        [Test]
        public void AttackData_CalculatesTotalDuration()
        {
            // Arrange
            AttackData attack = ScriptableObject.CreateInstance<AttackData>();

            // Act
            float totalDuration = attack.TotalDuration;

            // Assert
            // 15 frames at 60fps = 0.25 seconds
            Assert.AreEqual(0.25f, totalDuration, 0.01f, "TotalDuration should be TotalFrames / 60");

            // Cleanup
            Object.DestroyImmediate(attack);
        }

        [Test]
        public void BaseCharacterStats_Asset_Exists()
        {
            // Run Tools > Knockout > Generate ScriptableObject Assets if this test fails

            // Arrange
            string assetPath = "Assets/Knockout/Scripts/Characters/Data/BaseCharacterStats.asset";

            // Act
            CharacterStats stats = AssetDatabase.LoadAssetAtPath<CharacterStats>(assetPath);

            // Assert
            Assert.IsNotNull(stats, "BaseCharacterStats asset should exist. Run Tools > Knockout > Generate ScriptableObject Assets if missing.");
            Assert.AreEqual(100f, stats.MaxHealth, "MaxHealth should be 100");
            Assert.AreEqual(5f, stats.MoveSpeed, "MoveSpeed should be 5");
        }

        [Test]
        public void AttackData_Jab_Asset_Exists()
        {
            // Run Tools > Knockout > Generate ScriptableObject Assets if this test fails

            // Arrange
            string assetPath = "Assets/Knockout/Scripts/Characters/Data/AttackData_Jab.asset";

            // Act
            AttackData attack = AssetDatabase.LoadAssetAtPath<AttackData>(assetPath);

            // Assert
            Assert.IsNotNull(attack, "AttackData_Jab asset should exist. Run Tools > Knockout > Generate ScriptableObject Assets if missing.");
            Assert.AreEqual("Jab", attack.AttackName);
            Assert.AreEqual(10f, attack.Damage);
            Assert.AreEqual(15, attack.TotalFrames, "Total frames should be 6+3+6=15");
        }
    }
}
