using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Build.Pipeline;

[CanEditMultipleObjects]
[CustomEditor(typeof(ModelLoadManager))]
public class TestEditor : Editor
{
    float f;
    bool _ismade = false;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ModelLoadManager _mLM = (ModelLoadManager)target;




        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Item Model"))
        {
            _mLM.ModelScavenge();
            _ismade = true;
        }
        GUILayout.EndHorizontal();



        GUILayout.BeginHorizontal();
        GUILayout.Label("모델 사이즈");
        if (_ismade)
            _mLM.ModelAdjust(EditorGUILayout.Slider(_mLM.EditorSliderReturn(0), 0, 15f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("모델 높이");
        if (_ismade)
            _mLM.ModelHeightAdjust(EditorGUILayout.Slider(_mLM.EditorSliderReturn(1), -15f, 15f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("콜라이더 높이");
        if (_ismade)
            _mLM.ColliderAdjustHeight(EditorGUILayout.Slider(_mLM.EditorSliderReturn(2), -15f, 15f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("콜라이더 가로 크기");
        if (_ismade)
            _mLM.ColliderAdjustX(EditorGUILayout.Slider(_mLM.EditorSliderReturn(3), 0, 15f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("콜라이더 세로 높이");
        if (_ismade)
            _mLM.ColliderAdjustY(EditorGUILayout.Slider(_mLM.EditorSliderReturn(4), 0, 15f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("콜라이더 가로 너비");
        if (_ismade)
            _mLM.ColliderAdjustZ(EditorGUILayout.Slider(_mLM.EditorSliderReturn(5), 0, 15f));
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label("아이템 이름");
        _mLM.MakeName(EditorGUILayout.TextField("Item", _mLM.ShowNamee()));
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label("아이템 설명");
        _mLM.MakeDesc(EditorGUILayout.TextField("Description", _mLM.ShowDesc()));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("아이템 생성") && _ismade)
            _mLM.GenerateItem();
        GUILayout.EndHorizontal();   
    }


}
