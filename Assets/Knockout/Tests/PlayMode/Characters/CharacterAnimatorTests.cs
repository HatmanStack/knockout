using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using UnityEditor.Animations;
using Knockout.Characters.Components;

namespace Knockout.Tests.PlayMode.Characters
{
    /// <summary>
    /// PlayMode tests for CharacterAnimator component.
    /// Tests animator parameter setting and animation event callbacks.
    /// </summary>
    public class CharacterAnimatorTests
    {
        private const string ANIMATOR_CONTROLLER_PATH = "Assets/Knockout/Animations/Characters/BaseCharacter/BaseCharacterAnimatorController.controller";

        private GameObject _testCharacter;
        private Animator _animator;
        private CharacterAnimator _characterAnimator;

        [SetUp]
        public void SetUp()
        {
            // Create test character GameObject
            _testCharacter = new GameObject("TestCharacter");
            _animator = _testCharacter.AddComponent<Animator>();
            _characterAnimator = _testCharacter.AddComponent<CharacterAnimator>();

            // Load and assign animator controller
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ANIMATOR_CONTROLLER_PATH);

            if (controller != null)
            {
                _animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning($"[CharacterAnimatorTests] Animator controller not found at {ANIMATOR_CONTROLLER_PATH}. " +
                    "Run 'Tools > Knockout > Generate Phase 2 Assets' to create it.");
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test objects
            if (_testCharacter != null)
            {
                Object.DestroyImmediate(_testCharacter);
            }
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_SetMovement_UpdatesAnimatorParameters()
        {
            // Skip if animator controller not found
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            yield return null; // Wait one frame for initialization

            // Act
            _characterAnimator.SetMovement(new Vector2(0.5f, 1f), 0.8f);
            yield return null;

            // Assert
            Assert.AreEqual(0.5f, _animator.GetFloat("MoveDirectionX"), 0.01f, "MoveDirectionX should be set correctly");
            Assert.AreEqual(1f, _animator.GetFloat("MoveDirectionY"), 0.01f, "MoveDirectionY should be set correctly");
            Assert.AreEqual(0.8f, _animator.GetFloat("MoveSpeed"), 0.01f, "MoveSpeed should be set correctly");
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_TriggerAttack_SetsAnimatorParameters()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            yield return null;

            // Act
            _characterAnimator.TriggerJab();
            yield return null;

            // Assert
            Assert.AreEqual(0, _animator.GetInteger("AttackType"), "AttackType should be 0 for Jab");
            // Note: Trigger parameters are consumed immediately, can't assert them
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_TriggerHook_SetsCorrectAttackType()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            yield return null;

            // Act
            _characterAnimator.TriggerHook();
            yield return null;

            // Assert
            Assert.AreEqual(1, _animator.GetInteger("AttackType"), "AttackType should be 1 for Hook");
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_TriggerUppercut_SetsCorrectAttackType()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            yield return null;

            // Act
            _characterAnimator.TriggerUppercut();
            yield return null;

            // Assert
            Assert.AreEqual(2, _animator.GetInteger("AttackType"), "AttackType should be 2 for Uppercut");
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_SetBlocking_UpdatesAnimatorParameter()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            yield return null;

            // Act
            _characterAnimator.SetBlocking(true);
            yield return null;

            // Assert
            Assert.IsTrue(_animator.GetBool("IsBlocking"), "IsBlocking should be true");

            // Act
            _characterAnimator.SetBlocking(false);
            yield return null;

            // Assert
            Assert.IsFalse(_animator.GetBool("IsBlocking"), "IsBlocking should be false");
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_TriggerHitReaction_SetsParametersAndLayerWeight()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            yield return null;

            // Act
            _characterAnimator.TriggerHitReaction(1); // Medium hit
            yield return null;

            // Assert
            Assert.AreEqual(1, _animator.GetInteger("HitType"), "HitType should be 1 for medium hit");
            Assert.AreEqual(1.0f, _animator.GetLayerWeight(2), 0.01f, "Override layer weight should be 1.0");
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_TriggerKnockdown_SetsKnockedDownParameter()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            yield return null;

            // Act
            _characterAnimator.TriggerKnockdown();
            yield return null;

            // Assert
            Assert.IsTrue(_animator.GetBool("KnockedDown"), "KnockedDown should be true");
            Assert.AreEqual(1.0f, _animator.GetLayerWeight(2), 0.01f, "Override layer weight should be 1.0");
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_TriggerKnockout_SetsKnockedOutParameter()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            yield return null;

            // Act
            _characterAnimator.TriggerKnockout();
            yield return null;

            // Assert
            Assert.IsTrue(_animator.GetBool("KnockedOut"), "KnockedOut should be true");
            Assert.AreEqual(1.0f, _animator.GetLayerWeight(2), 0.01f, "Override layer weight should be 1.0");
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_SetUpperBodyLayerWeight_UpdatesLayerWeight()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            yield return null;

            // Act
            _characterAnimator.SetUpperBodyLayerWeight(0.7f);
            yield return null;

            // Assert
            Assert.AreEqual(0.7f, _animator.GetLayerWeight(1), 0.01f, "Upper body layer weight should be 0.7");
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_AnimationEvents_InvokeCorrectCallbacks()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            bool attackStartFired = false;
            bool hitboxActivateFired = false;
            bool hitboxDeactivateFired = false;
            bool attackEndFired = false;

            _characterAnimator.OnAttackStart += () => attackStartFired = true;
            _characterAnimator.OnHitboxActivate += () => hitboxActivateFired = true;
            _characterAnimator.OnHitboxDeactivate += () => hitboxDeactivateFired = true;
            _characterAnimator.OnAttackEnd += () => attackEndFired = true;

            yield return null;

            // Act - Manually invoke animation event methods
            _characterAnimator.AnimEvent_OnAttackStart();
            _characterAnimator.AnimEvent_OnHitboxActivate();
            _characterAnimator.AnimEvent_OnHitboxDeactivate();
            _characterAnimator.AnimEvent_OnAttackEnd();

            yield return null;

            // Assert
            Assert.IsTrue(attackStartFired, "OnAttackStart event should fire");
            Assert.IsTrue(hitboxActivateFired, "OnHitboxActivate event should fire");
            Assert.IsTrue(hitboxDeactivateFired, "OnHitboxDeactivate event should fire");
            Assert.IsTrue(attackEndFired, "OnAttackEnd event should fire");
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_AnimEventOnAttackEnd_ResetsUpperBodyWeight()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            yield return null;

            // Arrange - Set upper body weight to non-zero
            _characterAnimator.SetUpperBodyLayerWeight(1.0f);
            yield return null;
            Assert.AreEqual(1.0f, _animator.GetLayerWeight(1), 0.01f);

            // Act - Trigger attack end event
            _characterAnimator.AnimEvent_OnAttackEnd();
            yield return null;

            // Assert
            Assert.AreEqual(0f, _animator.GetLayerWeight(1), 0.01f, "Upper body weight should reset to 0");
        }

        [UnityTest]
        public IEnumerator CharacterAnimator_AnimEventOnHitReactionEnd_ResetsOverrideWeight()
        {
            if (_animator.runtimeAnimatorController == null)
            {
                Assert.Ignore("Animator controller not found. Run Phase 2 asset generator first.");
                yield break;
            }

            yield return null;

            // Arrange - Set override weight to non-zero
            _characterAnimator.SetOverrideLayerWeight(1.0f);
            yield return null;
            Assert.AreEqual(1.0f, _animator.GetLayerWeight(2), 0.01f);

            // Act - Trigger hit reaction end event
            _characterAnimator.AnimEvent_OnHitReactionEnd();
            yield return null;

            // Assert
            Assert.AreEqual(0f, _animator.GetLayerWeight(2), 0.01f, "Override weight should reset to 0");
        }
    }
}
