using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace FoundryBlazor.Extensions;

/// <summary>
/// RandomName class, used to generate a random name.
/// </summary>
public class MockDataMaker
{
     readonly Random Rand;
     readonly List<string> FirstNames;
     readonly List<string> LastNames;
     readonly List<string> Words;

    public MockDataMaker()
    {
        var first = new string[]
        {
            "Steve","Evan","Deb","David"
        };
        var last = new string[]
        {
            "North", "South", "East","West","Earth","Wind","Fire","Water"
        };

        Rand = new Random();
        FirstNames = new List<string>(first);
        LastNames = new List<string>(last);

        var data = "tortor risus dapibus augue vel accumsan tellus nisi eu orci mauris lacinia sapien quis libero nullam sit amet turpis elementum ligula vehicula consequat morbi a ipsum integer a nibh in quis justo maecenas rhoncus aliquam lacus morbi quis tortor id nulla ultrices aliquet maecenas leo odio condimentum id luctus nec molestie sed justo pellentesque viverra pede ac diam cras pellentesque volutpat dui maecenas tristique est et tempus semper est quam pharetra magna ac consequat metus sapien ut nunc vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae mauris viverra diam vitae quam suspendisse potenti nullam".Split(" ");
        Words = new List<string>(data);
    }

    public string GenerateName()
    {
        string first = FirstNames[Rand.Next(FirstNames.Count)];
        string last = LastNames[Rand.Next(LastNames.Count)];

        return $"{first}_{last}";
    }

    public string GenerateText()
    {
        var list = new List<string>();
        for (int i = 0; i < GenerateInt(5, 12); i++)
        {
            string word = Words[Rand.Next(Words.Count)];
            list.Add(word);
        }

        return string.Join(" ", list);
    }

    public string GenerateWord()
    {
        string word = Words[Rand.Next(Words.Count)];
        return word;
    }

    public double GenerateDouble(double min = 0.0, double max = 1.0)
    {
        return min + (max - min) * Rand.NextDouble();
    }
    public int GenerateInt(int min = 0, int max = 1)
    {
        return Rand.Next(min, max);
    }
}

