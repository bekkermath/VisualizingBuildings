using UnityEngine;
using System.Globalization;

public class Chamber : MonoBehaviour
{
    public string chamberName;
    public string WDistance;
    public int[] rootValues;
    public double height;

    public Transform[] vertices;
    public Transform triangle;
    public Transform[] lines;
    public Transform vertexPrefab;

    public void Create(string chamberName)
    {
        this.vertices = new Transform[3];
        lines = new Transform[3];

        this.rootValues = new int[6];
        this.chamberName = chamberName;
    }

    public override string ToString()
    {
        string str = string.Empty;

        str += chamberName + ":" + WDistance + ":";

        foreach (int rootValue in rootValues)
            str += rootValue.ToString("f6", new CultureInfo("en-US")) + ":";

        str += height.ToString("f6", new CultureInfo("en-US"));

        return str;
    }
}
