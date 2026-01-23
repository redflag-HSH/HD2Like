using System;
using System.IO;
using Unity.Mathematics;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
public class ModelLoadManager : MonoBehaviour
{
    public GameObject PhysicCollider;
    public GameObject ModelObject;
    public Material ModelMaterial;
    [SerializeField] Material _ubematerial;

    // Update is called once per frame
    void Update()
    {

    }
    public void GenerateItem()
    {
        GetComponent<ItemSaver>().GenerateItem();
    }
    public void ModelScavenge()
    {
        //폴더 확인

        System.IO.DirectoryInfo di = new DirectoryInfo("Assets/Resources/Models");

        foreach (FileInfo file in di.GetFiles())
        {
            //모델명 확인
            Debug.Log("파일명 : " + file.Name);
            Debug.Log("파일 주소 : " + file.Directory);
            //가장 먼저 IC+숫자 아닌 이름을 가진 모델 파일 불러오기
            if (file.Name.IndexOf("IC", 0, 2) == -1)
            {
                //큐브 생성
                //프리미티브 큐브 생성
                GameObject cubee = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cubee.transform.position = new Vector3(0, .8f, -4);
                cubee.GetComponent<Renderer>().material = _ubematerial;
                //자식으로 모델 파일 생성
                //print(file.Name);
                string nameee = Path.GetFileNameWithoutExtension(file.Name);
                ModelObject = Instantiate(Resources.Load("Models/" + nameee), new Vector3(0, .8f, -4), quaternion.identity) as GameObject;
                //모델 파일 삭제
                AssetDatabase.DeleteAsset("Assets/Models/" + nameee);
                ModelObject.transform.localScale = new Vector3(5, 5, 5);
                //ModelObject = t.gameObject;
                PhysicCollider = cubee;
                break;
            }
        }
        //머테리얼 관련 스크립트
        di = new DirectoryInfo("Assets/Resources/Textures");
        foreach (FileInfo file in di.GetFiles())
        {
            if (file.Name.IndexOf("IC", 0, 2) == -1)
            {
                //머테리얼 생성
                Material material = new Material(Shader.Find("Unlit/Texture"));
                //머테리얼의 텍스쳐 입히기
                string nameee = Path.GetFileNameWithoutExtension(file.Name);
                material.mainTexture = (Texture2D)Resources.Load("Textures/" + nameee);
                Debug.Log(material.mainTexture);
                ModelMaterial = material;

                ModelObject.GetComponentInChildren<Renderer>().material = material;
                break;
            }
        }

        //머테리얼 파일 삭제
    }
    public void ColliderAdjustX(float scale)
    {
        PhysicCollider.transform.localScale = new Vector3(scale, PhysicCollider.transform.localScale.y, PhysicCollider.transform.localScale.z);
    }
    public void ColliderAdjustY(float scale)
    {
        PhysicCollider.transform.localScale = new Vector3(PhysicCollider.transform.localScale.x, scale, PhysicCollider.transform.localScale.z);
    }
    public void ColliderAdjustZ(float scale)
    {
        PhysicCollider.transform.localScale = new Vector3(PhysicCollider.transform.localScale.x, PhysicCollider.transform.localScale.y, scale);
    }
    public void ColliderAdjustHeight(float scale)
    {
        PhysicCollider.transform.position = new Vector3(0, 0.8f + scale, -4);
    }

    public void ModelAdjust(float scale)
    {
        ModelObject.transform.localScale = new Vector3(scale, scale, scale);
    }
    public void ModelHeightAdjust(float scale)
    {
        ModelObject.transform.position = new Vector3(0, 0.8f + scale, -4);
    }
    public float EditorSliderReturn(int num)
    {
        float scale = -122;
        switch (num)
        {
            //모델 사이즈
            case 0:
                scale = ModelObject.transform.localScale.x;
                break;
            //모델 높이
            case 1:
                scale = ModelObject.transform.position.y - .8f;
                break;
            //판정 높이
            case 2:
                scale = PhysicCollider.transform.position.y - .8f;
                break;
            //판정 가로 크기
            case 3:
                scale = PhysicCollider.transform.localScale.x;
                break;
            //판정 높이 크기
            case 4:
                scale = PhysicCollider.transform.localScale.y;
                break;
            //판정 세로 크기
            case 5:
                scale = PhysicCollider.transform.localScale.z;
                break;

        }
        return scale;
    }
    public void ScaeItem()
    {
        GetComponent<ItemSaver>().GenerateItem();
    }
    public void MakeDesc(string namee)
    {
        GetComponent<ItemSaver>().ObjInfo = namee;
    }
    public string ShowDesc()
    {
        return GetComponent<ItemSaver>().ObjInfo;
    }
    public string ShowNamee()
    {
        return GetComponent<ItemSaver>().ObjName;
    }
    public void MakeName(string namee)
    {
        GetComponent<ItemSaver>().ObjName = namee;
    }
}
