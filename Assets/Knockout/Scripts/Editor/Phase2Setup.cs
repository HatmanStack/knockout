using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;
using Knockout.Characters.Data;

#if UNITY_EDITOR
namespace Knockout.Editor
{
    /// <summary>
    /// Editor utility to set up Phase 2 assets (prefabs, animator controller, avatar masks).
    /// Run via menu: Tools > Knockout > Phase 2 Setup
    /// </summary>
    public static class Phase2Setup
    {
        private const string MODEL_PATH = "Assets/Elizabeth Warren caricature/Mecanim Elizabeth Warren rigged 1.fbx";
        private const string PREFAB_DIR = "Assets/Knockout/Prefabs/Characters";
        private const string ANIMATOR_MASK_DIR = "Assets/Knockout/Animations/Characters/BaseCharacter/AnimatorMasks";
        private const string ANIMATOR_CONTROLLER_PATH = "Assets/Knockout/Animations/Characters/BaseCharacter/BaseCharacterAnimatorController.controller";
        private const string ANIMATION_CLIPS_DIR = "Assets/Knockout/Animations/Characters/BaseCharacter/AnimationClips";

        [MenuItem("Tools/Knockout/Generate Phase 2 Assets")]
        public static void GenerateAllPhase2Assets()
        {
            Debug.Log("[Phase2Setup] Starting Phase 2 asset generation...");

            CreateCharacterPrefabs();
            CreateAvatarMasks();
            CreateAnimatorController();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase2Setup] Phase 2 asset generation complete!");
        }

        [MenuItem("Tools/Knockout/Generate Character Prefabs (Phase 2)")]

        private static void CreateCharacterPrefabs()
        {
            Debug.Log("[Phase2Setup] Creating character prefabs...");

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(PREFAB_DIR))
            {
                Directory.CreateDirectory(PREFAB_DIR);
                AssetDatabase.Refresh();
            }

            // Load character model
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MODEL_PATH);
            if (modelPrefab == null)
            {
                Debug.LogError($"[Phase2Setup] Could not find character model at {MODEL_PATH}");
                return;
            }

            // Create PlayerCharacter prefab
            string playerPrefabPath = PREFAB_DIR + "/PlayerCharacter.prefab";
            if (!File.Exists(playerPrefabPath))
            {
                GameObject playerCharacter = CreateCharacterGameObject(modelPrefab, "PlayerCharacter");
                PrefabUtility.SaveAsPrefabAsset(playerCharacter, playerPrefabPath);
                Object.DestroyImmediate(playerCharacter);
                Debug.Log($"[Phase2Setup] Created PlayerCharacter prefab at {playerPrefabPath}");
            }
            else
            {
                Debug.Log("[Phase2Setup] PlayerCharacter prefab already exists");
            }

            // Create AICharacter prefab
            string aiPrefabPath = PREFAB_DIR + "/AICharacter.prefab";
            if (!File.Exists(aiPrefabPath))
            {
                GameObject aiCharacter = CreateCharacterGameObject(modelPrefab, "AICharacter");
                PrefabUtility.SaveAsPrefabAsset(aiCharacter, aiPrefabPath);
                Object.DestroyImmediate(aiCharacter);
                Debug.Log($"[Phase2Setup] Created AICharacter prefab at {aiPrefabPath}");
            }
            else
            {
                Debug.Log("[Phase2Setup] AICharacter prefab already exists");
            }

            AssetDatabase.Refresh();
            Debug.Log("[Phase2Setup] Character prefabs created successfully!");
        }

        private static GameObject CreateCharacterGameObject(GameObject modelPrefab, string name)
        {
            // Instantiate model
            GameObject character = Object.Instantiate(modelPrefab);
            character.name = name;

            // Add Rigidbody
            Rigidbody rb = character.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = character.AddComponent<Rigidbody>();
            }
            rb.mass = 70f;
            rb.drag = 0f;
            rb.angularDrag = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Add Capsule Collider
            CapsuleCollider capsule = character.GetComponent<CapsuleCollider>();
            if (capsule == null)
            {
                capsule = character.AddComponent<CapsuleCollider>();
            }
            capsule.center = new Vector3(0, 1, 0);
            capsule.radius = 0.3f;
            capsule.height = 1.8f;
            capsule.direction = 1; // Y-axis

            // Add CharacterController script
            var controller = character.GetComponent<Knockout.Characters.CharacterController>();
            if (controller == null)
            {
                character.AddComponent<Knockout.Characters.CharacterController>();
            }

            // Ensure Animator exists
            Animator animator = character.GetComponent<Animator>();
            if (animator == null)
            {
                animator = character.AddComponent<Animator>();
            }
            animator.applyRootMotion = true;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            // Create child containers
            CreateChildIfNotExists(character.transform, "Hitboxes");
            CreateChildIfNotExists(character.transform, "Hurtboxes");

            return character;
        }

        private static void CreateChildIfNotExists(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                GameObject childObj = new GameObject(name);
                childObj.transform.SetParent(parent);
                childObj.transform.localPosition = Vector3.zero;
            }
        }

        private static void CreateAvatarMasks()
        {
            Debug.Log("Creating avatar masks...");

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(ANIMATOR_MASK_DIR))
            {
                Directory.CreateDirectory(ANIMATOR_MASK_DIR);
                AssetDatabase.Refresh();
            }

            // Load character model to get avatar
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MODEL_PATH);
            if (modelPrefab == null)
            {
                Debug.LogError($"Could not find character model at {MODEL_PATH}");
                return;
            }

            Animator animator = modelPrefab.GetComponent<Animator>();
            if (animator == null || animator.avatar == null)
            {
                Debug.LogError("Model does not have a valid Avatar");
                return;
            }

            // Create Upper Body Mask
            string upperBodyMaskPath = Path.Combine(ANIMATOR_MASK_DIR, "UpperBodyMask.mask");
            if (!File.Exists(upperBodyMaskPath))
            {
                AvatarMask upperBodyMask = new AvatarMask();
                ConfigureUpperBodyMask(upperBodyMask);
                AssetDatabase.CreateAsset(upperBodyMask, upperBodyMaskPath);
                Debug.Log($"Created UpperBodyMask at {upperBodyMaskPath}");
            }
            else
            {
                Debug.Log("UpperBodyMask already exists");
            }

            // Create Full Body Mask
            string fullBodyMaskPath = Path.Combine(ANIMATOR_MASK_DIR, "FullBodyMask.mask");
            if (!File.Exists(fullBodyMaskPath))
            {
                AvatarMask fullBodyMask = new AvatarMask();
                ConfigureFullBodyMask(fullBodyMask);
                AssetDatabase.CreateAsset(fullBodyMask, fullBodyMaskPath);
                Debug.Log($"Created FullBodyMask at {fullBodyMaskPath}");
            }
            else
            {
                Debug.Log("FullBodyMask already exists");
            }

            AssetDatabase.Refresh();
            Debug.Log("Avatar masks created successfully!");
        }

        private static void ConfigureUpperBodyMask(AvatarMask mask)
        {
            // Enable upper body, disable legs
            mask.transformCount = 0; // Use humanoid body parts instead

            // Disable all first
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
            {
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
            }

            // Enable upper body parts
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
        }

        private static void ConfigureFullBodyMask(AvatarMask mask)
        {
            // Enable all body parts
            mask.transformCount = 0;

            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
            {
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, true);
            }
        }

        private static void CreateAnimatorController()
        {
            Debug.Log("[Phase2Setup] Creating Animator Controller...");

            string controllerDir = Path.GetDirectoryName(ANIMATOR_CONTROLLER_PATH);
            if (!AssetDatabase.IsValidFolder(controllerDir))
            {
                Directory.CreateDirectory(controllerDir);
                AssetDatabase.Refresh();
            }

            if (File.Exists(ANIMATOR_CONTROLLER_PATH))
            {
                Debug.Log("[Phase2Setup] Animator Controller already exists");
                return;
            }

            // Create new animator controller
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ANIMATOR_CONTROLLER_PATH);

            // Load avatar masks
            AvatarMask upperBodyMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(ANIMATOR_MASK_DIR + "/UpperBodyMask.mask");
            AvatarMask fullBodyMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(ANIMATOR_MASK_DIR + "/FullBodyMask.mask");

            // Create parameters
            CreateAnimatorParameters(controller);

            // Configure Base Layer (Locomotion)
            ConfigureLocomotionLayer(controller);

            // Add Upper Body Layer
            ConfigureUpperBodyLayer(controller, upperBodyMask);

            // Add Full Body Override Layer
            ConfigureFullBodyOverrideLayer(controller, fullBodyMask);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Phase2Setup] Created Animator Controller at {ANIMATOR_CONTROLLER_PATH}");
        }

        private static void CreateAnimatorParameters(AnimatorController controller)
        {
            // Locomotion parameters
            controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveDirectionX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveDirectionY", AnimatorControllerParameterType.Float);

            // Attack parameters
            controller.AddParameter("AttackTrigger", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackType", AnimatorControllerParameterType.Int);
            controller.AddParameter("IsBlocking", AnimatorControllerParameterType.Bool);
            controller.AddParameter("UpperBodyWeight", AnimatorControllerParameterType.Float);

            // Hit reaction parameters
            controller.AddParameter("HitReaction", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("HitType", AnimatorControllerParameterType.Int);
            controller.AddParameter("KnockedDown", AnimatorControllerParameterType.Bool);
            controller.AddParameter("KnockedOut", AnimatorControllerParameterType.Bool);
            controller.AddParameter("OverrideWeight", AnimatorControllerParameterType.Float);
        }

        private static void ConfigureLocomotionLayer(AnimatorController controller)
        {
            AnimatorControllerLayer baseLayer = controller.layers[0];
            baseLayer.name = "Locomotion";
            baseLayer.defaultWeight = 1f;

            AnimatorStateMachine stateMachine = baseLayer.stateMachine;

            // Load Idle animation
            AnimationClip idleClip = LoadAnimationClip("Idle");
            if (idleClip == null)
            {
                Debug.LogWarning("[Phase2Setup] Could not find Idle animation clip");
                return;
            }

            // Create Idle state
            AnimatorState idleState = stateMachine.AddState("Idle", new Vector3(300, 0, 0));
            idleState.motion = idleClip;
            stateMachine.defaultState = idleState;

            Debug.Log("[Phase2Setup] Configured Locomotion layer");
        }

        private static void ConfigureUpperBodyLayer(AnimatorController controller, AvatarMask mask)
        {
            AnimatorControllerLayer upperBodyLayer = new AnimatorControllerLayer
            {
                name = "UpperBody",
                defaultWeight = 1f,
                blendingMode = AnimatorLayerBlendingMode.Override,
                avatarMask = mask,
                stateMachine = new AnimatorStateMachine()
            };

            controller.AddLayer(upperBodyLayer);

            AnimatorStateMachine stateMachine = upperBodyLayer.stateMachine;

            // Create Empty state (default)
            AnimatorState emptyState = stateMachine.AddState("Empty", new Vector3(300, 0, 0));
            stateMachine.defaultState = emptyState;

            Debug.Log("[Phase2Setup] Configured UpperBody layer");
        }

        private static void ConfigureFullBodyOverrideLayer(AnimatorController controller, AvatarMask mask)
        {
            AnimatorControllerLayer overrideLayer = new AnimatorControllerLayer
            {
                name = "FullBodyOverride",
                defaultWeight = 0f,
                blendingMode = AnimatorLayerBlendingMode.Override,
                avatarMask = mask,
                stateMachine = new AnimatorStateMachine()
            };

            controller.AddLayer(overrideLayer);

            AnimatorStateMachine stateMachine = overrideLayer.stateMachine;

            // Create Empty state (default)
            AnimatorState emptyState = stateMachine.AddState("Empty", new Vector3(300, 0, 0));
            stateMachine.defaultState = emptyState;

            Debug.Log("[Phase2Setup] Configured FullBodyOverride layer");
        }

        private static AnimationClip LoadAnimationClip(string clipName)
        {
            string[] guids = AssetDatabase.FindAssets($"{clipName} t:AnimationClip", new[] { ANIMATION_CLIPS_DIR });

            if (guids.Length == 0)
            {
                Debug.LogWarning($"[Phase2Setup] Animation clip '{clipName}' not found in {ANIMATION_CLIPS_DIR}");
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);

            // FBX files contain multiple assets, need to load sub-assets
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip && asset.name == clipName)
                {
                    return clip;
                }
            }

            return null;
        }
    }
}
#endif
