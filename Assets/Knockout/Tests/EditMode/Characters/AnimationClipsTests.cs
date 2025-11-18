using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Knockout.Tests.EditMode.Characters
{
    /// <summary>
    /// Tests for boxing animation clips import and configuration.
    /// </summary>
    public class AnimationClipsTests
    {
        private const string AnimationClipsPath = "Assets/Knockout/Animations/Characters/BaseCharacter/AnimationClips/";

        [Test]
        public void AnimationClips_Exist_ForRequiredAnimations()
        {
            // Arrange
            string[] requiredClips = new string[]
            {
                "Idle",
                "Left_Jab",
                "left_hook",
                "Right_hook",
                "left_uppercut",
                "Right_uppercut",
                "Block",
                "hit_by_jab_V1",
                "hit_by_hook_V1",
                "knockout_V1"
            };

            // Act & Assert
            foreach (string clipName in requiredClips)
            {
                AnimationClip clip = FindAnimationClip(clipName);
                Assert.IsNotNull(clip, $"Animation clip '{clipName}' should exist");
            }
        }

        [Test]
        public void AnimationClips_AllFilesImported()
        {
            // Arrange
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { AnimationClipsPath });

            // Act
            int clipCount = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith(AnimationClipsPath))
                {
                    clipCount++;
                }
            }

            // Assert
            Assert.GreaterOrEqual(clipCount, 10, "Should have at least 10 animation clips imported");
        }

        [Test]
        public void AttackAnimations_ExistForAllPunchTypes()
        {
            // Arrange & Act
            AnimationClip leftJab = FindAnimationClip("Left_Jab");
            AnimationClip leftHook = FindAnimationClip("left_hook");
            AnimationClip rightHook = FindAnimationClip("Right_hook");
            AnimationClip leftUppercut = FindAnimationClip("left_uppercut");
            AnimationClip rightUppercut = FindAnimationClip("Right_uppercut");

            // Assert
            Assert.IsNotNull(leftJab, "Left jab animation should exist");
            Assert.IsNotNull(leftHook, "Left hook animation should exist");
            Assert.IsNotNull(rightHook, "Right hook animation should exist");
            Assert.IsNotNull(leftUppercut, "Left uppercut animation should exist");
            Assert.IsNotNull(rightUppercut, "Right uppercut animation should exist");
        }

        [Test]
        public void DefensiveAnimations_ExistForBlockAndDodge()
        {
            // Arrange & Act
            AnimationClip block = FindAnimationClip("Block");
            AnimationClip leftDodge = FindAnimationClip("left_dodge");
            AnimationClip rightDodge = FindAnimationClip("right_dodge");

            // Assert
            Assert.IsNotNull(block, "Block animation should exist");
            Assert.IsNotNull(leftDodge, "Left dodge animation should exist");
            Assert.IsNotNull(rightDodge, "Right dodge animation should exist");
        }

        [Test]
        public void HitReactionAnimations_ExistForDifferentHitTypes()
        {
            // Arrange & Act
            AnimationClip hitByJab = FindAnimationClip("hit_by_jab_V1");
            AnimationClip hitByHook = FindAnimationClip("hit_by_hook_V1");
            AnimationClip knockout = FindAnimationClip("knockout_V1");

            // Assert
            Assert.IsNotNull(hitByJab, "Hit by jab animation should exist");
            Assert.IsNotNull(hitByHook, "Hit by hook animation should exist");
            Assert.IsNotNull(knockout, "Knockout animation should exist");
        }

        /// <summary>
        /// Helper method to find an animation clip by name in the animation clips folder.
        /// </summary>
        private AnimationClip FindAnimationClip(string clipName)
        {
            string[] guids = AssetDatabase.FindAssets($"{clipName} t:AnimationClip", new[] { AnimationClipsPath });
            if (guids.Length == 0) return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        }
    }
}
