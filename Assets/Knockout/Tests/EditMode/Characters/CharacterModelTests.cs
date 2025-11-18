using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Knockout.Tests.EditMode.Characters
{
    /// <summary>
    /// Tests for character model configuration and import settings.
    /// </summary>
    public class CharacterModelTests
    {
        [Test]
        public void ElizabethWarrenModel_HasHumanoidAvatar()
        {
            // Arrange
            string modelPath = "Assets/Knockout/Models/Characters/BaseCharacter/ElizabethWarren.fbx";

            // Act
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

            // Assert
            Assert.IsNotNull(model, "Elizabeth Warren model should exist at path");

            Animator animator = model.GetComponent<Animator>();
            Assert.IsNotNull(animator, "Model should have Animator component");
            Assert.IsNotNull(animator.avatar, "Animator should have Avatar");
            Assert.IsTrue(animator.avatar.isHuman, "Avatar should be configured as Humanoid");
            Assert.IsTrue(animator.avatar.isValid, "Avatar should be valid");
        }

        [Test]
        public void ElizabethWarrenModel_TexturesExist()
        {
            // Arrange
            string texturePath = "Assets/Knockout/Models/Characters/BaseCharacter/Textures/";

            // Act & Assert
            string colorTexture = texturePath + "Elizabeth Warren color.png";
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Texture2D>(colorTexture),
                "Color texture should exist");

            string normalTexture = texturePath + "Elizabeth Warren normal.png";
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Texture2D>(normalTexture),
                "Normal texture should exist");

            string specTexture = texturePath + "Elizabeth Warren spec.png";
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Texture2D>(specTexture),
                "Specular texture should exist");
        }

        [Test]
        public void ElizabethWarrenModel_MaterialExists()
        {
            // Arrange
            string materialPath = "Assets/Knockout/Materials/Characters/phong1.mat";

            // Act
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            // Assert
            Assert.IsNotNull(material, "Character material should exist");
        }
    }
}
