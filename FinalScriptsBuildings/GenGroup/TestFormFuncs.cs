/// <summary>
/// Functions of the form of the delegate 'TestForm'. Outputs a boolean
/// 'true' if 'matrix' is of the right form to correspond to Weyl distance
/// 's'. The precise functions only depend on the type and rank, not on
/// wether the building is affine or spherical.
///
/// The coefficients of 'matrix' are the valuations of the coefficients of
/// the distance matrix between two chambers. 's' is a simple reflection.
/// </summary>


using System;

public static class TestFormFuncs
{
    public static bool A2(double[,] matrix, string s)
    {
        bool testForm = true;

        if (matrix.GetLength(0) != 3 || matrix.GetLength(1) != 3)
        {
            testForm = false;
            Console.Write("Distance matrix has wrong dimensions!");
        }
        else
        {

            switch (s)
            {
                case "s0":
                    if (matrix[0, 0] < 0  ||
                        matrix[0, 1] < 0  ||
                        matrix[0, 2] < 0  ||
                        matrix[1, 0] != 0 ||
                        matrix[1, 1] < 0  ||
                        matrix[1, 2] < 0  ||
                        matrix[2, 0] < 1  ||
                        matrix[2, 1] < 1  ||
                        matrix[2, 2] != 0  ) testForm = false;
                    break;
                case "s1":
                    if (matrix[0, 0] != 0 ||
                        matrix[0, 1] < 0  ||
                        matrix[0, 2] < 0  ||
                        matrix[1, 0] < 1  ||
                        matrix[1, 1] < 0  ||
                        matrix[1, 2] < 0  ||
                        matrix[2, 0] < 1  ||
                        matrix[2, 1] != 0 ||
                        matrix[2, 2] < 0  ) testForm = false;
                    break;
                case "s2":
                    if (matrix[0, 0] < 0   ||
                        matrix[0, 1] < 0   ||
                        matrix[0, 2] != -1 ||
                        matrix[1, 0] < 1   ||
                        matrix[1, 1] != 0  ||
                        matrix[1, 2] < 0   ||
                        matrix[2, 0] < 1   ||
                        matrix[2, 1] < 1   ||
                        matrix[2, 2] < 0   ) testForm = false;
                    break;
            }
        }

        return testForm;
    }

    public static bool A3(double[,] matrix, string s) //Tests if 'matrix' corresponds to Weyl distance s.
                                                      //Highly dependend on specific building (depends on 'B' and 'S').
    {
        bool testForm = true;

        if (matrix.GetLength(0) != 4 || matrix.GetLength(1) != 4)
        {
            testForm = false;
            Console.Write("Distance matrix has wrong dimensions!");
        }
        else
        {

            switch (s)
            {
                case "s0":
                    if (matrix[0,0] < 0  ||
                        matrix[0,1] < 0  ||
                        matrix[0,2] < 0  ||
                        matrix[0,3] < 0  ||
                        matrix[1,0] != 0 ||
                        matrix[1,1] < 0  ||
                        matrix[1,2] < 0  ||
                        matrix[1,3] < 0  ||
                        matrix[2,0] < 1  ||
                        matrix[2,1] < 1  ||
                        matrix[2,2] != 0 ||
                        matrix[2,3] < 0  ||
                        matrix[3,0] < 1  ||
                        matrix[3,1] < 1  ||
                        matrix[3,2] < 1  ||
                        matrix[3,3] != 0  ) testForm = false;
                    break;
                case "s1":
                    if (matrix[0, 0] != 0 ||
                        matrix[0, 1] < 0  ||
                        matrix[0, 2] < 0  ||
                        matrix[0, 3] < 0  ||
                        matrix[1, 0] < 1  ||
                        matrix[1, 1] < 0  ||
                        matrix[1, 2] < 0  ||
                        matrix[1, 3] < 0  ||
                        matrix[2, 0] < 1  ||
                        matrix[2, 1] != 0 ||
                        matrix[2, 2] < 0  ||
                        matrix[2, 3] < 0  ||
                        matrix[3, 0] < 1  ||
                        matrix[3, 1] < 1  ||
                        matrix[3, 2] < 1  ||
                        matrix[3, 3] != 0  ) testForm = false;
                    break;
                case "s2":
                    if (matrix[0, 0] != 0 ||
                        matrix[0, 1] < 0  ||
                        matrix[0, 2] < 0  ||
                        matrix[0, 3] < 0  ||
                        matrix[1, 0] < 1  ||
                        matrix[1, 1] != 0 ||
                        matrix[1, 2] < 0  ||
                        matrix[1, 3] < 0  ||
                        matrix[2, 0] < 1  ||
                        matrix[2, 1] < 1  ||
                        matrix[2, 2] < 0  ||
                        matrix[2, 3] < 0  ||
                        matrix[3, 0] < 1  ||
                        matrix[3, 1] < 1  ||
                        matrix[3, 2] != 0 ||
                        matrix[3, 3] < 0  ) testForm = false;
                    break;
            }
        }

        return testForm;
    }
}
