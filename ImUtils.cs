using System;
using System.Collections.Generic;
using OpenCvSharp;


namespace BarrierX_NagiBot
{
    class ImUtils
    {
        public static double Distance(Point2f ptA, Point2f ptB)
        {
            // http://qaru.site/questions/525580/opencv-c-distance-between-two-points
            // http://www.studyguide.ru/note.php?id=14
            // https://www.calc.ru/Formula-Dliny-Otrezka.html
            // http://www.cyberforum.ru/csharp-beginners/thread1140305.html

            return Math.Sqrt(Math.Pow(ptA.X - ptB.X, 2) + Math.Pow(ptA.Y - ptB.Y, 2));
        }


        // return the coordinates in top-left, top-right,
        // bottom-right, and bottom-left order
        public static Point2f[] OrderPoints(Point2f[] pts) {
            // https://www.pyimagesearch.com/2016/03/21/ordering-coordinates-clockwise-with-python-and-opencv/

            // sort the points based on their x-coordinates
            Point2f[] xSorted = new Point2f[4];
            Point2f tmp;
            for (int i = 0; i < 4; i++) {
                xSorted[i] = pts[i];
            }

            float xmin = xSorted[0].X;
            int imin = 0;
            for (int i1 = 0; i1 < 3; i1++)
            {
                imin = i1;
                xmin= xSorted[i1].X;
                for (int i2 = i1 + 1; i2 < 4; i2++)
                {
                    if (xmin > xSorted[i2].X)
                    {
                        imin = i2;
                        xmin = xSorted[i2].X;
                    }
                }
                if (imin != i1)
                {
                    tmp = xSorted[i1];
                    xSorted[i1] = xSorted[imin];
                    xSorted[imin] = tmp;
                }
                
            }

            // grab the left-most and right-most points from the sorted
            // x-roodinate points
            Point2f tl = xSorted[0];
            Point2f bl = xSorted[1];
            if (xSorted[0].Y > xSorted[1].Y) {
                tl = xSorted[1];
                bl = xSorted[0];
            }

            Point2f tr = xSorted[2];
            Point2f br = xSorted[3];
            if (xSorted[2].Y > xSorted[3].Y)
            {
                tr = xSorted[3];
                br = xSorted[2];
            }

            // return the coordinates in top-left, top-right,
            // bottom-right, and bottom-left order
            xSorted[0] = tl;
            xSorted[1] = tr;
            xSorted[2] = br;
            xSorted[3] = bl;

            return xSorted;
        }



        public static Mat FourPointTransform(Mat image, Point2f[] pts) {
            // obtain a consistent order of the points and unpack them
            // individually
            Point2f[] rect = OrderPoints(pts);
            Point2f tl = rect[0];
            Point2f tr = rect[1];
            Point2f br = rect[2];
            Point2f bl = rect[3];

            // compute the width of the new image, which will be the
            // maximum distance between bottom-right and bottom-left
            // x-coordiates or the top-right and top-left x-coordinates
            //widthA = np.sqrt(((br[0] - bl[0]) * *2) + ((br[1] - bl[1]) * *2))
            double widthA = Distance(br, bl);
            double widthB = Distance(tr, tl);
            int maxWidth = (int)(widthA>widthB?widthA:widthB);

            // compute the height of the new image, which will be the
            // maximum distance between the top-right and bottom-right
            // y-coordinates or the top-left and bottom-left y-coordinates
            double heightA = Distance(tr, br);
            double heightB = Distance(tl, bl);
            int maxHeight = (int)(heightA > heightB ? heightA : heightB);

            // now that we have the dimensions of the new image, construct
            // the set of destination points to obtain a "birds eye view",
            // (i.e. top-down view) of the image, again specifying points
            // in the top-left, top-right, bottom-right, and bottom-left
            // order
            Point2f[] dst = new Point2f[4];
            dst[0] = new Point2f(0, 0);
            dst[1] = new Point2f(maxWidth - 1, 0);
            dst[2] = new Point2f(maxWidth - 1, maxHeight - 1);
            dst[3] = new Point2f(0, maxHeight - 1);

            // compute the perspective transform matrix and then apply it
            //M = cv2.getPerspectiveTransform(rect, dst)
            //warped = cv2.warpPerspective(image, M, (maxWidth, maxHeight))
            Mat M = Cv2.GetPerspectiveTransform(rect, dst);
            Mat warped = new Mat();
            Cv2.WarpPerspective(image, warped, M, new Size(maxWidth, maxHeight));

            // return the warped image
            return warped;
        }

        public static Mat FourPointTransformFast(Mat image, Point2f[] orderedPts, int width, int height)
        {
            // now that we have the dimensions of the new image, construct
            // the set of destination points to obtain a "birds eye view",
            // (i.e. top-down view) of the image, again specifying points
            // in the top-left, top-right, bottom-right, and bottom-left
            // order
            Point2f[] dst = new Point2f[4];
            dst[0] = new Point2f(0, 0);
            dst[1] = new Point2f(width - 1, 0);
            dst[2] = new Point2f(width - 1, height - 1);
            dst[3] = new Point2f(0, height - 1);

            // compute the perspective transform matrix and then apply it
            //M = cv2.getPerspectiveTransform(rect, dst)
            //warped = cv2.warpPerspective(image, M, (maxWidth, maxHeight))
            Mat M = Cv2.GetPerspectiveTransform(orderedPts, dst);
            Mat warped = new Mat();
            Cv2.WarpPerspective(image, warped, M, new Size(width, height));

            // return the warped image
            return warped;
        }




        public static void PolylinesPoints2f(Mat img, Point2f[] points2f, Scalar color)
        {
            // https://stackoverflow.com/questions/35969667/how-to-use-the-opencvsharp-3-polylines-fillpoly-library-function
            List<List<Point>> listOfListOfPoint = new List<List<Point>>();
            List<Point> points = new List<Point>();
            listOfListOfPoint.Add(points);
            for (int i = 0; i < points2f.Length; i++)
            {
                Point p = new Point((int)points2f[i].X, (int)points2f[i].Y);
                points.Add(p);
            }

            Cv2.Polylines(img, listOfListOfPoint, true, color, 1);
        }


    }
}
