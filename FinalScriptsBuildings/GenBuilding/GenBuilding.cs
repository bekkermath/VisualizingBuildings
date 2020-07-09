/// <summary>
/// Script for generating the building. It depends on GenGroup, Chamber, Vertex
/// Line, TriangleMesh, Dijkstra, GroupParams, GroupParamFuncs, paramNames,
/// posFuncs, BuildingParamFuncs, BuildingParams, TestForm, TestFormFuncs and
/// TestFormNames.
/// 
/// It's hardcoded inputs are bPName, which refers to the right method to set the building
/// parameters, atFName, which is used by GenGroup to pick the right TestForm method, and
/// the radius, which is only used in the affine case, but must be set for finite buildings
/// too.
/// </summary>


using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class GenBuilding : MonoBehaviour
{
    // Hardcoded input variables (for working with GenGroup):
    private readonly paramNames bPName =
        paramNames.AffA2;               // Name of the building.
    private readonly TestFormNames tFName =
        TestFormNames.A2;               // Type of Coxeter complex. 
                                        // Make sure this coincides with 'bPName'!
    private readonly int radius = 5;    // Maximal word length up to which the Weyl group 
                                        // should be generated (if building is affine).

    
    // Transforms for points, lines, triangles and text:
    public Transform vertexPrefab;
    public Transform linePrefab;
    public Transform trianglePrefab;
    public Transform textPrefab;

    //Methods set by bPName.
    private static readonly BuildingParams[] bPs = {
        BuildingParamFuncs.SphA2,
        BuildingParamFuncs.AffA2,
        BuildingParamFuncs.SphA3
    };                                  // Array of all possible building parameter functions.
    private static readonly PosFuncs[] pFs =
    {
        BuildingParamFuncs.PosSphA2,
        BuildingParamFuncs.PosAffA2,
        BuildingParamFuncs.PosSphA3
    };                                  // Array of possible position functions for vertices.
    private PosFuncs pF;

    // Group data set by GenGroup:
    public Dictionary<string, Chamber>
        chambDict;      // (chambername, chamber)
    public Dictionary<string, ArrayList>
        chamberGraph;   // (chambername, list of neighbours with adjacency relations)

    // Set during run by GenBuildings:
    public ArrayList chambNames;    // List of chamber names.
    public ArrayList W;             // Weyl group.
    public string[] S;              // Simple reflections.
    public string maxWord;          // Word with maximal word length (only useful in 
                                     // finite case)
    public int maxWordLength;       // Length of maxWord.
    public Dictionary<string, Transform>
        triangDict;                  // (chambername, associated triangle).
    public Dictionary<string, ArrayList>
        chambsAtDist;               // (Weyl group element, chambers at that distance from 
                                    // fundamental chamber)

    public ArrayList vertices;      // List of vertices.
    public ArrayList lines;         // List of Lines between vertices.

    public Vector3[] roots;         // List of the simple roots of the fundamental apartment.
    public Vector3[] fundCoord;     // List of coordinates for the vertices of the fundamental chamber.
    public bool[] AffineRoot;       // AffineRoot[i] is true if reflection corresponding to roots[i]
                                    //is affine (not through origin).

    // Parsing helpers:
    static readonly char[] space = new char[] { ' ' };
    static readonly StringSplitOptions remEmpty =
        StringSplitOptions.RemoveEmptyEntries;

    public void Start()// Main method.
    {
        // Generate building data.
        GenGroup.Generate(radius, bPName, tFName);
        SetGroupData();


        SetBuildingParams();
        SetMaxWordLength();

        // Draw the building.
        DrawBuilding();
    }

    ////////////////////////////////////////////////////////////
    /// Methods for setting building data.
    private void SetGroupData()
    {
        // Copy data from GenGroup.
        W = GenGroup.W;
        S = GenGroup.S;
        chambNames = GenGroup.chambers;
        chamberGraph = GenGroup.chamberGraph;

        // Declare dictionaries and ArrayLists.
        chambDict = new Dictionary<string, Chamber>();
        triangDict = new Dictionary<string, Transform>();
        chambsAtDist = new Dictionary<string, ArrayList>();
        vertices = new ArrayList();
        lines = new ArrayList();
    }

    private void SetBuildingParams()   // Declares simple roots of 
                              // the fundamental apartment.
    {
        bPs[(int)bPName]();
        pF = pFs[(int)bPName];
    }

    private void SetMaxWordLength()  // Calculates which element of W
                                     // has maximal length and sets 
                                     // maxWord and maxWordLength.
    {
        maxWordLength = int.MinValue;
        foreach (string w in W)
        {
            string[] wSplit = w.Split(space, remEmpty);

            if (wSplit.Length > maxWordLength)
            {
                maxWordLength = wSplit.Length;
                maxWord = w;
            }
        }
    }

    ////////////////////////////////////////////////////////////
    /// Methods for drawing the building
    private void DrawBuilding() // Main method for drawing the building
    {
        // Initialize the chambers.
        DecChambers();
        SetChambHeight();

        // Initialize all the transforms.
        DecVertices();
        DecLines();
        DecTriangles();

        // Set positions of the vertices.
        SetPosVertices();
    }

    /// Methods for declaring the chambers
    private void DecChambers()  //Declares and sets the chambers
                                //(Transforms called triangles with Chamber component).
    {
        for (int i = 0; i < chambNames.Count; i++)
        {
            DecChamb((string)chambNames[i]);
        }

        //Set the chambsAtDist dictionary.
        for (int i = 0; i < W.Count; i++)
        {
            string w = (string)W[i];
            ArrayList cADw = new ArrayList();

            for (int j = 0; j < chambNames.Count; j++)
                if (chambDict[(string)chambNames[j]].WDistance == w)
                    cADw.Add(chambNames[j]);

            chambsAtDist.Add(w, cADw);
        }
    }

    private void DecChamb(string chambName) // Declare a single chamber.
    {
        //Declare new triangle and set it's chamber component.
        Transform triangTrans = Instantiate(trianglePrefab);
        triangTrans.SetParent(transform);

        Chamber chamber = triangTrans.GetComponent<Chamber>();
        chamber.Create(chambName);
        chamber.triangle = triangTrans;

        chambDict.Add(chambName, chamber);
        triangDict.Add(chambName, triangTrans);

        //Set Weyl distance from fundamental chamber to 'chamber'.
        chamber.WDistance = SetWDist(chambName);
    }

    private string SetWDist(string chambName)   //Finds the Weyl distance from 'chambName'
                                                //to the fundamental chamber.
    {
        Dictionary<string, int> Dist = Dijkstra.Distance(
                chamberGraph, chambNames, "I B", chambName);
        string[] pathTree = Dijkstra.PathTree(Dist,
            chamberGraph, chambName);
        string WDist = pathTree[pathTree.Length - 1];

        //Get standard presentation of WDist
        if (WDist.Split(space, remEmpty).Length != 0)
        {
            foreach (string w in W)
            {
                if (GenGroup.MatsEq(GenGroup.matrixDict[w],
                    GenGroup.GetMatrix(WDist))) WDist =  w;
            }
        }
        else WDist = "I";

        return WDist;
    }

    private void SetChambHeight()   //Sets chamber.height. Used for seperating the apartments.
    {
        //Declare dictionary that keeps track of what chambers have been processed.
        Dictionary<string, bool> processed =
            new Dictionary<string, bool>();
        foreach (string chamber in chambNames)
            processed.Add(chamber, false);
        string curChambName;
        Chamber curChamber;


        //Set height of fundamental chamber to 1.
        curChambName = "I B";
        curChamber = chambDict["I B"];
        chambDict["I B"].height = 1;
        processed["I B"] = true;

        //W is ordered by word length.
        for (int i = 0; i < W.Count; i++)
        {
            string w = (string)W[i];
            ArrayList cADw = chambsAtDist[w];

            for (int j = 0; j < cADw.Count; j++)
            {
                curChambName = (string)cADw[j];
                curChamber = chambDict[curChambName];

                //If chamber is not processed yet, continue.
                if (!processed[curChambName]) continue;

                //Loop over the neighbours of the chamber.
                //We keep track of how many neighbours have been processed on
                //each edge of the chamber, so we can set their height properly.
                Dictionary<string, int> numProc =
                    new Dictionary<string, int>();
                for (int k = 0; k < S.Length; k++)
                    numProc.Add(S[k], 0);

                for (int k = 0; k < chamberGraph[curChambName].Count; k++)
                {
                    string[] neighbour = (string[])chamberGraph[curChambName][k];
                    string chamb2Name = neighbour[0];
                    Chamber chamber2 = chambDict[chamb2Name];

                    //If neighbour is arleady processed, continue.
                    if (processed[chamb2Name]) continue;

                    chamber2.height = curChamber.height +
                        numProc[neighbour[1]] / ((double)(cADw.Count));
                    processed[chamb2Name] = true;
                    numProc[neighbour[1]]++;
                }
            }

        }
    }

    /// Methods for declaring the transforms
    private void DecVertices()  // Declare the vertices.
                                // For each simple reflection there is a
                                // vertex per chamber. We check if it already
                                // exists, and if so copy it from another chamber.
    {
        Dictionary<string, bool> processed =
            new Dictionary<string, bool>();
        string curChambName;
        Chamber curChamber;

        string[] coTypes = { "s0", "s1", "s2" };// If the building is of rank 2, 
                                                // we still generate the triangles
                                                // because it looks cooler. For this
                                                // we add one vertex of type "s3" at (0,0,0).
        int numbTypes = coTypes.Length;
        int numProc = 0;

        for (int i = 0; i < chambNames.Count; i++)
            processed.Add((string)chambNames[i], false);

        // Set vertices for the first chamber in chambNames (probably "I  B").
        curChambName = (string)chambNames[0];
        curChamber = chambDict[curChambName];
        processed[curChambName] = true;
        numProc++;
        for (int i = 0; i < numbTypes; i++)
        {
            DecVert(i, coTypes, curChamber);
        }

        while (numProc < chambNames.Count)
        {
            curChambName = (string)chambNames[numProc];
            curChamber = chambDict[curChambName];
            numProc++;
            processed[curChambName] = true;

            bool[] typeInit = { false, false, false };  //Keeps track which vertices of which
                                                        //type have been generated.
            ArrayList neighbours = chamberGraph[curChambName];

            for (int i = 0; i < neighbours.Count; i++)
            {
                string[] neighAdj = (string[])neighbours[i];
                string neighbour = neighAdj[0];
                string adjacency = neighAdj[1];

                if (!processed[neighbour]) continue;

                for (int j = 0; j < numbTypes; j++)
                {
                    if (adjacency == coTypes[j] &&
                        (!typeInit[(j + 1) % numbTypes] ||
                        !typeInit[(j + 2) % numbTypes]))    // If chambers are adjacent for simple reflection 'adjacency'
                                                            // and 'curChamber' does not have the vertices they should 
                                                            //share yet, copy the vertices form neighbour.
                    {
                        CopyVert((j + 1) % numbTypes, curChamber,
                            chambDict[neighbour]);
                        typeInit[(j + 1) % numbTypes] = true;

                        CopyVert((j + 2) % numbTypes, curChamber,
                            chambDict[neighbour]);
                        typeInit[(j + 2) % numbTypes] = true;

                        break;
                    }
                }

                // Terminate the loop if all coTypes have been initialized.
                bool done = true;
                for (int j = 0; j < numbTypes; j++)
                    if (!typeInit[j]) done = false;
                if (done) break;
            }

            // Declare vertices that could not be copied from neighbours.
            for (int i = 0; i < numbTypes; i++)
            {
                if (typeInit[i]) continue;  // Skip types that are already initialized

                DecVert(i, coTypes, curChamber);
            }

        }
    }

    private void DecVert(int coType, string[] coTypes, Chamber chamber) // Declare a single vertex.
    {
        Transform vertTrans = Instantiate(vertexPrefab);
        vertTrans.SetParent(transform);
        Vertex vertComp = vertTrans.GetComponent<Vertex>();

        vertComp.Create(coTypes[coType], chamber.chamberName);
        vertComp.WDistance = chamber.WDistance;
        chamber.vertices[coType] = vertTrans;

        vertices.Add(vertTrans);
    }

    private void CopyVert(int coType, Chamber chamber,
        Chamber neighbour)              // Copy vertex from neighbour to chamber.
    {
        Transform vertTrans = neighbour.vertices[coType];
        chamber.vertices[coType] = vertTrans;

        Vertex vertComp = vertTrans.GetComponent<Vertex>();
        vertComp.contChamber[chamber.chamberName] = true;

        if (chamber.WDistance.Split(space, remEmpty).Length <
            neighbour.WDistance.Split(space, remEmpty).Length)
            vertComp.WDistance = chamber.WDistance;
    }

    private void DecLines() // Same principle as for DecVertices. Creates for each 
                            // chamber a line for each of the types. Copies it from
                            // another chamber if it already exists.
    {
        Dictionary<string, bool> processed =
            new Dictionary<string, bool>();
        string curChambName;
        Chamber curChamber;
        string[] types = { "s0", "s1", "s2" };
        int numbTypes = types.Length;
        int numProc = 0;

        for (int i = 0; i < chambNames.Count; i++)
            processed.Add((string)chambNames[i], false);

        curChambName = (string)chambNames[0];
        curChamber = chambDict[curChambName];
        processed[curChambName] = true;
        numProc++;

        for (int i = 0; i < numbTypes; i++)
        {
            DecLine(i, types, curChamber);
        }

        while (numProc < chambNames.Count)
        {
            curChambName = (string)chambNames[numProc];
            curChamber = chambDict[curChambName];
            numProc++;
            processed[curChambName] = true;

            bool[] typeInit = { false, false, false };

            ArrayList neighbours = chamberGraph[curChambName];
            for (int i = 0; i < neighbours.Count; i++)
            {
                string[] neighAdj = (string[])neighbours[i];
                string neighbour = neighAdj[0];
                string adjacency = neighAdj[1];

                if (!processed[neighbour]) continue;

                for (int j = 0; j < numbTypes; j++)
                {
                    if (adjacency == types[j] && !typeInit[j])
                    {
                        CopyLine(j, curChamber, chambDict[neighbour]);
                        typeInit[j] = true;

                        break;
                    }
                }

                if (typeInit[0] && typeInit[1] && typeInit[2]) break;
            }

            for (int j = 0; j < numbTypes; j++)
            {
                if (!typeInit[j])
                {
                    DecLine(j, types, curChamber);
                }
            }
        }
    }

    private void DecLine(int type, string[] types, Chamber chamber)
    {
        int numbTypes = types.Length;

        Transform lineTrans = Instantiate(linePrefab);
        lineTrans.SetParent(transform);
        Line lineComp = lineTrans.GetComponent<Line>();

        lineComp.Create(chamber.vertices[(type + 1) % numbTypes].gameObject,
            chamber.vertices[(type + 2) % numbTypes].gameObject,
            types[type], chamber.WDistance);
        lineComp.contChambers.Add(chamber.chamberName);

        chamber.lines[type] = lineTrans;
        lines.Add(lineTrans);
    }

    private void CopyLine(int type, Chamber chamber, Chamber neighbour)
    {
        Transform lineTrans = neighbour.lines[type];
        chamber.lines[type] = lineTrans;

        Line lineComp = lineTrans.GetComponent<Line>();
        lineComp.contChambers.Add(chamber.chamberName);

        if (chamber.WDistance.Split(space, remEmpty).Length <
            lineComp.WDistance.Split(space, remEmpty).Length)
            lineComp.WDistance = chamber.WDistance;
    }

    public void DecTriangles()  // Declares one triangle for every chamber.
    {
        for (int i = 0; i < chambNames.Count; i++)
        {
            string chambName = (string)chambNames[i];
            DecTriangle(triangDict[chambName]);
        }
    }

    private void DecTriangle(Transform triangTrans)  // Declares a single triangle.
    {
        Chamber chamber = triangTrans.GetComponent<Chamber>();
        TriangleMesh triangMesh = triangTrans.GetComponent<TriangleMesh>();

        triangMesh.color = SetTriangleColor(chamber.WDistance, chamber.height);
        triangMesh.Create(chamber.vertices);
    }

    private Color SetTriangleColor(string WDist, double height) // Set the color of a single triangle.
    {
        Color color;
        float transparancy = 0.2f;
        if (height == 1) transparancy = 1f;

        switch (WDist.Split(space, remEmpty).Length)
        {
            case 1:
                if (WDist == "I")
                    color = new Color(1, 0, 0, transparancy);
                else
                    color = new Color(0, 1, 0, transparancy);
                break;
            case 2:
                color = new Color(0, 0, 1, transparancy);
                break;
            case 3:
                color = new Color(1, 1, 0, transparancy);
                break;
            case 4:
                color = new Color(1, 0, 1, transparancy);
                break;
            case 5:
                color = new Color(0, 1, 1, transparancy);
                break;
            default:
                color = new Color(1, 1, 1, transparancy);
                break;
        }

        return color;
    }
    /// Methods for setting the positions of the vertices.
    private void SetPosVertices()   // Sets the position of the vertices.
                                    // Position is determined based on the WDistance only.
                                    // By using chamber.height, chambers with same WDistance 
                                    // are seperated from each other later.
    {
        SetFundVerts(); // Set the positioin of the vertices in the fundamental chamber.

        for (int i = 0; i < vertices.Count; i++)
        {
            Transform vertTrans = (Transform)vertices[i];

            if (vertTrans.GetComponent<Vertex>().contChamber["I B"])
                continue;   // Skip vertices in the fundamental chamber.

            SetVertPos(vertTrans);

        }

    }

    private void SetFundVerts() // Sets the position of the vertices in the fundamental chamber.
    {
        for (int i = 0; i < chambDict["I B"].vertices.Length; i++)
        {
            Transform vertTrans = chambDict["I B"].vertices[i];
            Vertex vertComp = vertTrans.GetComponent<Vertex>();
            Vector3 position = fundCoord[i];

            vertTrans.localPosition = position;
            vertComp.position = position;
        }
    }

    private void SetVertPos(Transform vertTrans)   // Sets position of a single vertex based.
                                                // Position is determined by taking the vertex
                                                // in the fundamental chamber of the same type,
                                                // and transforming it by the Weyl distance.
    {
        Vertex vertComp = vertTrans.GetComponent<Vertex>();
        string WDist = vertComp.WDistance;
        string[] WDistSplit = WDist.Split(space, remEmpty);
        string coType = vertComp.coType;

        Chamber chamber = new Chamber();
        float maxHeight = int.MinValue;
        ArrayList cADw = chambsAtDist[WDist];
        for (int i = 0; i < cADw.Count; i++)
        {
            double chambHeight = chambDict[(string)cADw[i]].height;
            if (chambHeight > maxHeight) // Find maximum height in stack of chambers at distance w.
                maxHeight = (float)chambHeight;

            if (vertComp.contChamber[chambDict[
                (string)cADw[i]].chamberName])  // Choose chamber to be the one containing 'vertComp',
                                                // and with the same WDistance.
                chamber = chambDict[(string)cADw[i]];
        }

        Chamber fundChamber = chambDict["I B"];
        for (int i = 0; i < fundChamber.vertices.Length; i++)   // Find fundamental vertex of same coType and apply
                                                                // the reflections in the roots corresponding to
                                                                // the letter of 'w' to find position of vertex.
        {
            Transform fundVertTrans = fundChamber.vertices[i];
            Vertex fundVert = fundVertTrans.GetComponent<Vertex>();

            if (coType != fundVert.coType) continue;

            Vector3 position = pF((float)chamber.height, maxHeight,
                fundVertTrans, WDistSplit);

            vertTrans.localPosition = position;
            vertTrans.GetComponent<Vertex>().position = position;

            break;
        }
    }

    public Vector3 Reflect(Vector3 oldPos, string[] WDist)
    {
        Vector3 newPos;

        if (WDist.Length == 1)
        {
            char[] WDistSplit = WDist[0].ToCharArray();
            bool succes = int.TryParse(WDistSplit[1].ToString(), NumberStyles.Number,
                new CultureInfo("en-US"), out int coType);  // Find index of reflection.

            if (succes)
            {
                if (AffineRoot[coType])
                    newPos = oldPos -
                    2f * (float)Vector3.Dot(roots[coType], oldPos + roots[coType]) * roots[coType];
                else
                    newPos = oldPos -
                    2f * (float)Vector3.Dot(roots[coType], oldPos) * roots[coType];
            }
            else
            {
                Console.WriteLine("parse unsuccessful");

                newPos = oldPos;
            }
        }
        else
        {
            string[] newWDist = new string[WDist.Length - 1];
            for (int i = 1; i < WDist.Length; i++)
            {
                newWDist[i - 1] = WDist[i];
            }

            newPos = Reflect(Reflect(oldPos, newWDist),
                new string[] { WDist[0] });
        }

        return newPos;
    }
}
