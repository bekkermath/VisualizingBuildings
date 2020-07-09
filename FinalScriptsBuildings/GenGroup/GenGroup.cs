/// <summary>
/// Script for generating all necessary components of a group in order
/// to generate its building. Can be used
/// as direct input for the GenBuilding class, or for writing to text.
///
/// Depends on TestFormNames, GroupParamNames, TestFormFuncs, 
/// GroupParamFuncs, TestForm and GroupParams for delegate declaration.
///
/// It takes dimension of the matrices, characteristic of the field,
/// residual characteristic of the field, rank of the building, size
/// of the Weyl group W (for finite W), maximal radius of the building
/// (for infinite W) and the simple reflections as hard coded input.
/// The field should be an instance of Q with a p-adic valuation. For
/// the spherical building over F2 for instance, take field=Q2, and set
/// 'affine' to false.
///
/// It gives a ArrayList of chambers, the Weyl group W and the
/// Dictionary<string, ArrayList> chamber graph (with adjacency
/// relations) as output. An adjacency relation is the simple
/// reflection taking one chamber to its neighbour.
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public static class GenGroup
{
    //Input, hard coded:
    private static readonly GroupParams[] gPs = {
        GroupParamFuncs.SphA2,
        GroupParamFuncs.AffA2,
        GroupParamFuncs.SphA3 };
    private static readonly TestForm[] tFs = {
        TestFormFuncs.A2,
        TestFormFuncs.A3 };
    private static TestForm tF;

    public static readonly int fChar = 0;       //Characteristic of the field the matrices are defined over.
    public static readonly int resChar = 2;     //The residual characteristic (in affine case). For now, only
                                                //2 and 5 work! This has to do with the Method 'Val' (cannot
                                                //handle repeating digits).


    //Output:
    public static ArrayList W;  //The Weyl group (in the infinite case only 
                                //up to word length equal to 'radius'.

    public static ArrayList
        chambers;               //The eventual list of representatives of the chambers.
    public static Dictionary<string, ArrayList>
        chamberGraph;           //The chamber graph. Keys are the chamber names,
                                //values are arraylists of string pairs of the form
                                //(neighbour, adjacency relation).


    //Group parameters. Set during run:

    private static paramNames gPName;   //Name of the building.
    private static TestFormNames tFName;     //Type of Coxeter complex.
    private static int radius;               //The maximal occuring word
                                             //length if W is infinite.

    public static bool affine;      //True if the building is affine.
    public static int dim;          //Dimensions of the matrices
    public static int orderW;       //The order of W if it is finite
    public static int rank;         //Rank of the building (number of simple reflections).

    public static double[,] s0; //Matrix representations of the simple reflections of W.
    public static double[,] s1; //
    public static double[,] s2; //

    public static double[,] I;  //Unit matrix
    public static string[] S;   //The set of simple reflections
    public static double[,] b; //The characteristic matrix of the fundamental
                               //apartment, corresponding to B.

    public static Dictionary<string, double[,]>
        charMatDict;            //Keys are chamber names, values are characterist matrices.
    public static Dictionary<string, double[,]>
        matrixDict;             //Keys are matrix names, values are matrices.
    public static Dictionary<string, ArrayList>
        rGrDict;         //Key are elements of W, values are the root groups
                         //corresponding to that value. 


    //Parsing helpers:
    private static readonly char[] space = new char[] { ' ' };
    private static readonly StringSplitOptions remEmpty =
        StringSplitOptions.RemoveEmptyEntries;


    public static void Generate(int radPar, paramNames gPNPar,
        TestFormNames tFNPar)   //Main function.
    {
        //initalize basic variables
        SetParams(radPar, gPNPar, tFNPar);

        SetI();
        Setb();
        SetS();
        InitDicts();

        //Generate group components.
        GenW();     //Generate the Weyl group (if infinite, up to
                    //word length = 'radius').

        GenLTB();   //Generate left transversal set for B and chamber
                    //names derived from it.

        GenChGr();  //Generate the chamber graph.
    }


    ////////////////////////////////////////////////////////////////////////
    //Methods for initialization of variables.
    private static void SetParams(int radPar, paramNames gPNPar,
        TestFormNames tFNPar)
    {
        radius = radPar;
        gPName = gPNPar;
        tFName = tFNPar;

        gPs[(int)gPName]();

        tF = tFs[(int)tFName];
    }

    private static void SetI() //Sets the unit matrix of dimension 'dim'.
    {
        I = new double[dim, dim];

        for (int i = 0; i < dim; i++)
        {
            I[i, i] = 1d;
            for (int j = i + 1; j < dim; j++)
            {
                I[i, j] = 0d;
                I[j, i] = 0d;
            }
        }
    }


    private static void Setb()  //Depends on building type ('B')
    {
        b = new double[dim, dim];

        for (int i = 0; i < dim; i++)
        {
            b[i, i] = 1d;
            for (int j = i + 1; j < dim; j++)
            {
                b[i, j] = 1d;
                b[j, i] = (double)resChar;
            }
        }
    }

    private static void SetS() //Sets S, the set of simple reflections of cardinality 'rank'.
    {
        S = new string[rank];

        for (int i = 0; i < rank; i++)
        {
            string s = string.Empty;
            s += "s" + i.ToString();

            S[i] = s;
        }
    }


    private static void InitDicts() //Initializes the dictionaries and ArrayLists necessary.
    {
        W = new ArrayList();
        chambers = new ArrayList();

        chamberGraph = new Dictionary<string, ArrayList>();
        charMatDict = new Dictionary<string, double[,]>();
        rGrDict = new Dictionary<string, ArrayList>();

        matrixDict = new Dictionary<string, double[,]>()
        {
            {"I", I },
            {"s0", s0 },
            {"s1", s1 }
        };
        if (rank >= 3)   //Can be expanded to also include a fourth simple reflection if rank = 4.
            matrixDict.Add("s2", s2);

    }


    ////////////////////////////////////////////////////////////////////////
    //Methods for generating W.
    private static void GenW()  //Generates elements of W. Iterates over word length until either 
                                //W.Count == 'orderW' (W is finite), or l == 'radius' (W is infinite).
    {
        int l = 0;      //Length of words generated.
        int WCount = 0; //Necessary in finite case to check if iteration should stop.
        bool cont = true;

        while (cont)
        {
            string[] wordsGen = GenWords(l, S); //Generate all words of lenght 'l' with generators in 'S'
                                                //including possible equivalent presentations.
            int wGLength = wordsGen.Length;

            if (wGLength == 0) continue;

            for (int i = 0; i < wGLength; i++) //Check if word is equivalent to already been generated word.
                                               //If not, add to W and add (word,wordMat) to MatrixDict.
            {
                string word = wordsGen[i];
                double[,] wordMat = GetMatrix(word);

                bool matrixDouble = false;

                for (int j = 0; j < WCount; j++)
                {
                    double[,] checkMat = GetMatrix((string)W[j]);
                    if (MatsEq(wordMat, checkMat))
                    {
                        matrixDouble = true;
                        break;
                    }
                }
                if (!matrixDouble)
                {
                    W.Add(word);
                    if (l > 1) matrixDict.Add(word, wordMat);
                    WCount++;
                }
            }
            l++;

            if (affine) cont = (l <= radius);   //Condition to execute next loop for infinite W.
            else cont = (WCount < orderW);      //Condition to execute next loop for finite W.
        }
    }


    ////////////////////////////////////////////////////////////////////////
    //Methods for generating (part of) a left transversal set of B.
    private static void GenLTB()    //Calculates a left transversal set of B
                                    //by calculating the root groups for each
                                    //element of W, and declares chamber names.
    {
        for (int i = 0; i < W.Count; i++)
        {
            ArrayList rootGroups = new ArrayList();

            string w = (string)W[i];
            double[,] charMat = CharMat(w);

            //Generate root groups for w.
            for (int j = 0; j < dim; j++) //Depends on Coxeter group type
            {
                for (int k = j + 1; k < dim; k++)
                {
                    string[] rootGroup1;
                    string[] rootGroup2;

                    if (charMat[j, k] > 0 && charMat[j, k] != int.MaxValue)
                    {
                        rootGroup1 = new string[(int)Math.Max(1d, Math.Pow((double)resChar, charMat[j, k]))];
                        rootGroup1[0] = "I";
                        for (int l = 1; l < Math.Pow((double)resChar, charMat[j, k]); l++)
                            rootGroup1[l] = GenRGrElt(j, k, (double)l);
                    }
                    else
                        rootGroup1 = new string[] { "I" };

                    rootGroups.Add(rootGroup1);

                    if (charMat[k, j] > 1 && charMat[j, k] != int.MaxValue)
                    {
                        rootGroup2 = new string[(int)Math.Max(1d, Math.Pow((double)resChar, charMat[k, j] - 1))];
                        rootGroup2[0] = "I";
                        for (int l = 1; l < Math.Pow((double)resChar, charMat[k, j] - 1); l++)
                            rootGroup2[l] = GenRGrElt(k, j, (double)(resChar * l));
                    }
                    else
                        rootGroup2 = new string[] { "I" };

                    rootGroups.Add(rootGroup2);
                }
            }
            rGrDict.Add(w, rootGroups);

            //Generate chamber names.
            int rGrCount = rootGroups.Count;
            string[][] genSets = new string[rGrCount][];
            int[] lengths = new int[rGrCount];
            for (int j = 0; j < rGrCount; j++)
            {
                genSets[j] = (string[])rootGroups[j];
                lengths[j] = 1;
            }

            string[] rGrWords = GenWords(lengths, genSets);
            for (int j = 0; j < rGrWords.Length; j++)
            {
                string word = rGrWords[j];
                if (word == "" || word == "I") chambers.Add(w + " B");
                else chambers.Add(word + " " + w + " B");
            }
        }
    }

    private static double[,] CharMat(string w)  //Calculates the characteristic
                                                //matrix for an element of W, adds it
                                                //to 'charMatDict' and returns it.
    {
        double[,] wMat = GetMatrix(w);
        double[,] charMat = MultMats(wMat, b);
        charMat = MultMats(charMat, GetMatrix(InvWord(w)));

        for (int i = 0; i < dim; i++)
            for (int j = 0; j < dim; j++)
            {
                charMat[i, j] = Val(charMat[i, j], resChar);
            }

        charMatDict.Add(w, charMat);

        return charMat;
    }

    private static string GenRGrElt(int ipos, int jpos, double value)    //Generates the generators of the root groups with
                                                                         //corresponding letters, and their inverses.
                                                                         //Depends on building type!
    {
        double[,] matrix = new double[dim, dim];
        double[,] invMat = new double[dim, dim];

        for (int i = 0; i < dim; i++)
        {
            matrix[i, i] = 1d;
            invMat[i, i] = 1d;

            for (int j = i + 1; j < dim; j++)
            {
                matrix[i, j] = 0d;
                matrix[j, i] = 0d;

                if (ipos < jpos && i == ipos && j == jpos)
                    matrix[i, j] = value;
                if (ipos > jpos && i == jpos && j == ipos)
                    matrix[j, i] = value;


                invMat[i, j] = 0d;
                invMat[j, i] = 0d;

                if (ipos < jpos && i == ipos && j == jpos)
                    invMat[i, j] = Dmodi(-value, fChar);
                if (ipos > jpos && i == jpos && j == ipos)
                    invMat[j, i] = Dmodi(-value, fChar);
            }
        }

        string matName = "u(" + ipos.ToString() + "," + jpos.ToString()
            + ")(" + value.ToString() + ")";
        TrymatrixDictAdd(matName, matrix);

        string invMatName = "u(" + ipos.ToString() + "," + jpos.ToString()
            + ")(" + (Dmodi(-value, fChar)).ToString() + ")";
        TrymatrixDictAdd(invMatName, invMat);

        return matName;
    }


    ////////////////////////////////////////////////////////////////////////
    //Methods for generating the chamber graph.
    private static void GenChGr()
    {
        for (int i = 0; i < chambers.Count; i++)
        {
            string chamber1 = (string)chambers[i];
            ArrayList neighbours = new ArrayList();

            for (int j = 0; j < chambers.Count; j++)
            {
                string chamber2 = (string)chambers[j];

                if (chamber1 == chamber2) continue;

                double[,] distMat = DistMat(chamber1, chamber2);

                for (int k = 0; k < rank; k++)
                {
                    string s = S[k];

                    if (tF(distMat, s))
                    {
                        neighbours.Add(new string[] { chamber2, s });
                        break;
                    }
                }
            }
            chamberGraph.Add(chamber1, neighbours);
        }
    }

    //Calculate the matrix that determines the Weyl distance between the chambers.
    public static double[,] DistMat(string chamber1, string chamber2)//Returns the Weyl distance matrix from chamber1 to chamber2.
    {
        double[,] chamb1InvMat;
        double[,] chamb2Mat;
        double[,] distMat;

        chamb1InvMat = GetMatrix(InvWord(ChambMatName(chamber1)));
        chamb2Mat = GetMatrix(ChambMatName(chamber2));

        distMat = MultMats(chamb1InvMat, chamb2Mat);

        for (int i = 0; i < dim; i++)
            for (int j = 0; j < dim; j++)
                distMat[i, j] = Val(distMat[i, j], resChar);

        return distMat;
    }

    ////////////////////////////////////////////////////////////////////////
    //Helper methods for parsing and generating words.
    private static string[] GenWords(int length, string[] genSet)   //Recursively generates words of length 'length' from generators in genSet.
    {
        if (length < 0)
        {
            Console.WriteLine("Length must be a possitive integer!");
            return null;
        }
        if (length == 0 || genSet.Length == 0 || genSet == null)
            return new string[] { "I" };
        else if (length == 1)
            return genSet;

        else
        {
            string[] genWords = GenWords(length - 1, genSet);
            string[] words = new string[genSet.Length * genWords.Length];

            for (int j = 0; j < genWords.Length; j++)
                for (int k = 0; k < genSet.Length; k++)
                    words[genSet.Length * j + k] =
                        genWords[j] + " " + genSet[k];

            return words;
        }
    }

    private static string[] GenWords(int[] length, string[][] genSets)  //Overload for generating words of the form a[genSets.Length-1] ... a[0],
                                                                        //where a[i] is a word generated by elements of genSets[i].
                                                                        //Calls GenWords(int length, string[] genSet).
    {
        int numSubWords = length.Length;

        if (numSubWords != genSets.Length)
        {
            Console.WriteLine("Array length and array genSets do not have same length!");
            return null;
        }
        if (numSubWords == 0)
        {
            return new string[] { "I" };
        }


        string[] wordsStart = GenWords(length[0], genSets[0]);
        if (numSubWords == 1)
            return wordsStart;
        else
        {
            int[] lengthEnd = new int[numSubWords - 1];
            string[][] genSetsEnd = new string[numSubWords - 1][];
            for (int i = 1; i < numSubWords; i++) //Copy arrays starting from index=1.
            {
                lengthEnd[i - 1] = length[i];
                genSetsEnd[i - 1] = genSets[i];
            }

            string[] wordsEnd = GenWords(lengthEnd, genSetsEnd);
            int wSLength = wordsStart.Length;
            int wELength = wordsEnd.Length;

            string[] words = new string[wSLength * wELength];
            for (int i = 0; i < wSLength; i++)
            {
                for (int j = 0; j < wELength; j++)
                {
                    string wS = wordsStart[i];
                    string wE = wordsEnd[j];
                    if (wS == "I") //Remove unnecessary instances of 'I'
                        words[i * wELength + j] = wE;
                    else if (wE == "I")
                        words[i * wELength + j] = wS;
                    else
                        words[i * wELength + j] = wS + " " + wE;
                }
            }

            return words;
        }
    }

    public static string ChambMatName(string chamber) //Return the word associated to the chamber, e.g. 'g B' returns 'g'.
    {
        string chambMatName = string.Empty;

        string[] chambSplit = chamber.Split(space, remEmpty);

        for (int i = 0; i < chambSplit.Length - 1; i++)
        {
            chambMatName += chambSplit[i] + " ";
        }

        return chambMatName;
    }

    public static void TrymatrixDictAdd(string matName, double[,] matrix)   //Checks if matrix is already in matrixDict. If not, adds.
    {
        bool matrixDouble = false;

        foreach (double[,] entry in matrixDict.Values)
        {
            if (MatsEq(matrix, entry))
            {
                matrixDouble = true;
                break;
            }
        }

        if (!matrixDouble) matrixDict.Add(matName, matrix);
    }

    public static double[,] GetMatrix(string word)   //Takes word and returns corresponding matrix
                                                     //over field of characteristic p.
                                                     //MatrixDict entries should already exist 
                                                     //for all letters of the word!
    {
        double[,] matrix;

        //Create array of letters
        string[] wordSplit = word.Split(space, remEmpty);

        //Calculate corresponding matrix
        matrix = matrixDict[wordSplit[0]];
        for (int i = 1; i < wordSplit.Length; i++)
        {
            matrix = MultMats(matrix,
                matrixDict[wordSplit[i]]);
        }

        return matrix;
    }

    public static string InvWord(string word)     //Takes a word and returns its inverse
    {
        string[] wordSplit = word.Split(space, remEmpty);
        int wSLength = wordSplit.Length;

        //Find inverse of each letter and append to invWord in reverse order recursively
        if (wSLength == 1)
            return GetInverse(wordSplit[0]);
        else
        {
            string newWord = string.Empty;
            for (int i = 0; i < wSLength - 1; i++)
            {
                newWord += wordSplit[i] + " ";
            }

            return GetInverse(wordSplit[wSLength - 1]) + " " + InvWord(newWord);
        }
    }

    public static string GetInverse(string letter) //Returns the inverse of a letter.
                                                   //Depends on field!
    {
        string inv = string.Empty;
        char[] letterSplit = letter.ToCharArray();


        if (letterSplit[0] == 's' || letterSplit[0] == 'I')
            return letter;
        else if (letterSplit[0] == 'u')
        {
            for (int i = 0; i < 7; i++)
            {
                inv += letterSplit[i];
            }


            string invValString = string.Empty;
            if (letterSplit[7] != '-') invValString = "-";

            for (int i = 7; i < letterSplit.Length - 1; i++)
                invValString += letterSplit[i];

            int.TryParse(invValString, NumberStyles.Number, new CultureInfo("en-US"), out int invVal);
            invVal = (int)Dmodi((double)invVal, fChar);
            invValString = invVal.ToString();

            inv += invValString + ")";
            return inv;
        }
        else
        {
            Console.WriteLine("Input is not a letter and inverse cannot be returned.");

            return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////
    //Helper methods for matrix calculations.
    public static double[,] MultMats(double[,] a, double[,] b)   //Basic function for multiplying matrices.
    {
        if (a.GetLength(1) == b.GetLength(0))
        {
            double[,] c;
            c = new double[a.GetLength(0), b.GetLength(1)];

            for (int i = 0; i < c.GetLength(0); i++)
            {
                for (int j = 0; j < c.GetLength(1); j++)
                {
                    c[i, j] = 0;
                    for (int k = 0; k < a.GetLength(1); k++)
                    {
                        c[i, j] = c[i, j] + a[i, k] * b[k, j];
                    }
                    c[i, j] = Dmodi(c[i, j], fChar);
                }
            }

            return c;
        }
        else
        {
            Console.WriteLine(
                "Number of columns in First Matrix should be equal to Number of rows in Second Matrix.");

            return null;
        }
    }
    public static double Dmodi(double a, int b)
    {
        if (b == 0) return a;

        return (double)(a - b * Math.Floor(a / (double)b));
    }

    public static bool MatsEq(double[,] a, double[,] b)   //Function for checking if two matrices are equal
    {
        if (a.GetLength(0) != b.GetLength(0) ||
            a.GetLength(1) != b.GetLength(1))
        {
            return false;
        }
        else
        {
            for (int i = 0; i < a.GetLength(0); i++)
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    if (a[i, j] != b[i, j])
                    {
                        return false;
                    }
                }
        }
        return true;
    }

    private static int Val(double a, int p)//Calculates the p-adic valuation of a.
    {
        int val = int.MaxValue;

        if (a != 0)
        {
            if (a % 1 == 0)             //If a is an integer
            {
                if (Dmodi(a, p) != 0d) val = 0;
                else val = Val(a / p, p) + 1;
            }
            else
            {

                double b = a;
                int i = 0;
                while (b % 1 != 0)
                {
                    b *= 10;
                    i++;
                }
                if (((int)b).ToString().Split().Length == 15) val = 0;  //This is a bad fix in case a has repeating digits.
                else val = Val(b, p) - i;
            }
        }
        return val;
    }
}
