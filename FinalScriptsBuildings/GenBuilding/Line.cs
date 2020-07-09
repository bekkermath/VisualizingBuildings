using System.Collections;
using UnityEngine;


public class Line : MonoBehaviour
{

    public GameObject vertexOne;
    public GameObject vertexTwo;

    public string WDistance;
    public string lineType;
    public ArrayList contChambers;

    private LineRenderer line;



    public void Create(GameObject vertexOne, GameObject vertexTwo, string lineType, string WDistance)
    {
        contChambers = new ArrayList();

        this.vertexOne = vertexOne;
        this.vertexTwo = vertexTwo;

        this.lineType = lineType;
        this.WDistance = WDistance;

        line = gameObject.GetComponent<LineRenderer>();


        line.SetPosition(0, this.vertexOne.transform.localPosition);
        line.SetPosition(1, this.vertexTwo.transform.localPosition);
    }


    // Update is called once per frame
    public void Update()
    {
        // Check if the GameObjects are not null
        if (vertexOne != null && vertexTwo != null)
        {
            // Update position of the two vertex of the Line Renderer
            this.line.SetPosition(0, vertexOne.transform.localPosition);
            this.line.SetPosition(1, vertexTwo.transform.localPosition);

        }
    }

    public override string ToString()
    {
        string str = string.Empty;

        str += lineType;

        if (WDistance == "") str += ":I";
        else str += ":" + WDistance;

        foreach (string chamb in contChambers)
            str += ":" + chamb;

        return str;
    }
}