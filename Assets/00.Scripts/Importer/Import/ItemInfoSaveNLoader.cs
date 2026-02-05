using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;

public class ItemInfoSaveNLoader : MonoBehaviour
{
    public static ItemInfoSaveNLoader instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void SaveInfo(int num, string nam, string desc)
    {
        FileStream stram = new FileStream(Application.dataPath + "/03.Items/IIF.Json", FileMode.OpenOrCreate);
        JsonTest jtest1 = new JsonTest();
        jtest1.i = num;
        print(nam);
        jtest1.namee = new List<string> { };
        jtest1.description = new List<string> { };
        jtest1.namee.Add(nam);
        jtest1.description.Add(desc);
        string jsonData = JsonConvert.SerializeObject(jtest1);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        stram.Write(data, 0, data.Length);
        stram.Close();
    }
    public void LoadInfo(int num, out string nam, out string desc)
    {
        FileStream stram = new FileStream(Application.dataPath + "/03.Items/IIF.Json", FileMode.Open);
        byte[] data = new byte[stram.Length];
        stram.Read(data, 0, data.Length);
        stram.Close();
        string jsonData = Encoding.UTF8.GetString(data);
        JsonTest jtest2 = JsonConvert.DeserializeObject<JsonTest>(jsonData);
        jtest2.Print();
        nam = jtest2.namee[num];
        desc = jtest2.description[num];
    }
    public int LoadInfo()
    {
        FileStream stram = new FileStream(Application.dataPath + "/03.Items/IIF.Json", FileMode.Open);
        byte[] data = new byte[stram.Length];
        stram.Read(data, 0, data.Length);
        stram.Close();
        string jsonData = Encoding.UTF8.GetString(data);
        JsonTest jtest2 = JsonConvert.DeserializeObject<JsonTest>(jsonData);
        jtest2.Print();
        return jtest2.i;
    }
    public class JsonTest
    {
        public int i;
        public List<string> namee;
        public List<string> description;
        public JsonTest()
        {
            i = 23;
        }
        public void Print()
        {
            Debug.Log(i);
            Debug.Log(namee);
            Debug.Log(description);
        }
    }
}
