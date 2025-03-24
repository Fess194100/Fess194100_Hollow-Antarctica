using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class DataSave
{
    [SerializeField]
    public int LTotalCoin = 0;
    
    [SerializeField]
    public float LTotalFuel = 1000f;
    
    [SerializeField]
    public bool LAutoRecenter = false;
    
    [SerializeField]
    public string[] LStringArray = new string[32];

    /*[System.NonSerialized]
    public Color colorHead1 = Color.white;

    [SerializeField]
    public float ColorHead1R = 1f, ColorHead1G = 1f, ColorHead1B = 1f;*/


    public void GetData()
    {
        string filePath = Application.persistentDataPath + "/saveData.json";

        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                DataSave save = JsonUtility.FromJson<DataSave>(json);

                LTotalCoin = save.LTotalCoin;
                LTotalFuel = save.LTotalFuel;
                LAutoRecenter = save.LAutoRecenter;
                LStringArray = save.LStringArray;

                Debug.Log("Данные успешно загружены.");
            }
            else
            {
                Debug.Log("Файл сохранения не найден. Создание новых данных.");
                ResetData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Ошибка при загрузке данных: " + e.Message);
            ResetData();
        }
    }

    public void SetData()
    {
        DataSave save = new DataSave
        {
            LTotalCoin = LTotalCoin,
            LTotalFuel = LTotalFuel,
            LAutoRecenter = LAutoRecenter,
            LStringArray = LStringArray
        };

        string json = JsonUtility.ToJson(save, true); // true для красивого форматирования
        string filePath = Application.persistentDataPath + "/saveData.json";
        File.WriteAllText(filePath, json);

        Debug.Log("Данные успешно сохранены.");
    }

    public void ResetData()
    {
        LTotalCoin = 0;
        LTotalFuel = 1000f;
        LAutoRecenter = false;
        LStringArray = new string[32];

        SetData();
    }
}