using Inventor;

namespace Autodesk.InventorUtils.Extensions;

public static class CurveExtensions
{

    public static double CalculateSimilarity(this DrawingCurve curve1, DrawingCurve curve2, int decimalPlaces = 2)
    {
        Application invapp = (Application)curve1.Parent.Parent.Parent.Parent;
        curve1.OverrideColor=invapp.TransientObjects.CreateColor(255, 0, 0);
        curve2.OverrideColor=invapp.TransientObjects.CreateColor(255, 0, 0);


        double weightParent = 0.1;
        double weightCurveType = 0.3;
        double weightLength = 0.5;
        double weightSlope = 0.5;
        double weightEndpoints = 0.02;

        // Normalize weights
        double totalWeight = weightParent + weightCurveType + weightLength + weightSlope + weightEndpoints;
        weightParent /= totalWeight;
        weightCurveType /= totalWeight;
        weightLength /= totalWeight;
        weightSlope /= totalWeight;
        weightEndpoints /= totalWeight;

        double similarityScore = 0.0;

        // Check if the curves belong to the same parent view
        if (curve1.Parent == curve2.Parent)
        {
            similarityScore += weightParent;
        }

        // Check if the curves are of the same type
        if (curve1.CurveType == curve2.CurveType)
        {
            similarityScore += weightCurveType;
        }

        // Calculate length similarity
        double length1 = curve1.GetLength(decimalPlaces);
        double length2 = curve2.GetLength(decimalPlaces);
        double lengthDifference = Math.Abs(length1 - length2);
        double lengthSimilarity = 1.0 - (lengthDifference / Math.Max(length1, length2));
        similarityScore += weightLength * lengthSimilarity;

        // Calculate slope similarity for straight lines
        if (curve1.IsStraightLine() && curve2.IsStraightLine())
        {
            double slope1 = Math.Abs(curve1.GetSlope(decimalPlaces));
            double slope2 = Math.Abs(curve2.GetSlope(decimalPlaces));
            double slopeDifference = Math.Abs(slope1 - slope2);
            double slopeSimilarity = 1.0 - (slopeDifference / Math.Max(Math.Abs(slope1), Math.Abs(slope2)));
            similarityScore += weightSlope * slopeSimilarity;
        }

        // Calculate endpoint similarity
        double endpointSimilarity = CalculateEndpointSimilarity(curve1, curve2, decimalPlaces);
        similarityScore += weightEndpoints * endpointSimilarity;

        //reset color
        curve1.OverrideColor=null;
        curve2.OverrideColor=null;

        return Math.Round(similarityScore, decimalPlaces);
    }

    private static double CalculateEndpointSimilarity(DrawingCurve curve1, DrawingCurve curve2, int decimalPlaces)
    {
        double tolerance = Math.Pow(10, -decimalPlaces);
        bool startPointsEqual = curve1.StartPoint.IsEqualTo(curve2.StartPoint, tolerance);
        bool endPointsEqual = curve1.EndPoint.IsEqualTo(curve2.EndPoint, tolerance);

        if (startPointsEqual && endPointsEqual)
        {
            return 1.0;
        }
        else if (startPointsEqual || endPointsEqual)
        {
            return 0.5;
        }
        else
        {
            return 0.0;
        }
    }




    /// <summary>
    /// Check if the curve is vertical or not by comparing the X values of the start and end points
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="rdp">round decimal places</param>
    public static bool IsVertical(this DrawingCurve oDrawingCurve, int rdp = 4, bool useProjectedCurve = false)
    {
        //check if the curve is projected and a circle        
        bool IsCircle = oDrawingCurve.IsCircle();
        bool IsProjectedVertical = IsCircle && oDrawingCurve.IsRangeBoxVertical();

        bool IsVertical = !IsCircle && Math.Round(oDrawingCurve.StartPoint.X, rdp)==Math.Round(oDrawingCurve.EndPoint.X, rdp);

        return oDrawingCurve.IsStraightLine(useProjectedCurve)&&
             (IsProjectedVertical|| IsVertical);
    }
    /// <summary>
    /// Check if the curve is horizontal or not by comparing the Y values of the start and end points
    /// also checks if the projected curve is horizontal (for example, a circle can be projected to be a horizontal line)
    /// </summary>
    /// <param name="oDrawingCurve"></param>
    /// <param name="rdp"></param>
    /// <returns></returns>
    public static bool IsHorizontal(this DrawingCurve oDrawingCurve, int rdp = 4, bool useProjectedCurve = false)
    {
        //check if the line is straight (both curve and its projected type)
        //if it is then check if it is a circle--if its a circle then we 
        bool IsCircle = oDrawingCurve.IsCircle();
        bool IsProjectedHorizontal = IsCircle && oDrawingCurve.IsRangeBoxHorizontal();
        bool IsHorizontal = !IsCircle && Math.Round(oDrawingCurve.StartPoint.Y, rdp)==Math.Round(oDrawingCurve.EndPoint.Y, rdp);

        return oDrawingCurve.IsStraightLine(useProjectedCurve) &&
            (IsProjectedHorizontal || IsHorizontal);
    }

    /// <summary>
    /// check to see if the rangebox of the curve is horizontal
    /// This is useful for checking if the projected circle is horizontal
    /// This also works for straight curves
    /// </summary>
    /// <param name="oDrawingCurve"></param>
    /// <param name="rdp"></param>
    /// <returns></returns>
    public static bool IsRangeBoxHorizontal(this DrawingCurve oDrawingCurve, int rdp = 4)
    {
        return Math.Round(oDrawingCurve.Evaluator2D.RangeBox.MaxPoint.Y, rdp)==Math.Round(oDrawingCurve.Evaluator2D.RangeBox.MinPoint.Y, rdp);
    }

    public static bool IsRangeBoxVertical(this DrawingCurve oDrawingCurve)
    {
        return oDrawingCurve.Evaluator2D.RangeBox.MaxPoint.X==oDrawingCurve.Evaluator2D.RangeBox.MinPoint.X;
    }

    public static bool IsStraightLine(this DrawingCurve oDrawingCurve, bool useProjectedCurve = false)
    {
        return oDrawingCurve.CurveType==CurveTypeEnum.kLineSegmentCurve ||(useProjectedCurve && oDrawingCurve.ProjectedCurveType==Curve2dTypeEnum.kLineSegmentCurve2d);
    }

    /// <summary>
    /// returns the slope of the line provided the line is straight
    /// </summary>
    /// <param name="oDrawingCurve"></param>
    /// <returns></returns>
    public static double GetSlope(this DrawingCurve oDrawingCurve, int dp = 2)
    {
        return Math.Round(Math.Round((oDrawingCurve.EndPoint.Y-oDrawingCurve.StartPoint.Y), 4)/Math.Round((oDrawingCurve.EndPoint.X-oDrawingCurve.StartPoint.X), 4), dp);
    }

    public static bool IsParallel(this DrawingCurve oDrawingCurve1, DrawingCurve oDrawingCurve2)
    {
        if (!oDrawingCurve1.IsStraightLine() || !oDrawingCurve2.IsStraightLine())
        {
            return false;
        }
        //check if the lines are parallel
        return oDrawingCurve1.GetSlope()==oDrawingCurve2.GetSlope() || oDrawingCurve1.IsVertical() && oDrawingCurve2.IsVertical() ||
               oDrawingCurve1.IsHorizontal() && oDrawingCurve2.IsHorizontal();
    }
    /// <summary>
    /// Check if the curve is a circle
    /// </summary>
    /// <param name="oDrawingCurve"></param>
    /// <returns></returns>
    public static bool IsCircle(this DrawingCurve oDrawingCurve)
    {
        return oDrawingCurve.CurveType==CurveTypeEnum.kCircleCurve;
    }

    public static double HoleDiameter(this DrawingCurve oDrawingCurve, int rdp = 3)
    {
        return Math.Round(2* ((dynamic)((Edge)oDrawingCurve.ModelGeometry).Geometry).Radius, rdp);
    }

    public static Point GetCircleCenterPoint(this DrawingCurve oDrawingCurve)
    {
        return ((dynamic)((Edge)oDrawingCurve.ModelGeometry).Geometry).Center;
    }

    public static bool IsArc(this DrawingCurve oDrawingCurve, bool projectedArcOnly = true)
    {
        if (projectedArcOnly)
        {
            return oDrawingCurve.ProjectedCurveType==Curve2dTypeEnum.kCircularArcCurve2d;
        }
        return oDrawingCurve.CurveType==CurveTypeEnum.kCircularArcCurve ||
              oDrawingCurve.ProjectedCurveType==Curve2dTypeEnum.kCircularArcCurve2d;

    }

    public static double GetArcAngle(this DrawingCurve oCurve)
    {
        // Get the start, end, and center points of the arc
        Point2d startPoint = oCurve.StartPoint;
        Point2d endPoint = oCurve.EndPoint;
        Point2d centerPoint = oCurve.CenterPoint;

        // Calculate vectors from the center to the start and end points
        Vector2d vectorStart = centerPoint.VectorTo(startPoint);
        Vector2d vectorEnd = centerPoint.VectorTo(endPoint);

        // Calculate the angle between the vectors in radians
        double angle = vectorStart.AngleTo(vectorEnd);

        // Convert the angle to degrees
        double angleDegrees = angle * (180.0 / Math.PI);

        return Math.Round(angleDegrees, 0);
    }

    public static bool IsSemiCircle(this DrawingCurve oCurve)
    {

        return oCurve.GetArcAngle()==180;
    }

    /// <summary>
    /// Check if 2 lines are collinear. Lines are considered collinear if they share the same slope and at least one point on the extended infinite line.
    /// </summary>
    /// <param name="oDrawingCurve1"></param>
    /// <param name="oDrawingCurve2"></param>
    /// <returns></returns>
    public static bool IsCollinear(this DrawingCurve oDrawingCurve1, DrawingCurve oDrawingCurve2)
    {
        if (!oDrawingCurve1.IsStraightLine() || !oDrawingCurve2.IsStraightLine())
        {
            return false;
        }

        // Check for equal slopes (avoid floating-point imprecision).
        //const double tolerance = 1e-6;
        if (oDrawingCurve1.IsParallel(oDrawingCurve2))
        {
            // Lines with same slope might be collinear even without shared endpoints.
            // Check if one line's start/end point lies on the other line.
            var pointOnLine1 = oDrawingCurve1.IsPointOnExtendedLine(oDrawingCurve2.StartPoint);
            var pointOnLine2 = oDrawingCurve1.IsPointOnExtendedLine(oDrawingCurve2.EndPoint);
            return pointOnLine1 || pointOnLine2;
        }

        return false;
    }

    /// <summary>
    /// Check if a point lies on the extended line (infinite line) defined by the curve.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static bool IsPointOnExtendedLine(this DrawingCurve line, Point2d point)
    {
        // Check if the point lies exactly on the line segment (start & end points).
        if (line.StartPoint.IsEqualTo(point) || line.EndPoint.IsEqualTo(point))
        {
            return true;
        }

        // Otherwise, check if the point falls within a reasonable tolerance from the infinite line equation.
        // This approach avoids issues with line segments having zero length.

        // Get line direction vector (assuming it's calculated elsewhere).
        var direction = line.StartPoint.VectorTo(line.EndPoint);

        // Normalize the direction vector (assuming a Normalize method exists).
        direction.Normalize();

        // Calculate the vector from the line's start point to the point.
        var pointVector = point.VectorTo(line.StartPoint);

        // Project the point vector onto the direction vector.
        var projection = pointVector.DotProduct(direction);

        // Calculate the distance from the line (assuming DistanceTo exists).
        var distanceFromLine = Math.Abs(
            pointVector.X * direction.Y - pointVector.Y * direction.X
        );

        // Check if the distance is within a tolerance (adjust tolerance as needed).
        const double tolerance = 1e-6;
        return distanceFromLine <= tolerance;
    }

    /// <summary>
    /// Check if 2 lines overlap. Lines are considered overlapping if they share the same slope and at least one point.
    /// </summary>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <returns></returns>
    public static bool Overlaps(this DrawingCurve line1, DrawingCurve line2)
    {
        // Lines are considered fully overlapping if:
        // 1. They have the same slope (including zero slope for horizontal/vertical lines).
        // 2. They share both start and end points.

        if (line1.IsParallel(line2)) // Adjust tolerance as needed
        {
            // Check if any part of one line lies on the other
            return (PointOnLine(line1.StartPoint, line2) || PointOnLine(line1.EndPoint, line2) ||
                            PointOnLine(line2.StartPoint, line1) || PointOnLine(line2.EndPoint, line1));

        }

        return false;
    }

    /// <summary>
    /// Check if a point lies on a line segment
    /// </summary>
    /// <param name="point"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    static bool PointOnLine(Point2d point, DrawingCurve line)
    {
        // Check if a point lies on a line segment
        Vector2d v1 = line.EndPoint.VectorTo(line.StartPoint);
        Vector2d v2 = point.VectorTo(line.StartPoint);

        //if v2 is 0, then the point is the start point of the line
        if (v2.Length==0)
        {
            return true;
        }

        double dotProduct = v1.DotProduct(v2);
        double mag1 = v1.Length;
        double mag2 = v2.Length;

        // Tolerance for comparing dot products
        double tolerance = 0.001;

        return Math.Abs(dotProduct / (mag1 * mag2) - 1) < tolerance;
    }

    public static double GetLength(this DrawingCurve oDrawingCurve, int rdp = 2)
    {
        Inventor.DrawingView oView = oDrawingCurve.Parent;
        double MinParam;
        double MaxParam;
        double scaleLength;
        double actualLength;
        //use 2d evaluator
        oDrawingCurve.Evaluator2D.GetParamExtents(out MinParam, out MaxParam);

        oDrawingCurve.Evaluator2D.GetLengthAtParam(MinParam, MaxParam, out scaleLength);
        actualLength=Math.Round(scaleLength/oView.Scale, rdp);

        return actualLength;

    }

    /// <summary>
    /// Gets the adjacent straight curves to the arc, optionally check adjacent curves to lines as well
    /// This will always skip circles i.e. it wont check for adajacent curves to a circle curve.
    /// </summary>
    /// <param name="oMainCurve"></param>
    /// <returns></returns>
    public static List<DrawingCurve> GetAdjacentCurves(this DrawingCurve oMainCurve, bool projectedArcOnly = true, bool useArcOnly = true)
    {
        List<DrawingCurve> adjacentCurves = new List<DrawingCurve>();
        //only run the check if the curve is an arc--if asked to check for arcs only
        //always--if the curve is a circle, then dont perform the check
        if ((!oMainCurve.IsArc(projectedArcOnly) && useArcOnly)|| oMainCurve.IsCircle())
        {
            return adjacentCurves;
        }
        Inventor.DrawingView oView = oMainCurve.Parent;
        DrawingCurve firstline = null;
        DrawingCurve thirdline = null;

        TransientGeometry oTG = ((Inventor.Application)((DrawingDocument)oView.Parent.Parent).Parent).TransientGeometry;

        var startpoint = oTG.CreatePoint2d(Math.Round(oMainCurve.StartPoint.X, 4), Math.Round(oMainCurve.StartPoint.Y, 4));
        var endpoint = oTG.CreatePoint2d(Math.Round(oMainCurve.EndPoint.X, 4), Math.Round(oMainCurve.EndPoint.Y, 4));
        //find the lines that are connected to the arc
        foreach (DrawingCurve oDrawingCurve2 in oView.DrawingCurves)
        {
            if (oDrawingCurve2==oMainCurve)
                continue;
            if (!oDrawingCurve2.IsStraightLine() && useArcOnly)
                continue;

            //check in startpoint and endpoint are nulls
            if (oDrawingCurve2.StartPoint==null || oDrawingCurve2.EndPoint==null)
                continue;

            //round to 4 decimal places
            var startpoint2 = oTG.CreatePoint2d(Math.Round(oDrawingCurve2.StartPoint.X, 4), Math.Round(oDrawingCurve2.StartPoint.Y, 4));
            var endpoint2 = oTG.CreatePoint2d(Math.Round(oDrawingCurve2.EndPoint.X, 4), Math.Round(oDrawingCurve2.EndPoint.Y, 4));

            //compare both start and end points using both the x and y values
            if (startpoint.IsEqualTo(startpoint2) || startpoint.IsEqualTo(endpoint2))
            {
                firstline=oDrawingCurve2;
            }
            if (endpoint.IsEqualTo(startpoint2) || endpoint.IsEqualTo(endpoint2))
            {
                thirdline=oDrawingCurve2;
            }
            //exit the loop if both lines are found
            if (firstline!=null && thirdline!=null)
                break;
        }
        adjacentCurves.Add(firstline);
        adjacentCurves.Add(thirdline);

        return adjacentCurves;
    }
    public static DrawingCurve GetLongerCurve(DrawingCurve oDrawingCurve1, DrawingCurve oDrawingCurve2)
    {
        DrawingCurve longerCurve = oDrawingCurve2;
        double length1 = oDrawingCurve1.GetLength();
        double length2 = oDrawingCurve2.GetLength();
        if (length1 > length2)
        {
            longerCurve = oDrawingCurve1;
        }

        return longerCurve;
    }

    /// <summary>
    /// Compare 2 cuves and return the one that's on the farthest extent in the specified direction.
    /// </summary>
    /// <param name="oDrawingCurve1"></param>
    /// <param name="oDrawingCurve2"></param>
    /// <param name="extentDirection"></param>
    /// <returns></returns>
    public static DrawingCurve GetExtentCurve(DrawingCurve oDrawingCurve1, DrawingCurve oDrawingCurve2, CurveExtentDirection extentDirection)
    {
        //check for nulls and return the not null curvbe
        if (oDrawingCurve1==null)
        {
            return oDrawingCurve2;
        }
        if (oDrawingCurve2==null)
        {
            return oDrawingCurve1;
        }

        DrawingCurve farthestCurve = null;// oDrawingCurve2;

        //depending on the direction, either check for X or Y values


        double x1mid = oDrawingCurve1.GetCurveCenterPoint();
        double x2mid = oDrawingCurve2.GetCurveCenterPoint();


        if (extentDirection==CurveExtentDirection.Top || extentDirection==CurveExtentDirection.Bottom)
        {
            x1mid =oDrawingCurve1.GetCurveCenterPoint(false);// Math.Round(oDrawingCurve1.MidPoint.Y, 4);
            x2mid =oDrawingCurve2.GetCurveCenterPoint(false);// Math.Round(oDrawingCurve2.MidPoint.Y, 4);
        }

        if (extentDirection==CurveExtentDirection.Right||extentDirection==CurveExtentDirection.Top)
        {
            //find the highest values
            if (x1mid >= x2mid)
            {
                farthestCurve= oDrawingCurve1;
                if (x1mid==x2mid)
                {
                    //slect the longer one
                    farthestCurve= GetLongerCurve(oDrawingCurve1, oDrawingCurve2);
                }
            }
            else
            {
                farthestCurve= oDrawingCurve2;
            }
        }
        else
        {
            //find the lowest values
            if (x1mid <= x2mid)
            {
                farthestCurve= oDrawingCurve1;
                if (x1mid==x2mid)
                {
                    //slect the longer one
                    farthestCurve= GetLongerCurve(oDrawingCurve1, oDrawingCurve2);
                }
            }
            else
            {
                farthestCurve= oDrawingCurve2;
            }
        }

        return farthestCurve;
    }

    public static double GetCurveCenterPoint(this DrawingCurve oDrawingCurve, bool isX = true)
    {
        bool IsCircle1 = oDrawingCurve.IsCircle();
        //var center1 = oDrawingCurve.GetCircleCenterPoint();
        double x1mid = 0;

        if (IsCircle1)
        {

            //var center1 = oDrawingCurve.GetCircleCenterPoint();
            x1mid =isX ? Math.Round(oDrawingCurve.CenterPoint.X, 4) : Math.Round(oDrawingCurve.CenterPoint.Y, 4);
        }
        else
        {
            x1mid =isX ? Math.Round(oDrawingCurve.MidPoint.X, 4) : Math.Round(oDrawingCurve.MidPoint.Y, 4);
        }

        return x1mid;

    }

    public static Point2d GetCurveCenterPoint2d(this DrawingCurve oDrawingCurve)
    {
        bool IsCircle1 = oDrawingCurve.IsCircle();


        if (IsCircle1)
        {

            //var center1 = oDrawingCurve.GetCircleCenterPoint();
            return oDrawingCurve.CenterPoint;
        }
        else
        {
            return oDrawingCurve.MidPoint;
        }


    }

    public enum CurveExtentDirection
    {
        Right,
        Left,
        Top,
        Bottom
    }
}
