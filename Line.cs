using OpenCvSharp;

namespace BarrierX_NagiBot
{
    class Line
    {
        public Mat mask;
        public Mat contour;
        public Mat approx;
        public Scalar color;

        public Point pt_up;
        public Point pt_left;
        public Point pt_right;

        public Line() { }
        public Line(Mat mask1, Mat contour1, Mat approx1, Scalar color1, Point pt_up1, Point pt_left1, Point pt_right1)
        {
            mask = mask1;
            contour = contour1;
            approx = approx1;
            color = color1;
            pt_up = pt_up1;
            pt_left = pt_left1;
            pt_right = pt_right1;
        }
    }
}
