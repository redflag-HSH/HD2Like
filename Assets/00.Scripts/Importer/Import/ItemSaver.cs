using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class ItemSaver : MonoBehaviour
{
    public string ObjName, ObjInfo;
    public int _objCode;
    public void GenerateItem()
    {
        //최후 오브젝트 코드 스크립터블에서 불러오기
        _objCode = ItemInfoSaveNLoader.instance.LoadInfo();
        _objCode += 1;
        //이름, 정보 저장
        //JSon으로 저장
        GetComponent<ItemInfoSaveNLoader>().SaveInfo(_objCode, ObjName, ObjInfo);

        //아이템 머테리얼 어드레서블에 추가
        AssetDatabase.CreateAsset(GetComponent<ModelLoadManager>().ModelMaterial, "Assets/03.Items/ItemMaterials/" + "IC" + _objCode + ".mat");
        /* AddressableAssetGroup ig = AddressableAssetSettingsDefaultObject.Settings.FindGroup("ItemsImsi");
         var infoguid = AssetDatabase.AssetPathToGUID("Assets/03.Items/Materials");
         //This is the function that actually makes the object addressable
         var infoentry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(infoguid, ig);
         infoentry.address = "Assets/ItemMaterials/" + "IC" + _objCode;
         //You'll need these to run to save the changes!
         AddressableAssetSettingsDefaultObject.Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, infoentry, true);
         AssetDatabase.SaveAssets();*/

        //아이템 모델을 판정의 자식으로
        GetComponent<ModelLoadManager>().ModelObject.transform.parent = GetComponent<ModelLoadManager>().PhysicCollider.transform;
        //itembase 스크립트 추가 
        GetComponent<ModelLoadManager>().PhysicCollider.AddComponent<BasicItem>();
        //판정의 이름을 아이템 코드로
        GetComponent<ModelLoadManager>().PhysicCollider.name = "IC" + _objCode;
        //아이템 판정의 머테리얼을 비활성화
        GetComponent<ModelLoadManager>().PhysicCollider.GetComponent<Renderer>().enabled = false;
        //아이템 모델 및 판정 프리팹 화
        PrefabUtility.SaveAsPrefabAsset(GetComponent<ModelLoadManager>().PhysicCollider, Application.dataPath + "/03.Items/ItemObjects/" + "IC" + _objCode + ".prefab");
        //아이템 어드레서블에 추가
        //Make a gameobject an addressable
        AddressableAssetGroup g = AddressableAssetSettingsDefaultObject.Settings.FindGroup("ItemsImsi");
        var guid = AssetDatabase.AssetPathToGUID("Assets/03.Items/ItemObjects");
        //This is the function that actually makes the object addressable
        var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, g);
        entry.address = "Assets/ItemObjects/" + "IC" + _objCode;
        //You'll need these to run to save the changes!
        AddressableAssetSettingsDefaultObject.Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        AssetDatabase.SaveAssets();
    }
}
