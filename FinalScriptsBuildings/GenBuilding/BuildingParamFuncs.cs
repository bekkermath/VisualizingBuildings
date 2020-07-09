using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuildingParamFuncs
{
    public static void SphA2()
    {
        GenBuilding building = 
            GameObject.Find("Building").GetComponent<GenBuilding>();

        building.roots = new Vector3[]{
            new Vector3( Mathf.Sqrt(3f) / 2f, 0, -1f / 2f),
            new Vector3(0, 0, 1f)
        };

        building.fundCoord = new Vector3[]
        {
            new Vector3(1f, 0, 0),
            new Vector3(1f / 2f, 0, Mathf.Sqrt(3f) / 2f),
            new Vector3(0, 0, 0)
        };

        building.AffineRoot = new bool[] { false, false };
    }

    public static Vector3 PosSphA2(float height, float maxheight,
        Transform fundVertTrans, string[] WDistSplit)
    {
        GenBuilding building =
            GameObject.Find("Building").GetComponent<GenBuilding>();

        Vector3 pos = new Vector3(0, (height - 1f) - (maxheight - 1f) / 2f, 0) +
                building.Reflect(fundVertTrans.localPosition, WDistSplit);

        return pos;
    }


    public static void AffA2()
    {
        GenBuilding building =
            GameObject.Find("Building").GetComponent<GenBuilding>();

        building.roots = new Vector3[]{
            new Vector3( Mathf.Sqrt(3f) / 2f, 0, -1f / 2f),
            new Vector3(0, 0, 1f),
            new Vector3(-Mathf.Sqrt(3f) / 2f, 0, -1f / 2f)
        };

        building.fundCoord = new Vector3[]
        {
            new Vector3(1f, 0, 0),
            new Vector3(1f / 2f, 0, Mathf.Sqrt(3f) / 2f),
            new Vector3(0, 0, 0)
        };

        building.AffineRoot = new bool[] { false, false, true };
    }

    public static Vector3 PosAffA2(float height, float maxheight,
        Transform fundVertTrans, string[] WDistSplit)
    {
        GenBuilding building =
            GameObject.Find("Building").GetComponent<GenBuilding>();

        Vector3 pos = new Vector3(0, (height - 1f) - (maxheight - 1f) / 2f, 0) +
                building.Reflect(fundVertTrans.localPosition, WDistSplit);

        return pos;
    }

    public static void SphA3()
    {
        GenBuilding building =
            GameObject.Find("Building").GetComponent<GenBuilding>();

        building.roots = new Vector3[]{
            new Vector3(0, 1f, 0),
            new Vector3(0, -1f / 2f, -Mathf.Sqrt(3f) / 2f),
            new Vector3(Mathf.Sqrt(2f / 3f), 0, Mathf.Sqrt(3f) / 3f)
        };

        building.fundCoord = new Vector3[]
        {
            new Vector3( 1f / 3f, Mathf.Sqrt(6f) / 3f, -Mathf.Sqrt(2f) / 3f),
            new Vector3(Mathf.Sqrt(3f) / 3f, 0, -Mathf.Sqrt(6f) / 3f),
            new Vector3(1f, 0, 0)
        };

        building.AffineRoot = new bool[] { false, false, false };
    }

    public static Vector3 PosSphA3(float height, float maxheight,
    Transform fundVertTrans, string[] WDistSplit)
    {
        GenBuilding building =
            GameObject.Find("Building").GetComponent<GenBuilding>();

        Vector3 pos = height *
                building.Reflect(fundVertTrans.localPosition, WDistSplit);

        return pos;
    }
}
