using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Knockout.Characters.Data;

#if UNITY_EDITOR
namespace Knockout.Editor
{
    /// <summary>
    /// Editor script to automatically generate Phase 1 Unity assets.
    /// Run this via Tools > Knockout > Generate Phase 1 Assets menu item.
    /// </summary>
    public static class Phase1AssetGenerator
    {
        [MenuItem("Tools/Knockout/Generate Phase 1 Assets")]
        public static void GenerateAllPhase1Assets()
        {
            Debug.Log("[Phase1AssetGenerator] Starting Phase 1 asset generation...");

            CreateScriptableObjectAssets();
            CreateGameplayTestScene();
            CreateCharacterPrefabs();

            Debug.Log("[Phase1AssetGenerator] Phase 1 asset generation complete!");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Knockout/Generate ScriptableObject Assets")]
        public static void CreateScriptableObjectAssets()
        {
            Debug.Log("[Phase1AssetGenerator] Creating ScriptableObject assets...");

            string dataPath = "Assets/Knockout/Scripts/Characters/Data/";

            // Create BaseCharacterStats
            CharacterStats baseStats = ScriptableObject.CreateInstance<CharacterStats>();
            // Note: Default values are set in CharacterStats class
            AssetDatabase.CreateAsset(baseStats, dataPath + "BaseCharacterStats.asset");
            Debug.Log($"Created: {dataPath}BaseCharacterStats.asset");

            // Create AttackData_Jab
            AttackData jab = ScriptableObject.CreateInstance<AttackData>();
            AssetDatabase.CreateAsset(jab, dataPath + "AttackData_Jab.asset");

            // Set values using SerializedObject to modify private fields
            SerializedObject jabSO = new SerializedObject(jab);
            jabSO.FindProperty("attackName").stringValue = "Jab";
            jabSO.FindProperty("damage").floatValue = 10f;
            jabSO.FindProperty("knockback").floatValue = 0.5f;
            jabSO.FindProperty("startupFrames").intValue = 6;
            jabSO.FindProperty("activeFrames").intValue = 3;
            jabSO.FindProperty("recoveryFrames").intValue = 6;
            jabSO.FindProperty("animationTrigger").stringValue = "AttackTrigger";
            jabSO.FindProperty("attackTypeIndex").intValue = 0;
            jabSO.ApplyModifiedProperties();
            Debug.Log($"Created: {dataPath}AttackData_Jab.asset");

            // Create AttackData_Hook
            AttackData hook = ScriptableObject.CreateInstance<AttackData>();
            AssetDatabase.CreateAsset(hook, dataPath + "AttackData_Hook.asset");

            SerializedObject hookSO = new SerializedObject(hook);
            hookSO.FindProperty("attackName").stringValue = "Hook";
            hookSO.FindProperty("damage").floatValue = 18f;
            hookSO.FindProperty("knockback").floatValue = 1.5f;
            hookSO.FindProperty("startupFrames").intValue = 10;
            hookSO.FindProperty("activeFrames").intValue = 4;
            hookSO.FindProperty("recoveryFrames").intValue = 12;
            hookSO.FindProperty("animationTrigger").stringValue = "AttackTrigger";
            hookSO.FindProperty("attackTypeIndex").intValue = 1;
            hookSO.ApplyModifiedProperties();
            Debug.Log($"Created: {dataPath}AttackData_Hook.asset");

            // Create AttackData_Uppercut
            AttackData uppercut = ScriptableObject.CreateInstance<AttackData>();
            AssetDatabase.CreateAsset(uppercut, dataPath + "AttackData_Uppercut.asset");

            SerializedObject uppercutSO = new SerializedObject(uppercut);
            uppercutSO.FindProperty("attackName").stringValue = "Uppercut";
            uppercutSO.FindProperty("damage").floatValue = 30f;
            uppercutSO.FindProperty("knockback").floatValue = 3.0f;
            uppercutSO.FindProperty("startupFrames").intValue = 15;
            uppercutSO.FindProperty("activeFrames").intValue = 5;
            uppercutSO.FindProperty("recoveryFrames").intValue = 18;
            uppercutSO.FindProperty("animationTrigger").stringValue = "AttackTrigger";
            uppercutSO.FindProperty("attackTypeIndex").intValue = 2;
            uppercutSO.ApplyModifiedProperties();
            Debug.Log($"Created: {dataPath}AttackData_Uppercut.asset");

            EditorUtility.SetDirty(baseStats);
            EditorUtility.SetDirty(jab);
            EditorUtility.SetDirty(hook);
            EditorUtility.SetDirty(uppercut);

            Debug.Log("[Phase1AssetGenerator] ScriptableObject assets created successfully!");
        }

        [MenuItem("Tools/Knockout/Generate GameplayTest Scene")]
        public static void CreateGameplayTestScene()
        {
            Debug.Log("[Phase1AssetGenerator] Creating GameplayTest scene...");

            // Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create organizational parent GameObjects
            GameObject environment = new GameObject("Environment");
            GameObject lighting = new GameObject("Lighting");
            GameObject cameras = new GameObject("Cameras");
            GameObject spawnPoints = new GameObject("SpawnPoints");

            // Create Ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(5, 1, 5);
            ground.transform.SetParent(environment.transform);

            // Create Walls
            CreateWall("Wall_North", new Vector3(0, 2.5f, 25), new Vector3(50, 5, 1), environment.transform);
            CreateWall("Wall_South", new Vector3(0, 2.5f, -25), new Vector3(50, 5, 1), environment.transform);
            CreateWall("Wall_East", new Vector3(25, 2.5f, 0), new Vector3(1, 5, 50), environment.transform);
            CreateWall("Wall_West", new Vector3(-25, 2.5f, 0), new Vector3(1, 5, 50), environment.transform);

            // Configure Directional Light (should already exist)
            Light[] lights = Object.FindObjectsOfType<Light>();
            if (lights.Length > 0)
            {
                lights[0].name = "MainLight";
                lights[0].transform.rotation = Quaternion.Euler(50, -30, 0);
                lights[0].intensity = 1.0f;
                lights[0].transform.SetParent(lighting.transform);
            }

            // Create Fill Light
            GameObject fillLightGO = new GameObject("FillLight");
            Light fillLight = fillLightGO.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.transform.rotation = Quaternion.Euler(-20, 120, 0);
            fillLight.intensity = 0.4f;
            fillLight.color = new Color(0.9f, 0.95f, 1.0f);
            fillLightGO.transform.SetParent(lighting.transform);

            // Note: Cinemachine setup requires Cinemachine package
            // For now, we'll keep the Main Camera and document Cinemachine setup
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0, 3, -10);
                mainCam.transform.LookAt(Vector3.up);
                mainCam.transform.SetParent(cameras.transform);
            }

            // Create Spawn Points
            GameObject playerSpawn = new GameObject("PlayerSpawnPoint");
            playerSpawn.transform.position = new Vector3(-5, 0, 0);
            playerSpawn.transform.rotation = Quaternion.Euler(0, 90, 0);
            playerSpawn.transform.SetParent(spawnPoints.transform);

            GameObject aiSpawn = new GameObject("AISpawnPoint");
            aiSpawn.transform.position = new Vector3(5, 0, 0);
            aiSpawn.transform.rotation = Quaternion.Euler(0, -90, 0);
            aiSpawn.transform.SetParent(spawnPoints.transform);

            // Save scene
            string scenePath = "Assets/Knockout/Scenes/GameplayTest.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[Phase1AssetGenerator] Scene created: {scenePath}");

            Debug.Log("[Phase1AssetGenerator] GameplayTest scene created successfully!");
        }

        [MenuItem("Tools/Knockout/Generate Character Prefabs")]
        public static void CreateCharacterPrefabs()
        {
            Debug.Log("[Phase1AssetGenerator] Creating character prefabs...");

            string modelPath = "Assets/Knockout/Models/Characters/BaseCharacter/ElizabethWarren.fbx";
            string prefabPath = "Assets/Knockout/Prefabs/Characters/";

            // Load the character model
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                Debug.LogError($"[Phase1AssetGenerator] Could not find character model at {modelPath}");
                return;
            }

            // Create PlayerCharacter prefab
            GameObject playerChar = Object.Instantiate(model);
            playerChar.name = "PlayerCharacter";

            // Add Rigidbody
            Rigidbody rb = playerChar.AddComponent<Rigidbody>();
            rb.mass = 70;
            rb.drag = 0;
            rb.angularDrag = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Add Capsule Collider
            CapsuleCollider capsule = playerChar.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0, 1, 0);
            capsule.radius = 0.3f;
            capsule.height = 1.8f;

            // Add CharacterController component
            CharacterController controller = playerChar.AddComponent<CharacterController>();

            // Assign CharacterStats
            CharacterStats stats = AssetDatabase.LoadAssetAtPath<CharacterStats>("Assets/Knockout/Scripts/Characters/Data/BaseCharacterStats.asset");
            if (stats != null)
            {
                SerializedObject controllerSO = new SerializedObject(controller);
                controllerSO.FindProperty("characterStats").objectReferenceValue = stats;
                controllerSO.ApplyModifiedProperties();
            }

            // Add child GameObjects for hitboxes and hurtboxes
            GameObject hitboxes = new GameObject("Hitboxes");
            hitboxes.transform.SetParent(playerChar.transform);
            hitboxes.transform.localPosition = Vector3.zero;

            GameObject hurtboxes = new GameObject("Hurtboxes");
            hurtboxes.transform.SetParent(playerChar.transform);
            hurtboxes.transform.localPosition = Vector3.zero;

            // Save as prefab
            string playerPrefabPath = prefabPath + "PlayerCharacter.prefab";
            PrefabUtility.SaveAsPrefabAsset(playerChar, playerPrefabPath);
            Debug.Log($"[Phase1AssetGenerator] Created: {playerPrefabPath}");

            // Clean up instance
            Object.DestroyImmediate(playerChar);

            // Create AICharacter as prefab variant
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
            string aiPrefabPath = prefabPath + "AICharacter.prefab";

            // Create variant
            GameObject aiPrefab = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            aiPrefab.name = "AICharacter";

            // Save as variant
            PrefabUtility.SaveAsPrefabAsset(aiPrefab, aiPrefabPath);
            Debug.Log($"[Phase1AssetGenerator] Created: {aiPrefabPath}");

            // Clean up
            Object.DestroyImmediate(aiPrefab);

            Debug.Log("[Phase1AssetGenerator] Character prefabs created successfully!");
        }

        private static void CreateWall(string name, Vector3 position, Vector3 scale, Transform parent)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.transform.SetParent(parent);
        }
    }
}
#endif
