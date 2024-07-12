namespace Autodesk.InventorUtils.Extensions;
public static class ViewExtensions
{
    /// <summary>
    /// Gets the top, bottom, left and right most lines of the view respectively
    /// </summary>
    /// <param name="oView"></param>
    /// <returns></returns>
    public static Dictionary<CurveExtensions.CurveExtentDirection, DrawingCurve> GetExtentCurves(this DrawingView oView)
    {
        DrawingCurve topMostLine = null;
        DrawingCurve bottomMostLine = null;
        DrawingCurve leftMostLine = null;
        DrawingCurve rightMostLine = null;

        CurveExtensions.CurveExtentDirection curveExtentTop = CurveExtensions.CurveExtentDirection.Top;
        CurveExtensions.CurveExtentDirection curveExtentBottom = CurveExtensions.CurveExtentDirection.Bottom;
        CurveExtensions.CurveExtentDirection curveExtentLeft = CurveExtensions.CurveExtentDirection.Left;
        CurveExtensions.CurveExtentDirection curveExtentRight = CurveExtensions.CurveExtentDirection.Right;

        //init return value
        Dictionary<CurveExtensions.CurveExtentDirection, DrawingCurve> extentCurves = new();
        Application invapp = (Application)oView.Parent.Parent.Parent;

        foreach (DrawingCurve oDrawingCurve in oView.DrawingCurves)
        {         
            if (!oDrawingCurve.IsHorizontal(default, true) && !oDrawingCurve.IsVertical(default, true))
            { continue; }

            topMostLine=CurveExtensions.GetExtentCurve(topMostLine, oDrawingCurve, curveExtentTop);
            bottomMostLine=CurveExtensions.GetExtentCurve(bottomMostLine, oDrawingCurve, curveExtentBottom);
            leftMostLine=CurveExtensions.GetExtentCurve(leftMostLine, oDrawingCurve, curveExtentLeft);
            rightMostLine=CurveExtensions.GetExtentCurve(rightMostLine, oDrawingCurve, curveExtentRight);           
        }

        //construct return value
        extentCurves.Add(curveExtentTop, topMostLine);
        extentCurves.Add(curveExtentBottom, bottomMostLine);
        extentCurves.Add(curveExtentLeft, leftMostLine);
        extentCurves.Add(curveExtentRight, rightMostLine);

        return extentCurves;

    }

}
