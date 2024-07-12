namespace Autodesk.InventorUtils.Extensions;

public static class MathExtensions
{

    //override equals method
    public static bool IsEqual(this double d1, double d2, double tolerance = 0.001)
    {
        return Math.Abs(d1 - d2) < tolerance;
    }
}


