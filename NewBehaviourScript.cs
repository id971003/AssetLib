using System;
using System.Collections.Generic;

public static class ListSorter
{
    public static void SortAndPrint<T, TKey>(List<T> list, Func<T, TKey> keySelector)
    {
        list.Sort((item1, item2) => Comparer<TKey>.Default.Compare(keySelector(item1), keySelector(item2)));

        // 정렬 후 출력
        Console.WriteLine($"정렬 결과 (기준: {keySelector.Method.Name}):");
        PrintList(list);
    }

    private static void PrintList<T>(List<T> list)
    {
        foreach (var item in list)
        {
            Console.WriteLine(item);
        }
        Console.WriteLine();
    }
}

// 사용자 지정 클래스 정의
class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public decimal Money { get; set; }

    public override string ToString()
    {
        return $"Name: {Name}, Age: {Age}, Money: {Money}";
    }
}

class Program
{
    static void Main()
    {
        // 예제를 위한 List 생성
        List<Person> people = new List<Person>
        {
            new Person { Name = "Alice", Age = 30, Money = 1000 },
            new Person { Name = "Bob", Age = 25, Money = 1500 },
            new Person { Name = "Charlie", Age = 35, Money = 1200 },
            new Person { Name = "David", Age = 30, Money = 800 }
        };

        // Age로 정렬
        ListSorter.SortAndPrint(people, p => p.Age);

        // Money로 정렬
        ListSorter.SortAndPrint(people, p => p.Money);
    }
}