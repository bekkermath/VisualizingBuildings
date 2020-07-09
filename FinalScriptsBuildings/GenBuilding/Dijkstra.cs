using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Dijkstra
{
    public static Dictionary<string, int> Distance(Dictionary<string, ArrayList> chamberGraph,
        ArrayList chambers, string src, string trgt)
    {
        Dictionary<string, int> dist =
            new Dictionary<string, int>();
        Dictionary<string, bool> visNodes =
            new Dictionary<string, bool>();

        int numVisNodes = 1;
        string curNode;

        foreach (string chamber in chambers)
        {
            dist.Add(chamber, int.MaxValue);
            visNodes.Add(chamber, false);
        }
        dist[src] = 0;
        curNode = src;

        while (numVisNodes < chambers.Count && dist[curNode]
            != int.MaxValue && dist[trgt] != dist[curNode])
        {

            visNodes[curNode] = true;
            numVisNodes++;

            foreach (string[] neighbourAr in chamberGraph[curNode])
            {
                string neighbor = neighbourAr[0];

                if (!visNodes[neighbor] &&
                    dist[curNode] + 1 < dist[neighbor])
                    dist[neighbor] = dist[curNode] + 1;
            }

            curNode = MinDistance(dist, visNodes);
        }

        return dist;
    }

    public static string MinDistance(Dictionary<string, int> distance,
                    Dictionary<string, bool> sptSet)
    {
        // Initialize min value 
        int min = int.MaxValue;
        string minChamber = "";

        foreach (string chamber in sptSet.Keys)
            if (sptSet[chamber] == false && distance[chamber] <= min)
            {
                min = distance[chamber];
                minChamber = chamber;
            }

        return minChamber;
    }

    public static string[] PathTree(Dictionary<string, int> distance,
        Dictionary<string, ArrayList> chamberGraph, string trgt)
    {
        int pathLength = distance[trgt] + 1;

        Debug.Log(pathLength.ToString());

        string[] pathTree = new string[pathLength + 1];

        string word = string.Empty;
        string curNode = trgt;

        pathTree[pathLength - 1] = trgt;

        if (pathLength > 1)
        {

            for (int i = 1; i < pathLength; i++)
            {
                for (int j = 0; j < chamberGraph[curNode].Count; j++)
                {
                    string[] neighbourAr = (string[])chamberGraph[curNode][j];
                    string neighbor = neighbourAr[0];

                    if (distance[neighbor] >= distance[curNode]) continue;

                    pathTree[pathLength - 1 - i] = neighbor;
                    curNode = neighbor;
                    word = neighbourAr[1] + " " + word;
                    break;

                }
            }

        }
        else
        {
            word = "I";
        }


        pathTree[pathLength] = word;

        return pathTree;
    }


}
