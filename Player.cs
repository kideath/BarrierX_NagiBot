using OpenCvSharp;

namespace BarrierX_NagiBot
{
    class Player
    {
        public Mat contour;
        public double area;
        public Mat points_approx; // точки контура
        public Point center;
        public double ratio; // width / height to skip lines

        public Point pt_up;
        public Point pt_left;
        public Point pt_right;

        public Player() { }

        public Player(GameObject obj)
        {
            contour = obj.contour;
            area = obj.area;
            points_approx = obj.points_approx;
            center = obj.center;
            ratio = obj.ratio;
        }

    }
}
