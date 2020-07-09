/// <summary>
/// Class containing the functions that set the parameters of GenGroup.
/// To set the TestForm, see TestFormFuncs.
/// 
/// Functions are of the form pf the delegate GroupParams.
/// </summary>

public static class GroupParamFuncs
{
    public static void SphA2()
    {
        GenGroup.affine = false;
        GenGroup.dim = 3;
        GenGroup.orderW = 6;
        GenGroup.rank = 2;

        GenGroup.s0 = new double[,] 
        { { 0d, 1d, 0d}, { 1d, 0d, 0d}, { 0d, 0d, 1d } };
        GenGroup.s1 = new double[,] 
        { { 1d, 0d, 0d }, { 0d, 0d, 1d }, { 0d, 1d, 0d } };
    }

    public static void AffA2()
    {
        GenGroup.affine = true;
        GenGroup.dim = 3;
        GenGroup.rank = 3;

        GenGroup.s0 = new double[,] 
        { { 0d, 1d, 0d }, { 1d, 0d, 0d }, { 0d, 0d, 1d } };
        GenGroup.s1 = new double[,] 
        { { 1d, 0d, 0d }, { 0d, 0d, 1d }, { 0d, 1d, 0d } };
        GenGroup.s2 = new double[,] 
        { { 0d, 0d, 1d / ((double)GenGroup.resChar) }, { 0d, 1d, 0d }, { (double)GenGroup.resChar, 0d, 0d } };
    }

    public static void SphA3()
    {
        GenGroup.affine = false;
        GenGroup.dim = 4;
        GenGroup.orderW = 24;
        GenGroup.rank = 3;

        GenGroup.s0 = new double[,] 
        { { 0d, 1d, 0d, 0d }, { 1d, 0d, 0d, 0d }, { 0d, 0d, 1d, 0d }, { 0d, 0d, 0d, 1d } };
        GenGroup.s1 = new double[,] 
        { { 1d, 0d, 0d, 0d }, { 0d, 0d, 1d, 0d }, { 0d, 1d, 0d, 0d }, { 0d, 0d, 0d, 1d } };
        GenGroup.s2 = new double[,] 
        { { 1d, 0d, 0d, 0d }, { 0d, 1d, 0d, 0d }, { 0d, 0d, 0d, 1d }, { 0d, 0d, 1d, 0d } };
    }

}
