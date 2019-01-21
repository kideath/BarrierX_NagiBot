using System;
using OpenCvSharp;

namespace BarrierX_NagiBot
{
    class ImUtils
    {
        public static double distance(Point2f ptA, Point2f ptB)
        {
            // http://qaru.site/questions/525580/opencv-c-distance-between-two-points
            // http://www.studyguide.ru/note.php?id=14
            // https://www.calc.ru/Formula-Dliny-Otrezka.html
            // http://www.cyberforum.ru/csharp-beginners/thread1140305.html

            return Math.Sqrt(Math.Pow(ptA.X - ptB.X, 2) + Math.Pow(ptA.Y - ptB.Y, 2));
        }
    }
}
