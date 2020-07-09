using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{

    public string coType;
    public string WDistance;
    public Vector3 position;
    public Dictionary<string, bool> contChamber;

    public void Create(string vertCoType, string chamb)
    {
        this.coType = vertCoType;
        contChamber = new Dictionary<string, bool>();
        foreach (string chamber in GenGroup.chambers)
        {
            contChamber.Add(chamber, false);
        }
        contChamber[chamb] = true;
    }

    public override string ToString()
    {
        string str = string.Empty;

        str += coType + ":";

        if (WDistance == "") str += "I";
        else str += WDistance;

        str += ":" + position.ToString("f6") + ":";
        foreach (KeyValuePair<string, bool> chamb in contChamber)
            if (chamb.Value) str += chamb.Key + ":";

        return str;
    }

}
