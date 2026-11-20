using System.IO;
using UnityEditor;
using UnityEngine;

namespace KingWorldWar.Editor
{
    /// <summary>
    /// Editor utility for generating 100% original low-poly 3D procedural placeholder prefabs
    /// for all 5 MVP unit archetypes:
    /// 1. Main Battle Tank (Ground)
    /// 2. Armored Vehicle (Ground)
    /// 3. Heavy Artillery (Ground)
    /// 4. Fighter Aircraft (Air)
    /// 5. Recon Aircraft (Air)
    /// Zero third-party assets ($0 cost, 100% original geometry).
    /// </summary>
    [InitializeOnLoad]
    public static class UnitPrefabGenerator
    {
        private const string PrefabDir = "Assets/KINGWorldWar/Art/Prefabs/Units";
        private const string MaterialsDir = "Assets/KINGWorldWar/Art/Materials";

        static UnitPrefabGenerator()
        {
            EditorApplication.delayCall += EnsureAllPrefabsExist;
        }

        [MenuItem("KingWorldWar/Generate MVP Unit Prefabs (Phase 7)")]
        public static void EnsureAllPrefabsExist()
        {
            if (!Directory.Exists(PrefabDir))
            {
                Directory.CreateDirectory(PrefabDir);
            }

            var matCharcoal = LoadOrCreateMaterial("M_Charcoal", new Color32(0x1F, 0x24, 0x21, 0xFF));
            var matSteel = LoadOrCreateMaterial("M_SteelGray", new Color32(0x4A, 0x55, 0x68, 0xFF));
            var matMilitary = LoadOrCreateMaterial("M_MilitaryGreen", new Color32(0x3E, 0x4E, 0x3D, 0xFF));
            var matGold = LoadOrCreateMaterial("M_RestrainedGold", new Color32(0xD4, 0xAF, 0x37, 0xFF));

            CreateTankPrefab(matMilitary, matCharcoal, matSteel);
            CreateArmoredVehiclePrefab(matMilitary, matCharcoal, matSteel);
            CreateArtilleryPrefab(matSteel, matMilitary, matCharcoal);
            CreateFighterAircraftPrefab(matCharcoal, matMilitary, matGold, matSteel);
            CreateReconAircraftPrefab(matSteel, matMilitary, matCharcoal, matGold);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Material LoadOrCreateMaterial(string name, Color color)
        {
            string path = $"{MaterialsDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            if (!Directory.Exists(MaterialsDir))
            {
                Directory.CreateDirectory(MaterialsDir);
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void CreateTankPrefab(Material matHull, Material matTreads, Material matBarrel)
        {
            string path = $"{PrefabDir}/Unit_Tank_Placeholder.prefab";
            if (File.Exists(path)) return;

            var root = new GameObject("Unit_Tank_Placeholder");

            // Hull
            var hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hull.name = "Hull";
            hull.transform.SetParent(root.transform);
            hull.transform.localScale = new Vector3(2.4f, 0.9f, 4.0f);
            hull.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            hull.GetComponent<Renderer>().sharedMaterial = matHull;

            // Left Tread
            var leftTread = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftTread.name = "LeftTread";
            leftTread.transform.SetParent(root.transform);
            leftTread.transform.localScale = new Vector3(0.5f, 0.7f, 4.2f);
            leftTread.transform.localPosition = new Vector3(-1.3f, 0.35f, 0f);
            leftTread.GetComponent<Renderer>().sharedMaterial = matTreads;

            // Right Tread
            var rightTread = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightTread.name = "RightTread";
            rightTread.transform.SetParent(root.transform);
            rightTread.transform.localScale = new Vector3(0.5f, 0.7f, 4.2f);
            rightTread.transform.localPosition = new Vector3(1.3f, 0.35f, 0f);
            rightTread.GetComponent<Renderer>().sharedMaterial = matTreads;

            // Turret
            var turret = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            turret.name = "Turret";
            turret.transform.SetParent(root.transform);
            turret.transform.localScale = new Vector3(1.6f, 0.45f, 1.8f);
            turret.transform.localPosition = new Vector3(0f, 1.15f, -0.2f);
            turret.GetComponent<Renderer>().sharedMaterial = matTreads;

            // Main Gun Barrel
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "MainGunBarrel";
            barrel.transform.SetParent(root.transform);
            barrel.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            barrel.transform.localPosition = new Vector3(0f, 1.2f, 1.9f);
            barrel.GetComponent<Renderer>().sharedMaterial = matBarrel;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void CreateArmoredVehiclePrefab(Material matHull, Material matWheels, Material matWeapon)
        {
            string path = $"{PrefabDir}/Unit_ArmoredVehicle_Placeholder.prefab";
            if (File.Exists(path)) return;

            var root = new GameObject("Unit_ArmoredVehicle_Placeholder");

            // Sleek Armored Hull
            var hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hull.name = "Hull";
            hull.transform.SetParent(root.transform);
            hull.transform.localScale = new Vector3(1.8f, 0.9f, 3.4f);
            hull.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            hull.GetComponent<Renderer>().sharedMaterial = matHull;

            // Wheels (6-wheel combat carrier)
            for (int i = -1; i <= 1; i++)
            {
                var wL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wL.name = $"Wheel_L_{i}";
                wL.transform.SetParent(root.transform);
                wL.transform.localScale = new Vector3(0.6f, 0.2f, 0.6f);
                wL.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                wL.transform.localPosition = new Vector3(-1.0f, 0.35f, i * 1.1f);
                wL.GetComponent<Renderer>().sharedMaterial = matWheels;

                var wR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wR.name = $"Wheel_R_{i}";
                wR.transform.SetParent(root.transform);
                wR.transform.localScale = new Vector3(0.6f, 0.2f, 0.6f);
                wR.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                wR.transform.localPosition = new Vector3(1.0f, 0.35f, i * 1.1f);
                wR.GetComponent<Renderer>().sharedMaterial = matWheels;
            }

            // Compact Rapid-Fire Autocannon Turret
            var turret = GameObject.CreatePrimitive(PrimitiveType.Cube);
            turret.name = "AutocannonTurret";
            turret.transform.SetParent(root.transform);
            turret.transform.localScale = new Vector3(0.9f, 0.5f, 0.9f);
            turret.transform.localPosition = new Vector3(0f, 1.35f, 0.3f);
            turret.GetComponent<Renderer>().sharedMaterial = matWeapon;

            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "AutocannonBarrel";
            barrel.transform.SetParent(root.transform);
            barrel.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            barrel.transform.localPosition = new Vector3(0f, 1.4f, 1.2f);
            barrel.GetComponent<Renderer>().sharedMaterial = matWheels;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void CreateArtilleryPrefab(Material matChassis, Material matMount, Material matBarrel)
        {
            string path = $"{PrefabDir}/Unit_Artillery_Placeholder.prefab";
            if (File.Exists(path)) return;

            var root = new GameObject("Unit_Artillery_Placeholder");

            // Heavy Chassis
            var chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chassis.name = "Chassis";
            chassis.transform.SetParent(root.transform);
            chassis.transform.localScale = new Vector3(2.6f, 0.8f, 4.4f);
            chassis.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            chassis.GetComponent<Renderer>().sharedMaterial = matChassis;

            // Heavy Siege Gun Mount
            var mount = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mount.name = "GunMount";
            mount.transform.SetParent(root.transform);
            mount.transform.localScale = new Vector3(1.8f, 1.0f, 1.8f);
            mount.transform.localPosition = new Vector3(0f, 1.2f, -0.6f);
            mount.GetComponent<Renderer>().sharedMaterial = matMount;

            // Long-Range Howitzer Barrel (Angled upwards at 25 degrees)
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "HeavyHowitzerBarrel";
            barrel.transform.SetParent(root.transform);
            barrel.transform.localScale = new Vector3(0.32f, 2.2f, 0.32f);
            barrel.transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
            barrel.transform.localPosition = new Vector3(0f, 2.0f, 1.1f);
            barrel.GetComponent<Renderer>().sharedMaterial = matBarrel;

            // Rear Recoil Stabilizer Spades
            var spade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spade.name = "RecoilSpade";
            spade.transform.SetParent(root.transform);
            spade.transform.localScale = new Vector3(2.4f, 0.3f, 0.8f);
            spade.transform.localPosition = new Vector3(0f, 0.2f, -2.4f);
            spade.GetComponent<Renderer>().sharedMaterial = matChassis;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void CreateFighterAircraftPrefab(Material matFuselage, Material matWings, Material matCanopy, Material matEngines)
        {
            string path = $"{PrefabDir}/Unit_FighterAircraft_Placeholder.prefab";
            if (File.Exists(path)) return;

            var root = new GameObject("Unit_FighterAircraft_Placeholder");

            // Sleek Jet Fuselage
            var fuse = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fuse.name = "Fuselage";
            fuse.transform.SetParent(root.transform);
            fuse.transform.localScale = new Vector3(0.9f, 0.6f, 4.6f);
            fuse.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            fuse.GetComponent<Renderer>().sharedMaterial = matFuselage;

            // Swept Delta Wings
            var wings = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wings.name = "DeltaWings";
            wings.transform.SetParent(root.transform);
            wings.transform.localScale = new Vector3(4.8f, 0.08f, 1.8f);
            wings.transform.localPosition = new Vector3(0f, 0.5f, 0.2f);
            wings.GetComponent<Renderer>().sharedMaterial = matWings;

            // Twin Vertical Stabilizers
            var stabL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stabL.name = "VertStab_L";
            stabL.transform.SetParent(root.transform);
            stabL.transform.localScale = new Vector3(0.08f, 0.9f, 0.8f);
            stabL.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
            stabL.transform.localPosition = new Vector3(-0.55f, 1.0f, -1.8f);
            stabL.GetComponent<Renderer>().sharedMaterial = matEngines;

            var stabR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stabR.name = "VertStab_R";
            stabR.transform.SetParent(root.transform);
            stabR.transform.localScale = new Vector3(0.08f, 0.9f, 0.8f);
            stabR.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
            stabR.transform.localPosition = new Vector3(0.55f, 1.0f, -1.8f);
            stabR.GetComponent<Renderer>().sharedMaterial = matEngines;

            // Cockpit Canopy
            var canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopy.name = "CockpitCanopy";
            canopy.transform.SetParent(root.transform);
            canopy.transform.localScale = new Vector3(0.5f, 0.35f, 1.2f);
            canopy.transform.localPosition = new Vector3(0f, 0.95f, 0.8f);
            canopy.GetComponent<Renderer>().sharedMaterial = matCanopy;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void CreateReconAircraftPrefab(Material matFuselage, Material matWings, Material matTail, Material matSensors)
        {
            string path = $"{PrefabDir}/Unit_ReconAircraft_Placeholder.prefab";
            if (File.Exists(path)) return;

            var root = new GameObject("Unit_ReconAircraft_Placeholder");

            // Slender UAV / High-Altitude Scout Fuselage
            var fuse = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fuse.name = "Fuselage";
            fuse.transform.SetParent(root.transform);
            fuse.transform.localScale = new Vector3(0.7f, 0.5f, 3.8f);
            fuse.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            fuse.GetComponent<Renderer>().sharedMaterial = matFuselage;

            // High-Aspect Glider Wings (Wide surveillance span)
            var wings = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wings.name = "LongWings";
            wings.transform.SetParent(root.transform);
            wings.transform.localScale = new Vector3(7.2f, 0.06f, 0.9f);
            wings.transform.localPosition = new Vector3(0f, 0.5f, 0.5f);
            wings.GetComponent<Renderer>().sharedMaterial = matWings;

            // V-Tail
            var tailL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tailL.name = "VTail_L";
            tailL.transform.SetParent(root.transform);
            tailL.transform.localScale = new Vector3(0.06f, 0.7f, 0.6f);
            tailL.transform.localRotation = Quaternion.Euler(0f, 0f, -30f);
            tailL.transform.localPosition = new Vector3(-0.35f, 0.8f, -1.6f);
            tailL.GetComponent<Renderer>().sharedMaterial = matTail;

            var tailR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tailR.name = "VTail_R";
            tailR.transform.SetParent(root.transform);
            tailR.transform.localScale = new Vector3(0.06f, 0.7f, 0.6f);
            tailR.transform.localRotation = Quaternion.Euler(0f, 0f, 30f);
            tailR.transform.localPosition = new Vector3(0.35f, 0.8f, -1.6f);
            tailR.GetComponent<Renderer>().sharedMaterial = matTail;

            // Multispectral Recon Sensor Pod under nose
            var pod = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pod.name = "SensorTurret";
            pod.transform.SetParent(root.transform);
            pod.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            pod.transform.localPosition = new Vector3(0f, 0.05f, 1.4f);
            pod.GetComponent<Renderer>().sharedMaterial = matSensors;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }
    }
}
