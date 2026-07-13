using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MapEditorTool : EditorWindow
{
    private const string PartsFolder = "Assets/05.Prefabs/MapObjects";
    private const string MapRootName = "Map";
    private const float RotationStep = 15f;
    private const float ScaleStep = 0.1f;

    private List<GameObject> parts = new List<GameObject>();
    private GameObject selectedPart;
    private GameObject previewInstance;

    private bool isPlacing;
    private bool keepPlacing = true;
    private bool snapEnabled = true;
    private float gridSize = 1f;
    private float previewRotationY;
    private float previewScale = 1f;

    private float rotateStepInput = 15f;
    private float scaleStepInput = 0.1f;
    private float absoluteScaleInput = 1f;
    private float yStepInput = 0.1f;
    private float absoluteYInput;

    private Vector2 scroll;
    private Vector2 windowScroll;

    [MenuItem("Tools/Map Editor Tool")]
    private static void Open()
    {
        GetWindow<MapEditorTool>("Map Editor").RefreshParts();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        RefreshParts();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        StopPlacing();
    }

    private void RefreshParts()
    {
        parts = AssetDatabase.FindAssets("t:Prefab", new[] { PartsFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
            .Where(go => go != null)
            .OrderBy(go => go.name)
            .ToList();
        Repaint();
    }

    private void OnGUI()
    {
        windowScroll = EditorGUILayout.BeginScrollView(windowScroll);

        EditorGUILayout.LabelField("Map Parts", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh"))
            RefreshParts();

        EditorGUILayout.Space();
        snapEnabled = EditorGUILayout.Toggle("Snap To Grid", snapEnabled);
        using (new EditorGUI.DisabledScope(!snapEnabled))
            gridSize = EditorGUILayout.FloatField("Grid Size", Mathf.Max(0.01f, gridSize));
        keepPlacing = EditorGUILayout.Toggle("Keep Placing After Click", keepPlacing);

        EditorGUILayout.Space();
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(200));
        foreach (var part in parts)
        {
            bool isSelected = part == selectedPart;
            var style = isSelected ? EditorStyles.miniButtonMid : GUI.skin.button;
            var content = new GUIContent(part.name, AssetPreview.GetAssetPreview(part));
            if (GUILayout.Button(content, style, GUILayout.Height(48)))
            {
                selectedPart = part;
                StartPlacing();
            }
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (isPlacing)
        {
            EditorGUILayout.HelpBox(
                $"Click in Scene view to place.\nQ/E: rotate ({previewRotationY:0}°)   R/F: scale ({previewScale:0.00}x)   Esc: stop.",
                MessageType.Info);
            if (GUILayout.Button("Stop Placing"))
                StopPlacing();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Selected Object Tools", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("Snap Selected To Grid"))
                SnapSelectedToGrid();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotation");
            rotateStepInput = EditorGUILayout.FloatField("Rotate Step", rotateStepInput);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button($"-{rotateStepInput}°"))
                    RotateSelected(-rotateStepInput);
                if (GUILayout.Button($"+{rotateStepInput}°"))
                    RotateSelected(rotateStepInput);
                if (GUILayout.Button("Reset"))
                    SetSelectedRotation(Quaternion.identity);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Position Y (PageUp/PageDown in Scene view)");
            yStepInput = EditorGUILayout.FloatField("Y Step", yStepInput);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button($"-{yStepInput}"))
                    MoveSelectedY(-yStepInput);
                if (GUILayout.Button($"+{yStepInput}"))
                    MoveSelectedY(yStepInput);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                absoluteYInput = EditorGUILayout.FloatField(absoluteYInput);
                if (GUILayout.Button("Set Y"))
                    SetSelectedY(absoluteYInput);
                if (GUILayout.Button("Reset"))
                    SetSelectedY(0f);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scale");
            scaleStepInput = EditorGUILayout.FloatField("Scale Step", scaleStepInput);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("-"))
                    ScaleSelected(-scaleStepInput);
                if (GUILayout.Button("+"))
                    ScaleSelected(scaleStepInput);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                absoluteScaleInput = EditorGUILayout.FloatField(absoluteScaleInput);
                if (GUILayout.Button("Set Uniform Scale"))
                    SetSelectedScale(Mathf.Max(0.01f, absoluteScaleInput));
                if (GUILayout.Button("Reset"))
                    SetSelectedScale(1f);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Unpack Selected From Prefab"))
                UnpackSelected();

            EditorGUILayout.Space();
            DrawMaterialsSection();

            EditorGUILayout.Space();
            if (GUILayout.Button("Delete Selected"))
                DeleteSelected();
        }

        EditorGUILayout.EndScrollView();
    }

    private void StartPlacing()
    {
        if (selectedPart == null)
            return;

        StopPlacing();
        isPlacing = true;
        previewInstance = Instantiate(selectedPart);
        previewInstance.name = $"[Preview] {selectedPart.name}";
        previewInstance.hideFlags = HideFlags.HideAndDontSave;
        previewRotationY = 0f;
        previewScale = 1f;
        foreach (var collider in previewInstance.GetComponentsInChildren<Collider>())
            collider.enabled = false;
        SceneView.RepaintAll();
    }

    private void StopPlacing()
    {
        isPlacing = false;
        if (previewInstance != null)
            DestroyImmediate(previewInstance);
        previewInstance = null;
    }

    private void OnSceneGUI(SceneView view)
    {
        if (!isPlacing || selectedPart == null)
        {
            HandleSelectedYShortcuts();
            return;
        }

        view.wantsMouseMove = true;
        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Q) { previewRotationY -= RotationStep; e.Use(); Repaint(); }
            else if (e.keyCode == KeyCode.E) { previewRotationY += RotationStep; e.Use(); Repaint(); }
            else if (e.keyCode == KeyCode.R) { previewScale = Mathf.Max(0.05f, previewScale + ScaleStep); e.Use(); Repaint(); }
            else if (e.keyCode == KeyCode.F) { previewScale = Mathf.Max(0.05f, previewScale - ScaleStep); e.Use(); Repaint(); }
            else if (e.keyCode == KeyCode.Escape) { StopPlacing(); e.Use(); Repaint(); return; }
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 point = Physics.Raycast(ray, out RaycastHit hit) ? hit.point : IntersectGroundPlane(ray);
        if (snapEnabled)
        {
            point.x = Mathf.Round(point.x / gridSize) * gridSize;
            point.z = Mathf.Round(point.z / gridSize) * gridSize;
        }

        if (previewInstance != null)
        {
            previewInstance.transform.SetPositionAndRotation(point, Quaternion.Euler(0f, previewRotationY, 0f));
            previewInstance.transform.localScale = Vector3.one * previewScale;
        }

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            PlacePart(point);
            e.Use();
            if (!keepPlacing)
                StopPlacing();
        }

        if (e.type == EventType.MouseMove)
            HandleUtility.Repaint();

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    private static Vector3 IntersectGroundPlane(Ray ray)
    {
        var plane = new Plane(Vector3.up, Vector3.zero);
        return plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : ray.origin;
    }

    private void PlacePart(Vector3 position)
    {
        var root = FindOrCreateMapRoot();
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(selectedPart, root.transform);
        instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, previewRotationY, 0f));
        instance.transform.localScale = Vector3.one * previewScale;
        Undo.RegisterCreatedObjectUndo(instance, "Place Map Part");
        Selection.activeGameObject = instance;
    }

    private static GameObject FindOrCreateMapRoot()
    {
        var root = GameObject.Find(MapRootName);
        if (root == null)
        {
            root = new GameObject(MapRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Map Root");
        }
        return root;
    }

    private void SnapSelectedToGrid()
    {
        foreach (var go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "Snap To Grid");
            var pos = go.transform.position;
            pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
            pos.z = Mathf.Round(pos.z / gridSize) * gridSize;
            go.transform.position = pos;
        }
    }

    private void HandleSelectedYShortcuts()
    {
        if (Selection.gameObjects.Length == 0)
            return;

        Event e = Event.current;
        if (e.type != EventType.KeyDown)
            return;

        if (e.keyCode == KeyCode.PageUp) { MoveSelectedY(yStepInput); e.Use(); Repaint(); }
        else if (e.keyCode == KeyCode.PageDown) { MoveSelectedY(-yStepInput); e.Use(); Repaint(); }
    }

    private void MoveSelectedY(float deltaY)
    {
        foreach (var go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "Move Map Part Y");
            var pos = go.transform.position;
            pos.y += deltaY;
            go.transform.position = pos;
        }
    }

    private void SetSelectedY(float absoluteY)
    {
        foreach (var go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "Set Map Part Y");
            var pos = go.transform.position;
            pos.y = absoluteY;
            go.transform.position = pos;
        }
    }

    private void RotateSelected(float deltaYDegrees)
    {
        foreach (var go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "Rotate Map Part");
            go.transform.Rotate(Vector3.up, deltaYDegrees, Space.World);
        }
    }

    private void SetSelectedRotation(Quaternion rotation)
    {
        foreach (var go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "Reset Map Part Rotation");
            go.transform.rotation = rotation;
        }
    }

    private void ScaleSelected(float deltaScale)
    {
        foreach (var go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "Scale Map Part");
            float current = go.transform.localScale.x;
            go.transform.localScale = Vector3.one * Mathf.Max(0.01f, current + deltaScale);
        }
    }

    private void SetSelectedScale(float absoluteScale)
    {
        foreach (var go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "Set Map Part Scale");
            go.transform.localScale = Vector3.one * absoluteScale;
        }
    }

    private void DeleteSelected()
    {
        foreach (var go in Selection.gameObjects)
            Undo.DestroyObjectImmediate(go);
    }

    private void UnpackSelected()
    {
        var roots = new HashSet<GameObject>();
        foreach (var go in Selection.gameObjects)
        {
            var outermost = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            roots.Add(outermost != null ? outermost : go);
        }

        foreach (var root in roots)
        {
            if (PrefabUtility.IsPartOfAnyPrefab(root))
                PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.UserAction);
        }
    }

    private void DrawMaterialsSection()
    {
        EditorGUILayout.LabelField("Materials");

        var renderers = Selection.gameObjects
            .SelectMany(go => go.GetComponentsInChildren<Renderer>())
            .Distinct()
            .ToList();

        if (renderers.Count == 0)
        {
            EditorGUILayout.HelpBox("No renderers in selection.", MessageType.None);
            return;
        }

        foreach (var renderer in renderers)
        {
            EditorGUILayout.LabelField(renderer.name, EditorStyles.miniBoldLabel);
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                EditorGUI.BeginChangeCheck();
                var newMaterial = (Material)EditorGUILayout.ObjectField(
                    $"Slot {i}", materials[i], typeof(Material), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(renderer, "Change Map Part Material");
                    var updated = renderer.sharedMaterials;
                    updated[i] = newMaterial;
                    renderer.sharedMaterials = updated;
                }
            }
        }
    }
}
