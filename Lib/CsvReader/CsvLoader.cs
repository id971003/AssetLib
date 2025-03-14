using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;


[Serializable]
public class GameCsvDataMap : ClassMap<GameCsvData>
{
    public GameCsvDataMap()
    {
        Map(m => m.type).Name("type");
        
        Map(m => m.pName).Name("pName");
        Map(m => m.pClass).Name("pClass");
        Map(m => m.text).Name("text");
        
        Map(m => m.textColor).Name("textColor");
        
        Map(m => m.voiceN).Name("voiceN");
    }
}

public class GameCsvData
{
    public string type;
    
    public string pName;
    public string pClass;
    public string text;
    
    public string textColor;

    public string voiceN;
}
