using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Tools > Smoke Grenade > Build Test Scene 실행 시 연막탄 프리팹 2개와
// 네트워크 없이 바로 플레이해볼 수 있는 테스트 씬을 자동으로 만들어준다.
public static class SmokeGrenadeTestSceneBuilder
{
    const string ScenePath = "Assets/02.Scenes/SmokeGrenadeTest.unity";
    const string PrefabFolder = "Assets/05.Prefabs/Grenades";
    const string CloudPrefabPath = PrefabFolder + "/SmokeCloud.prefab";
    const string GrenadePrefabPath = PrefabFolder + "/SmokeGrenade.prefab";

    [MenuItem("Tools/Smoke Grenade/Build Test Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if ((System.IO.File.Exists(ScenePath) || System.IO.File.Exists(GrenadePrefabPath))
            && !EditorUtility.DisplayDialog(
                "Smoke Grenade Test Scene",
                "기존 테스트 씬/프리팹을 덮어씁니다. 계속할까요?",
                "계속", "취소"))
        {
            return;
        }

        EnsureFolder();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject cloudPrefab = BuildSmokeCloudPrefab();
        GameObject grenadePrefab = BuildSmokeGrenadePrefab(cloudPrefab);

        BuildEnvironment();
        BuildPlayer(grenadePrefab);

        bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (saved)
            Debug.Log($"Smoke grenade test scene created at {ScenePath}");
        else
            Debug.LogError($"씬 저장 실패: {ScenePath}");
    }

    static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(PrefabFolder))
            AssetDatabase.CreateFolder("Assets/05.Prefabs", "Grenades");
    }

    static GameObject BuildSmokeCloudPrefab()
    {
        GameObject go = new GameObject("SmokeCloud");
        go.AddComponent<ParticleSystem>();
        go.AddComponent<SmokeCloud>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, CloudPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject BuildSmokeGrenadePrefab(GameObject cloudPrefab)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "SmokeGrenade";
        go.transform.localScale = Vector3.one * 0.12f;

        SmokeGrenadeProjectile projectile = go.AddComponent<SmokeGrenadeProjectile>();

        Rigidbody rb = go.GetComponent<Rigidbody>();
        rb.mass = 0.5f;
        rb.linearDamping = 0.05f;

        SerializedObject so = new SerializedObject(projectile);
        so.FindProperty("smokeCloudPrefab").objectReferenceValue = cloudPrefab;
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, GrenadePrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static void BuildEnvironment()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(4f, 1f, 4f);

        CreateWall("Wall_Left", new Vector3(-3f, 1.5f, 5f), new Vector3(8f, 3f, 0.4f));
        CreateWall("Wall_Corner", new Vector3(2f, 1.5f, 8f), new Vector3(0.4f, 3f, 6f));
    }

    static void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
    }

    static void BuildPlayer(GameObject grenadePrefab)
    {
        Camera cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
        if (cam == null) return;

        cam.transform.SetPositionAndRotation(new Vector3(0f, 1.7f, -6f), Quaternion.identity);

        cam.gameObject.AddComponent<SmokeGrenadeTestPlayer>();
        cam.gameObject.AddComponent<SmokeVisionEffect>();
        SmokeGrenadeTestThrower thrower = cam.gameObject.AddComponent<SmokeGrenadeTestThrower>();

        SerializedObject so = new SerializedObject(thrower);
        so.FindProperty("grenadePrefab").objectReferenceValue = grenadePrefab;
        so.FindProperty("throwOrigin").objectReferenceValue = cam.transform;
        so.ApplyModifiedProperties();
    }
}
