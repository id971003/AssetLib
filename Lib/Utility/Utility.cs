/*
made 7rzr 2022-1-7
update 2023-12-29
쓸만한것들 모아논것
1.
*/


using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.UIElements;


public static class Utility 
{
    #region Waitfor
    public static readonly WaitForNextFrameUnit WaitFrame = new WaitForNextFrameUnit();
    public static readonly WaitForSeconds Wait001 = new WaitForSeconds(0.01f);
    public static readonly WaitForSeconds Wait01 = new WaitForSeconds(0.1f);
    public static readonly WaitForSeconds Wait02 = new WaitForSeconds(0.2f);
    public static readonly WaitForSeconds Wait03 = new WaitForSeconds(0.3f);
    public static readonly WaitForSeconds Wait05 = new WaitForSeconds(0.5f);
    public static readonly WaitForSeconds Wait005 = new WaitForSeconds(0.05f);
    public static readonly WaitForSeconds Wait1 = new WaitForSeconds(1);
    public static readonly WaitForSeconds Wait2 = new WaitForSeconds(2);
    public static readonly WaitForSeconds Wait3 = new WaitForSeconds(3);
    public static readonly WaitForSeconds Wait4 = new WaitForSeconds(4);
    public static readonly WaitForSeconds Wait5 = new WaitForSeconds(5);
    
    
    #endregion
    #region 정렬
    public static void QuickSort(List<List<string>> array,int comIndex=1)
    {
        var p = 0;
        var r = array.Count - 1;
        if(p<r)
        {
            var q = Partition(array, p, r);
            QuickSort(array, p, q - 1,comIndex);
            QuickSort(array, q + 1, r,comIndex);
        }
    }
    static void QuickSort(List<List<string>> array,int p, int r,int comIndex=1)
    {
        if (p < r)
        {
            var q = Partition(array, p, r,comIndex);
            QuickSort(array, p, q - 1);
            QuickSort(array, q + 1, r);
        }
    }
    static int Partition(List<List<string>> array, int p ,int r,int comIndex=1)
    {
        var q = p;
        for(int j=p;j<r;j++)
        {
            if (float.Parse(array[j][comIndex]) >=float.Parse(array[r][comIndex]))
            {
                Swap(array, q, j);
                q++;
            }
        }
        Swap(array, q, r);
        return q;
    }
    static void Swap(List<List<string>> array, int beforeIndex,int afterIndex)
    {
        (array[beforeIndex], array[afterIndex]) = (array[afterIndex], array[beforeIndex]);
    }
    

    #endregion
    #region ValueTounit 꼬리표
    public static readonly string[] Unit =
    {
        "만",
        "억",
        "조",
        "경",
        "해",
        "자",
        "양",
        "구",
        "간",
        "극",
        "항하사"
    };

    private static readonly int devicevAlue = 4;
    private static readonly float  powv= Mathf.Pow(10, devicevAlue);
    public static string DoubleToString(this float value) //꼬리 2개
    {
        if (value == 0)
        {
            return "0";
        }
        if (value < 1)
        {
            return value.ToString("N1");
        }
        if (value < powv)
        {
            return value.ToString("#,##0");
        }

        double a = powv;
        int count = 1;
        while (true)
        {
            if (value < a)
            {
                break;
            }

            count++;
            a *= powv;
            if (count - 1 > Unit.Length)
            {
                count--;
                a /= powv;
                return Math.Truncate((value / (a / powv)) * 100) / 100 + Unit[count - 2];
            }
        }

        return Math.Truncate((value / (a / powv)) * 100) / 100 + Unit[count - 2];
    }
    public static string DoubleToString_Value(this string value) //꼬리 2개 표시할꺼
    {
        return "X" + DoubleToString(float.Parse(value));
    }
    public static string DoubleToString_Value(this float value) //표시할꺼
    {
        return "X" + DoubleToString(value);
    }

    #endregion
    #region Time

    public static string Time_MinuteToTime(this int minute)
    {
        if (minute >= 60)
        {
            var a = minute / 60;
            var b = minute % 60;
            string result=a+"시간";

            if (b != 0)
            {
                result += b + "분";
            }

            return result;
        }
        else
        {
            return minute + "분";
        }
               
    }

    public static string Time_SecendToTime(this int secend)
    {
        string result="";
        var remainTime = secend % 60;
        if (secend >= 60)
        {
            
            var minute = secend / 60;
            result += Time_MinuteToTime(minute);
            if (remainTime != 0)
                result += remainTime + "초";
        }
        else
        {
            result = secend + "초";
        }

        return result;
    }
    #endregion
    
    public static string[] DataTextToStringList(string Asset,char a)
    {
        return Asset.Split(a);
    }

    /*
        사이즈 다른 2개를 정사이즈로 배치
        다음 프레임에서 해야해서 코루틴으로함 contantsizefitter
        오른쪽에 텍스트배치[크기 움직이는친구]
        조건
        1.2개다 acnchor 건들지말고 [0.5고정] y 크기 위치 맞추고 딱 붙혀놓고
        2.가변[보통 text 갯지?] 인친구에 content size fitter 넣어놓기
     */
    public static void ReSize_StaticSizeObjectNearBatch(RectTransform left,RectTransform right) 
    {

        if (right.sizeDelta.x <= left.sizeDelta.x) //텍스트 크기가 이미지 크키보다 작거나 같을때
        {
            var sizeDelta = left.sizeDelta;
            right.localPosition = new Vector2(sizeDelta.x/2, right.localPosition.y);
            left.localPosition = new Vector2(-sizeDelta.x / 2,
                left.localPosition.y);
        }
        else //텍스트가 더 길때
        {
            var totalsize = (right.sizeDelta.x + left.sizeDelta.x)/2;
            right.localPosition = new Vector2(left.sizeDelta.x/2, right.localPosition.y);
            left.localPosition = new Vector2(-right.sizeDelta.x/2, left.localPosition.y);
            
        }
    }

    public static IEnumerator AfterFrame(Action afterProcess)
    {
        yield return WaitFrame;
        afterProcess?.Invoke();
    }
    
    #region 이번게임만
    
    public static string Main_RePlaceInfo_CharacterInFoDamage(string info, float damage,int attackCound) //스킬설명 바꾸기
    {
        return info.Replace("D",  "("+Utility.DoubleToString(damage)+"% X"+attackCound+")로\n");
    }

    public static float Main_Return_CharacterInfoDamageTreasuer(float FirstData, float level) // 스킬레벨, 스킬데미지로 바꾸기
    {
        return FirstData + level * 0.1f * FirstData;
    }

    public static string Main_Return_CahracterInfoStory(string data)
    {
        string dd= data.Replace("A", "\n");
        return dd.Replace("B", "\n");
    }

    public static float Main_Return_DootResetCoast(float UsingPoint)
    {
        if (UsingPoint < 10)
            return 1;
        else if (UsingPoint < 20)
            return 3;
        else if (UsingPoint < 50)
            return 10;
        else if (UsingPoint < 100)
            return 30;
        else
        {
            return 100;
        }
    }

    public static string Return_ChapterScore(float Level)
    {
        int floatlevel = (int)Level;
        string chapter = ((floatlevel / 30)+1).ToString();
        string Deficult = ((floatlevel % 30)+1).ToString();
        return chapter + "-" + Deficult;
    }
    #region main 쪽 표시되는거
    public static float Return_Damage(float var)
    {
        return var;
    }
    public static float Return_CoolTime(float var) 
    {
        return ((1 - (var / (var + 50))));
    }

    public static float Return_CriDamage(float var)
    {
        return var;
    }
    public static float Return_CirPerCent(float var) 
    {
        return (1 - (40 / (40 + var))) * 100;
    }
    public static float Return_Damage_Ui(float var)
    {
        return var;
    }
    public static string Return_CriDamage_Ui(float var)
    {
        return var*100+"%";
    }

    public static string Return_Cooltime_Ui(float var) //표시될값
    {
        return ((1 - ((1 - (var / (var + 50))))) * 100).ToString("N1")+"%";
    }

    public static string Return_CriPercent_Ui(float var) //표시될 값
    {
        return ((1 - (40 / (40 + var))) * 100).ToString("N1")+"%";
    }
    #endregion
    //ui 표시
    
    #region  주차구하기
    public static int Return_RateWeekData(DateTime date)
    {
        DateTime a = date.LastDateOfWeek();

        int year = a.Year * 10000;
        int month;
        if (a.Month < 10)
        {
            month = int.Parse("0" + a.Month) * 100;
        }
        else
        {
            month = a.Month * 100;
        }

        int day;
        if (a.Day < 10)
        {
            day = int.Parse("0" + a.Day);
        }
        else
        {
            day = a.Day;
        }
        return year + month + day;
    }
    #endregion
    #endregion


}

public static class DateTimeExt
{
    //이번달의 첫번째 일
    public static DateTime FirstDateOfMonth(this DateTime dateTime)
    {
        return dateTime.AddDays(1 - dateTime.Day);
    }
    //이주의 첫번째 일  기준 일로부터
    public static DateTime FirstDateOfWeek(this DateTime dateTime, DayOfWeek ruleDayOfWeek = DayOfWeek.Tuesday)
    {
        return dateTime.AddDays(-((dateTime.DayOfWeek + 7 - ruleDayOfWeek) % 7));
    }

    //이주의 마지막 일 기준일로부터
    public static DateTime LastDateOfWeek(this DateTime dateTime, DayOfWeek ruleDayOfWeek = DayOfWeek.Tuesday)
    {
        return dateTime.FirstDateOfWeek(ruleDayOfWeek).AddDays(6);
    }
}


public static class ListExt
{
    // O(1) 
    public static void RemoveBySwap<T>(this List<T> list, int index)
    {
        if (list.Count<=0)
        {
            return;
        }

        list[index] = list[list.Count - 1];
        
        list.RemoveAt(list.Count - 1);
    }

    // O(n)
    public static void RemoveBySwapItem<T>(this List<T> list, T item)
    {
        if (list.Count<=0)
        {
            return;
        }
        int index = list.IndexOf(item);
        RemoveBySwap(list, index);
    }
}
