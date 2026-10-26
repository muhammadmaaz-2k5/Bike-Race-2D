using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DirtDash.Bike;
using DirtDash.Cameras;
using DirtDash.Core;

namespace DirtDash.Editor
{
    /// <summary>
    /// Automated setup script for Phase 1.
    /// Programmatically constructs the Bike_DirtRunner prefab using imported biker assets,
    /// sets up 2D physics joints, colliders, and generates the TestTrack scene with flat and ramp sections.
    /// </summary>
    [InitializeOnLoad]
    public static class Phase1Setup
    {
        private const string BikeStatsPath = "Assets/_Game/ScriptableObjects/BikeStats/Bike_DirtRunner.asset";
        private const string WheelMaterialPath = "Assets/_Game/Art/WheelFriction.physicsMaterial2D";
        private const string TrackMaterialPath = "Assets/_Game/Art/TrackFriction.physicsMaterial2D";
        private const string BikePrefabPath = "Assets/_Game/Prefabs/Bikes/Bike_DirtRunner.prefab";
        private const string TestScenePath = "Assets/_Game/Scenes/TestTrack.unity";
        private const string SourceBikerPrefabPath = "Assets/Bikers/Bikers/models/prefabs/Bikers.prefab";

        static Phase1Setup()
        {
            // Auto-check once upon editor load
            EditorApplication.delayCall += CheckAndRunInitialSetup;
        }

        private static void CheckAndRunInitialSetup()
        {
            if (!File.Exists(BikePrefabPath) || !File.Exists(TestScenePath))
            {
                RunSetup();
            }
        }

        [MenuItem("DirtDash/Setup Phase 1 Bike & Test Scene")]
        public static void RunSetup()
        {
            Debug.Log("[Dirt Dash] Starting Phase 1 automated setup...");

            EnsurePhysicsMaterials();
            BikeStats stats = EnsureBikeStats();
            GameObject bikePrefab = BuildBikePrefab(stats);
            BuildTestScene(bikePrefab);

            Debug.Log("[Dirt Dash] Phase 1 setup completed successfully! TestTrack scene is ready.");
        }

        private static void EnsurePhysicsMaterials()
        {
            if (!File.Exists(WheelMaterialPath))
            {
                var mat = new PhysicsMaterial2D("WheelFriction") { friction = 1.35f, bounciness = 0.05f };
                AssetDatabase.CreateAsset(mat, WheelMaterialPath);
            }

            if (!File.Exists(TrackMaterialPath))
            {
                var mat = new PhysicsMaterial2D("TrackFriction") { friction = 1.0f, bounciness = 0.0f };
                AssetDatabase.CreateAsset(mat, TrackMaterialPath);
            }

            AssetDatabase.SaveAssets();
        }

        private static BikeStats EnsureBikeStats()
        {
            var stats = AssetDatabase.LoadAssetAtPath<BikeStats>(BikeStatsPath);
            if (stats == null)
            {
                stats = ScriptableObject.CreateInstance<BikeStats>();
                stats.bikeId = "bike_dirtrunner";
                stats.displayName = "Dirt Runner";
                stats.description = "A reliable, balanced starter bike with responsive handling.";
                stats.isDefaultUnlocked = true;
                stats.motorSpeed = 2400f;
                stats.motorTorque = 42f;
                stats.maxSpeed = 30f;
                stats.accelerationSmoothing = 8f;
                stats.brakeTorque = 38f;
                stats.airRotationTorque = 28f;
                stats.groundLeanTorque = 12f;
                stats.bodyMass = 1.5f;
                stats.wheelMass = 0.45f;
                stats.suspensionFrequency = 8f;
                stats.suspensionDamping = 0.7f;
                AssetDatabase.CreateAsset(stats, BikeStatsPath);
                AssetDatabase.SaveAssets();
            }
            return stats;
        }

        private static GameObject BuildBikePrefab(BikeStats stats)
        {
            var wheelMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(WheelMaterialPath);

            // 1. Root Bike Object
            GameObject root = new GameObject("Bike_DirtRunner");
            root.tag = GameConstants.TagPlayer;

            // Rigidbody2D on Chassis
            Rigidbody2D bodyRb = root.AddComponent<Rigidbody2D>();
            bodyRb.mass = stats.bodyMass;
            bodyRb.linearDamping = stats.linearDrag;
            bodyRb.angularDamping = stats.angularDrag;
            bodyRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            bodyRb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Box Collider for the frame
            BoxCollider2D frameCollider = root.AddComponent<BoxCollider2D>();
            frameCollider.size = new Vector2(1.3f, 0.45f);
            frameCollider.offset = new Vector2(0f, 0.15f);

            // 2. Front Wheel
            GameObject frontWheelGo = new GameObject("FrontWheel");
            frontWheelGo.transform.SetParent(root.transform);
            frontWheelGo.transform.localPosition = new Vector3(0.75f, -0.32f, 0f);

            Rigidbody2D frontRb = frontWheelGo.AddComponent<Rigidbody2D>();
            frontRb.mass = stats.wheelMass;
            frontRb.interpolation = RigidbodyInterpolation2D.Interpolate;

            CircleCollider2D frontCol = frontWheelGo.AddComponent<CircleCollider2D>();
            frontCol.radius = 0.36f;
            frontCol.sharedMaterial = wheelMat;

            // 3. Back Wheel
            GameObject backWheelGo = new GameObject("BackWheel");
            backWheelGo.transform.SetParent(root.transform);
            backWheelGo.transform.localPosition = new Vector3(-0.75f, -0.32f, 0f);

            Rigidbody2D backRb = backWheelGo.AddComponent<Rigidbody2D>();
            backRb.mass = stats.wheelMass;
            backRb.interpolation = RigidbodyInterpolation2D.Interpolate;

            CircleCollider2D backCol = backWheelGo.AddComponent<CircleCollider2D>();
            backCol.radius = 0.36f;
            backCol.sharedMaterial = wheelMat;

            // 4. Wheel Joints on Root
            WheelJoint2D frontJoint = root.AddComponent<WheelJoint2D>();
            frontJoint.connectedBody = frontRb;
            frontJoint.anchor = new Vector2(0.75f, -0.32f);
            frontJoint.connectedAnchor = Vector2.zero;
            JointSuspension2D frontSusp = new JointSuspension2D
            {
                angle = 90f,
                frequency = stats.suspensionFrequency,
                dampingRatio = stats.suspensionDamping
            };
            frontJoint.suspension = frontSusp;
            frontJoint.useMotor = false;

            WheelJoint2D backJoint = root.AddComponent<WheelJoint2D>();
            backJoint.connectedBody = backRb;
            backJoint.anchor = new Vector2(-0.75f, -0.32f);
            backJoint.connectedAnchor = Vector2.zero;
            JointSuspension2D backSusp = new JointSuspension2D
            {
                angle = 90f,
                frequency = stats.suspensionFrequency,
                dampingRatio = stats.suspensionDamping
            };
            backJoint.suspension = backSusp;
            backJoint.motor = new JointMotor2D
            {
                motorSpeed = -stats.motorSpeed,
                maxMotorTorque = stats.motorTorque
            };
            backJoint.useMotor = false;

            // 5. Rider Head (Crash Trigger)
            GameObject headGo = new GameObject("RiderHead");
            headGo.transform.SetParent(root.transform);
            headGo.transform.localPosition = new Vector3(0.05f, 0.72f, 0f);

            CircleCollider2D headCol = headGo.AddComponent<CircleCollider2D>();
            headCol.radius = 0.24f;
            headCol.isTrigger = true;

            CrashDetector crashDetector = headGo.AddComponent<CrashDetector>();

            // 6. Flip Detector
            FlipDetector flipDetector = root.AddComponent<FlipDetector>();

            // 7. Visual Model Child (Instantiate 3D Biker Prefab)
            GameObject sourceBikerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourceBikerPrefabPath);
            Transform visualModelTransform = null;
            if (sourceBikerPrefab != null)
            {
                GameObject visualInstance = (GameObject)PrefabUtility.InstantiatePrefab(sourceBikerPrefab, root.transform);
                visualInstance.name = "BikerVisual";
                // Rotate 90 degrees around Y so the bike points forward along the 2D X-axis
                visualInstance.transform.localPosition = new Vector3(0.08f, -0.32f, 0f);
                visualInstance.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                visualInstance.transform.localScale = Vector3.one * 0.95f;
                visualModelTransform = visualInstance.transform;
            }

            // 8. BikeController Component
            BikeController controller = root.AddComponent<BikeController>();

            // Use SerializedObject to cleanly set private serialized fields
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("stats").objectReferenceValue = stats;
            so.FindProperty("bikeBody").objectReferenceValue = bodyRb;
            so.FindProperty("frontWheel").objectReferenceValue = frontRb;
            so.FindProperty("backWheel").objectReferenceValue = backRb;
            so.FindProperty("frontWheelJoint").objectReferenceValue = frontJoint;
            so.FindProperty("backWheelJoint").objectReferenceValue = backJoint;
            so.FindProperty("frontWheelCollider").objectReferenceValue = frontCol;
            so.FindProperty("backWheelCollider").objectReferenceValue = backCol;
            if (visualModelTransform != null)
            {
                so.FindProperty("visualBody").objectReferenceValue = visualModelTransform;
            }
            so.ApplyModifiedProperties();

            // Link crash detector to bike controller
            SerializedObject crashSo = new SerializedObject(crashDetector);
            crashSo.FindProperty("bikeController").objectReferenceValue = controller;
            crashSo.FindProperty("headCollider").objectReferenceValue = headCol;
            crashSo.ApplyModifiedProperties();

            // Link flip detector
            SerializedObject flipSo = new SerializedObject(flipDetector);
            flipSo.FindProperty("bikeController").objectReferenceValue = controller;
            flipSo.ApplyModifiedProperties();

            // Save Prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, BikePrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            Debug.Log($"[Dirt Dash] Bike prefab created at {BikePrefabPath}");
            return savedPrefab;
        }

        private static void BuildTestScene(GameObject bikePrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            UnityEngine.Camera cam = camGo.AddComponent<UnityEngine.Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.16f, 0.22f, 1f); // Dark slate sky
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 100f;
            camGo.transform.position = new Vector3(0f, 2f, -10f);
            camGo.AddComponent<AudioListener>();

            CameraFollow camFollow = camGo.AddComponent<CameraFollow>();

            // 2. Lights
            GameObject lightGo = new GameObject("Directional Light");
            Light dirLight = lightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = Color.white;
            dirLight.intensity = 1.25f;
            lightGo.transform.rotation = Quaternion.Euler(40f, 45f, 0f);

            // 3. GameManager
            GameObject gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();

            // 4. Track Surface (Flat + Bump + Ramp)
            var trackMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(TrackMaterialPath);

            GameObject trackGo = new GameObject("TestTrack");
            EdgeCollider2D edgeCollider = trackGo.AddComponent<EdgeCollider2D>();
            edgeCollider.sharedMaterial = trackMat;

            Vector2[] points = new Vector2[]
            {
                new Vector2(-15f, 0f),
                new Vector2(20f, 0f),     // Flat launch zone
                new Vector2(30f, 0.8f),   // Small bump
                new Vector2(38f, 0f),     // Valley
                new Vector2(50f, 0f),     // Approach
                new Vector2(65f, 4.2f),   // Big ramp!
                new Vector2(72f, 0f),     // Landing slope
                new Vector2(130f, 0f)     // Flat runout
            };
            edgeCollider.points = points;

            // Visual line for the track
            LineRenderer line = trackGo.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                line.SetPosition(i, new Vector3(points[i].x, points[i].y, 0f));
            }
            line.startWidth = 0.25f;
            line.endWidth = 0.25f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = new Color(0.95f, 0.60f, 0.15f, 1f); // Amber / orange track outline
            line.endColor = new Color(0.95f, 0.60f, 0.15f, 1f);

            // 5. Spawn Bike
            if (bikePrefab != null)
            {
                GameObject bikeInstance = (GameObject)PrefabUtility.InstantiatePrefab(bikePrefab);
                bikeInstance.transform.position = new Vector3(0f, 1.2f, 0f);

                // Set camera target to bike body
                camFollow.SetTarget(bikeInstance.transform, bikeInstance.GetComponent<Rigidbody2D>());
            }

            // Save Scene
            EditorSceneManager.SaveScene(scene, TestScenePath);

            // Add to Build Settings
            AddSceneToBuildSettings(TestScenePath);

            Debug.Log($"[Dirt Dash] TestTrack scene saved at {TestScenePath}");
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }

            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            for (int i = 0; i < scenes.Length; i++)
            {
                newScenes[i] = scenes[i];
            }
            newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
