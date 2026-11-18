using System.IO;
using KingWorldWar.Art.Palettes;
using KingWorldWar.Core.Scenes;
using KingWorldWar.Features.Camera;
using KingWorldWar.UI.DebugOverlay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace KingWorldWar.Editor
{
    /// <summary>
    /// Automated builder for King World War 3D visual foundation assets, materials, prefabs, and test scene.
    /// </summary>
    public static class VisualFoundationBuilder
    {
        private const string MaterialsPath = "Assets/KINGWorldWar/Art/Materials";
        private const string PrefabsPath = "Assets/KINGWorldWar/Art/Prefabs";
        private const string SettingsPath = "Assets/KINGWorldWar/Art/Settings";
        private const string PalettesPath = "Assets/KINGWorldWar/Art/Palettes";
        private const string ScenesPath = "Assets/KINGWorldWar/Scenes";

        [MenuItem("KingWorldWar/Build Visual Foundation (Phase 2)")]
        public static void BuildAll()
        {
            EnsureDirectories();
            PaletteSO palette = CreateOrLoadPalette();
            Material[] materials = CreateMaterials(palette);
            CreateQualityURPAssets();
            GameObject[] prefabs = CreatePrefabs(materials);
            BuildVisualTestScene(prefabs, materials[0]);
            RegisterSceneInBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VisualFoundationBuilder] Phase 2 Visual Foundation build completed successfully!");
        }

        private static void EnsureDirectories()
        {
            string[] dirs = {
                "Assets/KINGWorldWar/Art",
                MaterialsPath,
                PalettesPath,
                SettingsPath,
                PrefabsPath,
                $"{PrefabsPath}/Buildings",
                $"{PrefabsPath}/Units",
                ScenesPath
            };

            foreach (var dir in dirs)
            {
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    string parent = Path.GetDirectoryName(dir);
                    string name = Path.GetFileName(dir);
                    AssetDatabase.CreateFolder(parent, name);
                }
            }
        }

        private static PaletteSO CreateOrLoadPalette()
        {
            string path = $"{PalettesPath}/KWW_OfficialPalette.asset";
            var palette = AssetDatabase.LoadAssetAtPath<PaletteSO>(path);
            if (palette == null)
            {
                palette = ScriptableObject.CreateInstance<PaletteSO>();
                AssetDatabase.CreateAsset(palette, path);
            }
            return palette;
        }

        public static Material[] CreateMaterials(PaletteSO palette)
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null)
            {
                litShader = Shader.Find("Standard");
            }

            // 0: Terrain, 1: Charcoal, 2: Steel, 3: Military Green, 4: Restrained Gold
            Material mTerrain = CreateOrUpdateMaterial($"{MaterialsPath}/M_Terrain_Ground.mat", litShader, palette.TerrainDark, 0.1f, 0.1f);
            Material mCharcoal = CreateOrUpdateMaterial($"{MaterialsPath}/M_Building_Charcoal.mat", litShader, palette.Charcoal, 0.1f, 0.25f);
            Material mSteel = CreateOrUpdateMaterial($"{MaterialsPath}/M_Building_Steel.mat", litShader, palette.SteelGray, 0.8f, 0.5f);
            Material mMilitaryGreen = CreateOrUpdateMaterial($"{MaterialsPath}/M_Military_Green.mat", litShader, palette.MutedMilitaryGreen, 0.1f, 0.3f);
            Material mGold = CreateOrUpdateMaterial($"{MaterialsPath}/M_Accent_Gold.mat", litShader, palette.RestrainedGold, 0.85f, 0.65f);

            return new[] { mTerrain, mCharcoal, mSteel, mMilitaryGreen, mGold };
        }

        private static Material CreateOrUpdateMaterial(string path, Shader shader, Color color, float metallic, float smoothness)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void CreateQualityURPAssets()
        {
            string sourcePath = "Assets/SourceFiles/Settings/Mobile_RPAsset.asset";
            if (!File.Exists(sourcePath)) return;

            string[] tiers = { "Low", "Medium", "High" };
            float[] renderScales = { 0.8f, 1.0f, 1.0f };
            float[] shadowDists = { 25f, 50f, 80f };
            int[] msaas = { 1, 2, 4 };

            for (int i = 0; i < tiers.Length; i++)
            {
                string targetPath = $"{SettingsPath}/URP_Asset_{tiers[i]}.asset";
                if (!File.Exists(targetPath))
                {
                    AssetDatabase.CopyAsset(sourcePath, targetPath);
                }

                var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(targetPath);
                if (asset != null)
                {
                    asset.renderScale = renderScales[i];
                    asset.shadowDistance = shadowDists[i];
                    asset.msaaSampleCount = msaas[i];
                    asset.supportsHDR = i > 0;
                    EditorUtility.SetDirty(asset);
                }
            }
        }

        private static GameObject[] CreatePrefabs(Material[] mats)
        {
            Material mCharcoal = mats[1];
            Material mSteel = mats[2];
            Material mMilitary = mats[3];
            Material mGold = mats[4];

            // 1. HQ CommandCenter
            GameObject hq = new GameObject("HQ_CommandCenter");
            var baseBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseBlock.name = "Base";
            baseBlock.transform.SetParent(hq.transform);
            baseBlock.transform.localScale = new Vector3(8f, 3f, 8f);
            baseBlock.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            baseBlock.GetComponent<Renderer>().sharedMaterial = mCharcoal;

            var tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tower.name = "CommandTower";
            tower.transform.SetParent(hq.transform);
            tower.transform.localScale = new Vector3(4f, 4f, 4f);
            tower.transform.localPosition = new Vector3(0f, 5f, 0f);
            tower.GetComponent<Renderer>().sharedMaterial = mMilitary;

            var antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            antenna.name = "Antenna";
            antenna.transform.SetParent(hq.transform);
            antenna.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
            antenna.transform.localPosition = new Vector3(0f, 8f, 0f);
            antenna.GetComponent<Renderer>().sharedMaterial = mGold;

            string hqPath = $"{PrefabsPath}/Buildings/HQ_CommandCenter.prefab";
            PrefabUtility.SaveAsPrefabAsset(hq, hqPath);
            Object.DestroyImmediate(hq);

            // 2. Barracks
            GameObject barracks = new GameObject("Barracks");
            var barBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barBody.transform.SetParent(barracks.transform);
            barBody.transform.localScale = new Vector3(10f, 2.5f, 4f);
            barBody.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            barBody.GetComponent<Renderer>().sharedMaterial = mCharcoal;

            var barRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barRoof.transform.SetParent(barracks.transform);
            barRoof.transform.localScale = new Vector3(10.4f, 0.5f, 4.4f);
            barRoof.transform.localPosition = new Vector3(0f, 2.75f, 0f);
            barRoof.GetComponent<Renderer>().sharedMaterial = mSteel;

            string barPath = $"{PrefabsPath}/Buildings/Barracks.prefab";
            PrefabUtility.SaveAsPrefabAsset(barracks, barPath);
            Object.DestroyImmediate(barracks);

            // 3. Supply Depot
            GameObject depot = new GameObject("SupplyDepot");
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.transform.SetParent(depot.transform);
            pad.transform.localScale = new Vector3(7f, 0.4f, 7f);
            pad.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            pad.GetComponent<Renderer>().sharedMaterial = mCharcoal;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    var silo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    silo.transform.SetParent(depot.transform);
                    silo.transform.localScale = new Vector3(1.6f, 1.8f, 1.6f);
                    silo.transform.localPosition = new Vector3(x * 1.8f, 2.0f, z * 1.8f);
                    silo.GetComponent<Renderer>().sharedMaterial = mSteel;
                }
            }

            string depotPath = $"{PrefabsPath}/Buildings/SupplyDepot.prefab";
            PrefabUtility.SaveAsPrefabAsset(depot, depotPath);
            Object.DestroyImmediate(depot);

            // 4. Radar Station
            GameObject radar = new GameObject("RadarStation");
            var radBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            radBase.transform.SetParent(radar.transform);
            radBase.transform.localScale = new Vector3(4f, 1.5f, 4f);
            radBase.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            radBase.GetComponent<Renderer>().sharedMaterial = mCharcoal;

            var dish = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dish.transform.SetParent(radar.transform);
            dish.transform.localScale = new Vector3(2.5f, 0.5f, 2.5f);
            dish.transform.localPosition = new Vector3(0f, 4.0f, 0f);
            dish.transform.localRotation = Quaternion.Euler(30f, 45f, 0f);
            dish.GetComponent<Renderer>().sharedMaterial = mGold;

            string radarPath = $"{PrefabsPath}/Buildings/RadarStation.prefab";
            PrefabUtility.SaveAsPrefabAsset(radar, radarPath);
            Object.DestroyImmediate(radar);

            // 5. Vehicle Factory
            GameObject factory = new GameObject("VehicleFactory");
            var factBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            factBody.transform.SetParent(factory.transform);
            factBody.transform.localScale = new Vector3(12f, 3.5f, 8f);
            factBody.transform.localPosition = new Vector3(0f, 1.75f, 0f);
            factBody.GetComponent<Renderer>().sharedMaterial = mMilitary;

            var factDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            factDoor.transform.SetParent(factory.transform);
            factDoor.transform.localScale = new Vector3(4f, 2.8f, 0.3f);
            factDoor.transform.localPosition = new Vector3(0f, 1.4f, 4f);
            factDoor.GetComponent<Renderer>().sharedMaterial = mSteel;

            string factPath = $"{PrefabsPath}/Buildings/VehicleFactory.prefab";
            PrefabUtility.SaveAsPrefabAsset(factory, factPath);
            Object.DestroyImmediate(factory);

            // 6. Tank Unit Placeholder
            GameObject tank = new GameObject("Unit_Tank_Placeholder");
            var chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chassis.transform.SetParent(tank.transform);
            chassis.transform.localScale = new Vector3(2.4f, 0.8f, 3.6f);
            chassis.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            chassis.GetComponent<Renderer>().sharedMaterial = mMilitary;

            var tTurret = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tTurret.transform.SetParent(tank.transform);
            tTurret.transform.localScale = new Vector3(1.5f, 0.6f, 1.8f);
            tTurret.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            tTurret.GetComponent<Renderer>().sharedMaterial = mCharcoal;

            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.transform.SetParent(tank.transform);
            barrel.transform.localScale = new Vector3(0.2f, 1.4f, 0.2f);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            barrel.transform.localPosition = new Vector3(0f, 1.3f, 1.8f);
            barrel.GetComponent<Renderer>().sharedMaterial = mSteel;

            string tankPath = $"{PrefabsPath}/Units/Unit_Tank_Placeholder.prefab";
            PrefabUtility.SaveAsPrefabAsset(tank, tankPath);
            Object.DestroyImmediate(tank);

            // 7. Aircraft Unit Placeholder
            GameObject aircraft = new GameObject("Unit_Aircraft_Placeholder");
            var fuse = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fuse.transform.SetParent(aircraft.transform);
            fuse.transform.localScale = new Vector3(1f, 0.8f, 5f);
            fuse.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            fuse.GetComponent<Renderer>().sharedMaterial = mCharcoal;

            var wings = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wings.transform.SetParent(aircraft.transform);
            wings.transform.localScale = new Vector3(6f, 0.1f, 1.5f);
            wings.transform.localPosition = new Vector3(0f, 0.6f, 0.5f);
            wings.GetComponent<Renderer>().sharedMaterial = mMilitary;

            var tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tail.transform.SetParent(aircraft.transform);
            tail.transform.localScale = new Vector3(0.1f, 1.2f, 0.8f);
            tail.transform.localPosition = new Vector3(0f, 1.2f, -2.2f);
            tail.GetComponent<Renderer>().sharedMaterial = mGold;

            string airPath = $"{PrefabsPath}/Units/Unit_Aircraft_Placeholder.prefab";
            PrefabUtility.SaveAsPrefabAsset(aircraft, airPath);
            Object.DestroyImmediate(aircraft);

            return new[]
            {
                AssetDatabase.LoadAssetAtPath<GameObject>(hqPath),
                AssetDatabase.LoadAssetAtPath<GameObject>(barPath),
                AssetDatabase.LoadAssetAtPath<GameObject>(depotPath),
                AssetDatabase.LoadAssetAtPath<GameObject>(radarPath),
                AssetDatabase.LoadAssetAtPath<GameObject>(factPath),
                AssetDatabase.LoadAssetAtPath<GameObject>(tankPath),
                AssetDatabase.LoadAssetAtPath<GameObject>(airPath)
            };
        }

        private static void BuildVisualTestScene(GameObject[] prefabs, Material groundMat)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Directional Sun Light
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.97f, 0.92f);
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 2. Ground Plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Terrain";
            ground.transform.localScale = new Vector3(12f, 1f, 12f); // 120 x 120 meters
            ground.transform.position = Vector3.zero;
            ground.GetComponent<Renderer>().sharedMaterial = groundMat;

            // 3. Layout Buildings
            if (prefabs.Length >= 5)
            {
                PrefabUtility.InstantiatePrefab(prefabs[0], ground.transform.parent); // HQ at (0, 0, 0)

                var b1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[1]); // Barracks
                b1.transform.position = new Vector3(-16f, 0f, -8f);

                var b2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[2]); // Depot
                b2.transform.position = new Vector3(16f, 0f, -8f);

                var b3 = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[3]); // Radar
                b3.transform.position = new Vector3(-16f, 0f, 12f);

                var b4 = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[4]); // Factory
                b4.transform.position = new Vector3(16f, 0f, 12f);
            }

            // 4. Layout Units
            if (prefabs.Length >= 7)
            {
                var tank = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[5]);
                tank.transform.position = new Vector3(0f, 0f, -16f);
                tank.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

                var air = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[6]);
                air.transform.position = new Vector3(0f, 0f, 22f);
                air.transform.rotation = Quaternion.Euler(0f, -45f, 0f);
            }

            // 5. Camera Rig
            var rigGo = new GameObject("StrategyCameraRig");
            var rig = rigGo.AddComponent<StrategyCameraRig>();
            var camConfig = ScriptableObject.CreateInstance<CameraConfigSO>();
            rig.Config = camConfig;
            rig.SetFocusPosition(Vector3.zero, immediate: true);

            // 6. UI Canvas & Debug Overlay
            var canvasGo = new GameObject("DebugCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var overlayGo = new GameObject("DebugOverlayView");
            overlayGo.transform.SetParent(canvasGo.transform, false);
            var rect = overlayGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(16f, -16f);
            rect.sizeDelta = new Vector2(320f, 140f);

            var bg = overlayGo.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.08f, 0.75f);

            var textGo = new GameObject("StatsText");
            textGo.transform.SetParent(overlayGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 10f);
            textRect.offsetMax = new Vector2(-10f, -10f);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 14f;
            tmp.color = Color.white;

            var debugView = overlayGo.AddComponent<DebugOverlayView>();
            // Use reflection or serialized object to bind tmp
            var serializedView = new SerializedObject(debugView);
            serializedView.FindProperty("_statsText").objectReferenceValue = tmp;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            // Event System
            var eventGo = new GameObject("EventSystem");
            eventGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            string scenePath = $"{ScenesPath}/{SceneNameCatalog.VisualTest}.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void RegisterSceneInBuildSettings()
        {
            string scenePath = $"{ScenesPath}/{SceneNameCatalog.VisualTest}.unity";
            var existing = EditorBuildSettings.scenes;

            foreach (var s in existing)
            {
                if (s.path == scenePath) return;
            }

            var updated = new EditorBuildSettingsScene[existing.Length + 1];
            existing.CopyTo(updated, 0);
            updated[existing.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
