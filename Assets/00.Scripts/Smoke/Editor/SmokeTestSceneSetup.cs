using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SmokeSystem
{
    /// <summary>Tools > Smoke Grenade > Setup SmokeTest Scene: wires the existing SmokeTest scene's Main Camera up with move/look, throwing, and a disturb-test key, so the smoke system can be tested interactively instead of from a fixed viewpoint.</summary>
    public static class SmokeTestSceneSetup
    {
        const string ScenePath = "Assets/02.Scenes/SmokeTest.unity";
        const string GrenadePrefabPath = "Assets/00.Scripts/Smoke/SmokeGrenade.prefab";

        [MenuItem("Tools/Smoke Grenade/Setup SmokeTest Scene")]
        public static void Setup()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject cam = GameObject.Find("Main Camera");
            if (cam == null)
            {
                Debug.LogError("SmokeTestSceneSetup: 'Main Camera' not found in SmokeTest scene.");
                return;
            }

            // The old fixed-position Sphere thrower would double-fire alongside the camera's
            // new one (both just poll the right mouse button globally), so retire it.
            GameObject sphere = GameObject.Find("Sphere");
            if (sphere != null)
            {
                SmokeGrenadeThrower oldThrower = sphere.GetComponent<SmokeGrenadeThrower>();
                if (oldThrower != null)
                    Object.DestroyImmediate(oldThrower);
                sphere.SetActive(false);
            }

            if (cam.GetComponent<SmokeTestPlayerController>() == null)
                cam.AddComponent<SmokeTestPlayerController>();

            if (cam.GetComponent<SmokeVisionEffect>() == null)
                cam.AddComponent<SmokeVisionEffect>();

            if (cam.GetComponent<SmokeTestShooter>() == null)
                cam.AddComponent<SmokeTestShooter>();

            SmokeGrenadeThrower thrower = cam.GetComponent<SmokeGrenadeThrower>();
            if (thrower == null)
                thrower = cam.AddComponent<SmokeGrenadeThrower>();

            SmokeGrenadeProjectile grenadePrefab = AssetDatabase.LoadAssetAtPath<SmokeGrenadeProjectile>(GrenadePrefabPath);
            SerializedObject so = new SerializedObject(thrower);
            so.FindProperty("grenadePrefab").objectReferenceValue = grenadePrefab;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            if (saved)
                Debug.Log("[SmokeTestSceneSetup] SmokeTest ready — WASD/mouse to move, RMB to throw, F to punch a smoke hole, Esc to release the cursor.");
            else
                Debug.LogError("[SmokeTestSceneSetup] Failed to save SmokeTest scene.");
        }
    }
}
