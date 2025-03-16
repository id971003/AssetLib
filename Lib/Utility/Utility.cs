/*
made 7rzr 2022-1-7
update 2023-12-29
쓸만한것들 모아논것
1. WaitForSeconds
    코루틴할때 waitforsecond 나 딜레이를 주는 경우가 많은데
    이거 미리 캐싱해서 이용한다. 미리 만들어 놓으면 new 를 덜 호출해 메모리적으로 안정적이라고한다.

2. SortListSomthingValue
    어떤 타입의 리스트가 있을때 그 리스트를 어떤 값을 기준으로 정렬하는 메서드다

3. ValueToUnit
    수에 자리수 넣기 ( ex 123456 > 1.23만) 소숫점 2개까지 나오게해줌 
    세팅
        devicevAlue : 몇자리로 끊을껀지 [보통 4개겠지?]
        Unit : 단위수 만,억,조 등등... 넘어가면 123456억 이렇게나옴
        
4. Time
    Time_MinuteToTime : 364분 > 6시간 4분
    Time_SecendToTime : 364초 > 6분 4초
    
5. WeekDay
   DateTime 기준으로 다음주 월요일 계산 [LastDateOfWeek 의 addday 6이면 월요일 7이면 화요일 ...]
   string type 으로 반화해 계산
   
6. C_ReSize_StaticSizeObjectNearBatch
    재화이미지에 수량이 표기될때 둘을 가운데 정렬하는  친구임
    인자 엔 값, 왼쪽 이미지 , 오른쪽 text 가 들어감
    세팅
        이미지와 텍스트는 앵커값조절x[0.5고정]
        텍스트엔 contantsizefitter 넣고 horizontalfit 값은 preferred size
    contantsizefitter 이용하다보니 값이 들어가고 다음프레임에서 위치 조절이 일어나야함 그래서 코루틴으로 다음프레임 실행

*/


using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;


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

    #region SortListSomthingValue
    public static void SortListSomthingValue<T, TKey>(List<T> list, Func<T, TKey> keySelector)
    {
        //오름차순
        list.Sort((item1, item2) => Comparer<TKey>.Default.Compare(keySelector(item1), keySelector(item2)));
        
        //내림차순
        //list.Sort((item1, item2) => Comparer<TKey>.Default.Compare(keySelector(item2), keySelector(item1))); 
    }
    #endregion
    
    #region ValueToUnit
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
    public static string ValueToUnit(this float value) 
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
    
    #region WeekDay
    public static int WeekDay(DateTime date)
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

    #region C_ReSize_StaticSizeObjectNearBatch
    public static IEnumerator C_ReSize_StaticSizeObjectNearBatch(string TargetText,Image leftImage,Text rightText)
    {
        rightText.text = TargetText;
        yield return WaitFrame;
        if (rightText.rectTransform.sizeDelta.x <= leftImage.rectTransform.sizeDelta.x) //텍스트 크기가 이미지 크키보다 작거나 같을때
        {
            var sizeDelta = leftImage.rectTransform.sizeDelta;
            rightText.rectTransform.localPosition = new Vector2(sizeDelta.x/2, rightText.rectTransform.localPosition.y);
            leftImage.rectTransform.localPosition = new Vector2(-sizeDelta.x / 2,
                leftImage.rectTransform.localPosition.y);
        }
        else //텍스트가 더 길때
        {
            var totalsize = (rightText.rectTransform.sizeDelta.x + leftImage.rectTransform.sizeDelta.x)/2;
            rightText.rectTransform.localPosition = new Vector2(leftImage.rectTransform.sizeDelta.x/2, rightText.rectTransform.localPosition.y);
            leftImage.rectTransform.localPosition = new Vector2(-rightText.rectTransform.sizeDelta.x/2, leftImage.rectTransform.localPosition.y);
        }
    }
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

    public static T GetRandom<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new InvalidOperationException("Cannot get a random item from an empty or null list.");
        }

        int index = UnityEngine.Random.Range(0, list.Count);
        return list[index];
    }

    public static int GetRandomIndex<T>(this List<T> list)
    {
        if (list.Count == 0)
            return -1;

        return UnityEngine.Random.Range(0, list.Count);
    }

    public static T SelectByWeight<T>(this IList<T> elements, int weightSum, Func<T, int> getElementWeight)
    {
        int index = elements.SelectIndexByWeight(weightSum, getElementWeight);
        return elements[index];
    }
    public static int SelectIndexByWeight<T>(this IList<T> elements, int weightSum, Func<T, int> getElementWeight)
    {
        UnityEngine.Debug.Assert(weightSum > 0, "WeightSum should be a positive value");

        int selectionIndex = 0;
        int selectionWeightIndex = UnityEngine.Random.Range(0, weightSum);
        int elementCount = elements.Count;

        Debug.Assert(elementCount != 0, "Cannot perform selection on an empty collection");

        int itemWeight = getElementWeight(elements[selectionIndex]);
        while (selectionWeightIndex >= itemWeight)
        {
            selectionWeightIndex -= itemWeight;
            selectionIndex++;

            Debug.Assert(selectionIndex < elementCount, "Weighted selection exceeded indexable range. Is your weightSum correct?");

            itemWeight = getElementWeight(elements[selectionIndex]);
        }

        return selectionIndex;
    }

}
