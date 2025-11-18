using NUnit.Framework;
using UnityEngine;
using Knockout.Combat.HitDetection;

namespace Knockout.Tests.EditMode.Combat
{
    /// <summary>
    /// Edit mode tests for HitData struct.
    /// </summary>
    public class HitDataTests
    {
        [Test]
        public void HitData_Constructor_SetsAllFields()
        {
            // Arrange
            GameObject attacker = new GameObject("Attacker");
            Vector3 hitPoint = new Vector3(1, 2, 3);
            Vector3 hitDirection = Vector3.forward;

            // Act
            HitData hitData = new HitData(
                attacker: attacker,
                damage: 20f,
                knockback: 1.5f,
                hitPoint: hitPoint,
                hitDirection: hitDirection,
                hitType: 1,
                attackName: "TestAttack"
            );

            // Assert
            Assert.AreEqual(attacker, hitData.Attacker);
            Assert.AreEqual(20f, hitData.Damage);
            Assert.AreEqual(1.5f, hitData.Knockback);
            Assert.AreEqual(hitPoint, hitData.HitPoint);
            Assert.AreEqual(hitDirection, hitData.HitDirection);
            Assert.AreEqual(1, hitData.HitType);
            Assert.AreEqual("TestAttack", hitData.AttackName);

            // Cleanup
            Object.DestroyImmediate(attacker);
        }

        [Test]
        public void HitData_DefaultConstructor_InitializesFields()
        {
            // Arrange & Act
            HitData hitData = new HitData();

            // Assert (default values)
            Assert.IsNull(hitData.Attacker);
            Assert.AreEqual(0f, hitData.Damage);
            Assert.AreEqual(0f, hitData.Knockback);
            Assert.AreEqual(Vector3.zero, hitData.HitPoint);
            Assert.AreEqual(Vector3.zero, hitData.HitDirection);
            Assert.AreEqual(0, hitData.HitType);
            Assert.IsNull(hitData.AttackName);
        }

        [Test]
        public void HitData_CopyConstructor_CreatesIndependentCopy()
        {
            // Arrange
            GameObject attacker = new GameObject("Attacker");
            HitData original = new HitData(
                attacker: attacker,
                damage: 15f,
                knockback: 2f,
                hitPoint: Vector3.up,
                hitDirection: Vector3.right,
                hitType: 2,
                attackName: "Hook"
            );

            // Act
            HitData copy = original;
            copy.Damage = 30f; // Modify copy (note: structs are value types, so this creates a new instance)

            // Assert
            Assert.AreEqual(15f, original.Damage); // Original unchanged (struct behavior)
            Assert.AreEqual(30f, copy.Damage);

            // Cleanup
            Object.DestroyImmediate(attacker);
        }
    }
}
