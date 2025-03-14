/*
[Serializable]
public class TestData
{
    public int Level;
    public String Name;
}
[Serializable]
public class TestDataMap : ClassMap<GachaDataGameData>
{
    public GachaDataGameDataMap()
    {
Map(m => m.Level).Name("Level");
Map(m => m.Name).Name("Name").Default("123");;

    }
}
*/


using CsvHelper;
using CsvHelper.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;



public class CsvManager : MonoBehaviour
{
    public static CsvManager inst;

    private Dictionary<string, TextAsset> _csvFiles = new Dictionary<string, TextAsset>();

    public string _path;

    private void Awake()
    {
        if (inst == null)
        {
            inst = this;
            
            inst.SetCsvData();
            
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (inst != this) Destroy(gameObject);
        }
    }
    private void SetCsvData()
    {
        _csvFiles.Clear();
        
        TextAsset[] texts = Resources.LoadAll<TextAsset>(_path); //PATH
        foreach (var txt in texts)
        {
            _csvFiles.Add(txt.name, txt);
        }
    }
    public List<T> LoadCsvData<T, TMap>(string fileName) where TMap : ClassMap<T>
    {
        
        List <T> dataList = new List<T>();
        TextAsset csvFile = GetTextAsset(fileName);
        if (csvFile != null)
        {
            using (var reader = new StringReader(csvFile.text))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                
                csv.Context.RegisterClassMap<TMap>();
                dataList = csv.GetRecords<T>().ToList();
                Debug.Log($"Loaded {dataList.Count} records from {fileName} CSV");
            }
        }
        else
        {
            Debug.LogError($"CSV file not found with name: {fileName}");
        }

        return dataList;
    }

    public TextAsset GetTextAsset(string fileName) => _csvFiles[fileName];

    public string[] GetFileRelatedKeys(string str) => _csvFiles.Keys.Where(key => key.Contains(str)).ToArray();
}
