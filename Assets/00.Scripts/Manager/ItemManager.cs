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

    }

    public void ItemGenerate(Vector3 gePos, float Gradius, int Gcount)
    {
        for (int i = 0; i < Gcount; i++)
        {
            float[] ran = new float[3];
            for (int j = 0; j < 3; j++)
                ran[j] = Random.Range(-Gradius, Gradius);

            var it = Addressables.InstantiateAsync("IC");
            GameObject item = it.Result;
            item.transform.position = new Vector3(gePos.x + ran[0], gePos.y + ran[1], gePos.z + ran[2]);
        }
    }
    public void ItemGenerate(Vector3 gePos, float Gradius, int Gcount, int itemCode)
    {
        for (int i = 0; i < Gcount; i++)
        {
            float[] ran = new float[3];
            for (int j = 0; j < 3; j++)
                ran[j] = Random.Range(-Gradius, Gradius);

            var it = Addressables.InstantiateAsync("IC");
            GameObject item = it.Result;
            item.transform.position = new Vector3(gePos.x + ran[0], gePos.y + ran[1], gePos.z + ran[2]);
        }
    }
}
