using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public static class Utility 
{




    #region Waitfor
    public static readonly WaitForEndOfFrame WaitFrame = new WaitForEndOfFrame();
    public static readonly WaitForSeconds Wait001 = new WaitForSeconds(0.01f);
    public static readonly WaitForSeconds Wait01 = new WaitForSeconds(0.1f);
    public static readonly WaitForSeconds Wait02 = new WaitForSeconds(0.2f);
    public static readonly WaitForSeconds Wait05 = new WaitForSeconds(0.5f);
    public static readonly WaitForSeconds Wait1 = new WaitForSeconds(1);
    public static readonly WaitForSeconds Wait2 = new WaitForSeconds(2);
    public static readonly WaitForSeconds Wait5 = new WaitForSeconds(5);
    

    #endregion
    #region 정렬
    public static void QuickSort(List<List<string>> array)
    {
        var p = 0;
        var r = array.Count - 1;
        if(p<r)
        {
            var q = Partition(array, p, r);
            QuickSort(array, p, q - 1);
            QuickSort(array, q + 1, r);
        }
    }
    static void QuickSort(List<List<string>> array,int p, int r)
    {
        if (p < r)
        {
            var q = Partition(array, p, r);
            QuickSort(array, p, q - 1);
            QuickSort(array, q + 1, r);
        }
    }
    static int Partition(List<List<string>> array, int p ,int r)
    {
        var q = p;
        for(int j=p;j<r;j++)
        {
            if (float.Parse(array[j][1]) >=float.Parse(array[r][1]))
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
        "a", 
        "b",
        "c",
        "d"

    };

    private static readonly int devicevAlue = 3;
    private static readonly float  powv= Mathf.Pow(10, devicevAlue);
    public static string DoubleToString(this double value)
    {
        if (value <= powv)
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
            else
            {
                
                count++;
                a *= powv;
                if (count - 1 > Unit.Length)
                {
                    
                    count--;
                    a /= powv;
                    return Math.Truncate((value / (a / powv))*100)/100 + Unit[count - 2];
                }
            }
        }
        return Math.Truncate((value / (a / powv))*100)/100+Unit[count-2];
    }



    #endregion
    #region Time

    public static string Time_MinuteToTime(int minute)
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

    public static string Time_SecendToTime(int secend)
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
    
    
}
