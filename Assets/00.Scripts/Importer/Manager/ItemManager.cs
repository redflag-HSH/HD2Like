using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;
    public int cc;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            this.enabled = false;

        cc = ItemInfoSaveNLoader.instance.LoadInfo();
    }

    public void ItemGenerate(Vector3 gePos, float Gradius, int Gcount)
    {
        for (int i = 0; i < Gcount; i++)
        {
            float[] ran = new float[3];
            for (int j = 0; j < 3; j++)
                ran[j] = Random.Range(-Gradius, Gradius);

            string nam, info;
            int l = Random.Range(0, cc + 1);
            ItemInfoSaveNLoader.instance.LoadInfo(l, out nam, out info);
            var it = Addressables.InstantiateAsync("IC" + l);
            GameObject item = it.Result;
            item.transform.position = new Vector3(gePos.x + ran[0], gePos.y + ran[1], gePos.z + ran[2]);
            //아이템 이름, 정보 입력
            item.GetComponent<ItemBase>().GetInfo(nam, info);
        }
    }
}
