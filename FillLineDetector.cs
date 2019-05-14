using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
//using System.Runtime.InteropServices;
//using System.Drawing;
//using System.Drawing.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.XFeatures2D;
using OpenCvSharp.Text;

namespace BarrierX_NagiBot
{
    class FillLineDetector
    {
        public void Init(OCR ocr, Config config)
        {
            this.ocr = ocr;
            this.config = config;

            screenCenter = new Point(config.Width / 2, config.Height / 2);
            screenLeft = new Point(0, config.Height / 2);
            screenRight = new Point(config.Width, config.Height / 2);
        }

        public Player player = null;
        public Line line_center = null;
        public Line line_left = null;
        public Line line_right = null;
        public Player GetPlayer() { return player; }
        public Line GetLineCenter() { return line_center; }
        public Line GetLineLeft() { return line_left; }
        public Line GetLineRight() { return line_right; }

        Mat img_debug = null;
        public Mat Get_img_debug() { return img_debug; }

        private OCR ocr = null;
        private Config config = null;

        public static Point screenCenter;
        public static Point screenLeft;
        public static Point screenRight;

        public void ProcessFrame(Mat source)
        {
            Mat img = source;
            
            //Mat img_debug= null;
            bool skip = false;

            line_center = null;
            line_left = null;
            line_right = null;

            if (config.Preview != 0)
            {
                img_debug = new Mat();
                img.CopyTo(img_debug);
                //Cv2.Circle(img_debug, screenCenter, 5, new Scalar(255, 255, 0), -1);
                Cv2.Circle(img_debug, new Point(320, 240), 5, new Scalar(255, 255, 0), -1);
            }


            List<GameObject> list_obj = FindByWhiteMask(img, img_debug); // определение белых контуров
            player = FindPlayer(list_obj);  // поиск игрока
                                                   //Player player = null;
            ocr.FindTimer(mask_white, list_obj);
            double angle = GetAngle(img, img_debug, list_obj);
            if (angle != 0)
            {
                if (angle <= -45) angle += 90;
                else if (angle >= 45) angle -= 90;
                //angle = -8;

                // https://docs.opencv.org/3.0-beta/doc/py_tutorials/py_imgproc/py_geometric_transformations/py_geometric_transformations.html
                Mat M = Cv2.GetRotationMatrix2D(new Point2f(img.Cols / 2, img.Rows / 2), angle, 1);
                Mat dest = new Mat();
                Cv2.WarpAffine(img, dest, M, new Size(config.Width, config.Height));

                Cv2.Line(dest, screenLeft, screenRight, Colors.green, 3);
                Cv2.Circle(dest, screenCenter, 5, new Scalar(255, 255, 0), -1);
                Cv2.ImShow("rotated", dest);


            }
            Console.WriteLine($"ANGLE= {angle}");

            //Mat grey = new Mat();
            //Cv2.CvtColor(img, grey, ColorConversionCodes.BGR2GRAY);
            //Cv2.Canny(grey, grey, 100, 100 * 2);
            //Cv2.ImShow("Canny", grey);

            if (player != null)
            {
                if (config.Preview > 0)
                {
                    Cv2.Circle(img_debug, player.pt_up, 5, new Scalar(255, 255, 0), -1); // purple
                    Cv2.Circle(img_debug, player.pt_left, 5, new Scalar(255, 0, 153), -1); // light blue
                    Cv2.Circle(img_debug, player.pt_right, 5, new Scalar(100, 0, 255), -1); // pink
                }
                Console.WriteLine($"Player found at {player.center}");
            }
            else
            {
                Console.WriteLine("SKIP: player not found");
                skip = true;
            }
            //rgb(153, 0, 255)


            //line_center = null;
            if (!skip)
            {
                // в зависимости от положения коробля определяю точку для определения линии
                //Point diff = new Point(player.pt_up.X, player.pt_up.Y - 10);

                //if (diff.X > screenCenter.X) { diff.X -= 10; }
                //else { diff.X += 10; }
                //Mat[] rgb = img.Split();
                //TestPointByMask(mask_white, diff);
                //TestPointByMask(mask_white, new Point(30, 30));

                int offsetX = (int)(10); if (player.pt_up.X > screenCenter.X) { offsetX = (int)(-10); }
                Point diff = GetCheckedPointByMask(mask_white, player.pt_up, offsetX, (int)(-10), false);

                //Cv2.ImShow("thresh2", mask_white);
                //TestPointByMask(rgb[2], diff);



                if (diff.X >= 0 && diff.X <= config.Width
                    && diff.Y >= 0 && diff.Y <= config.Height)
                {
                    line_center = FindLine(img, diff, img_debug, "center");
                }
                else
                {
                    Console.WriteLine("SKIP: line center");
                    skip = true;
                }

            }

            // считаю середину слева
            //line_left = null;
            if (!skip)
            {
                Point mid_left = new Point(
                    (line_center.pt_up.X + line_center.pt_left.X) / 2,
                    (line_center.pt_up.Y + line_center.pt_left.Y) / 2);

                // смещаю линию на 10 пикселей вверх и вбок
                //mid_left.X -= 30;
                //mid_left.Y -= 30;
                mid_left = GetCheckedPointByMask(line_center.mask, mid_left, (int)(-30), (int)(-30), true);


                if (mid_left.X >= 0 && mid_left.X <= config.Width
                    && mid_left.Y >= 0 && mid_left.Y <= config.Height)
                {
                    if (config.Preview > 0) { Cv2.Circle(img_debug, mid_left, 6, new Scalar(20, 70, 25), -1); }
                    line_left = FindLine(img, mid_left, img_debug, "left");
                }
                else
                {
                    Console.WriteLine("SKIP: line left");
                    skip = true;
                }


            }

            // считаю середину справа
            //line_right = null;
            if (!skip)
            {
                Point mid_right = new Point(
                    (line_center.pt_up.X + line_center.pt_right.X) / 2,
                    (line_center.pt_up.Y + line_center.pt_right.Y) / 2);

                // смещаю линию на 10 пикселей вверх и вбок
                //mid_right.X += 30;
                //mid_right.Y -= 30;
                mid_right = GetCheckedPointByMask(line_center.mask, mid_right, (int)(30), (int)(-30), true);

                if (mid_right.X >= 0 && mid_right.X <= config.Width
                    && mid_right.Y >= 0 && mid_right.Y <= config.Height)
                {
                    if (config.Preview > 0) { Cv2.Circle(img_debug, mid_right, 6, new Scalar(20, 70, 25), -1); }
                    line_right = FindLine(img, mid_right, img_debug, "right");
                }
                else
                {
                    Console.WriteLine("SKIP: line right");
                    skip = true;
                }





                mask_white.Release();
                if (player != null) { player.contour.Release(); }

            }
        }

        public Mat mask_white = null;

        public List<GameObject> FindByWhiteMask(Mat src, Mat preview)
        {
            // thresh = cv2.inRange(image, (210, 210, 210), (255, 255, 255))
            //var 
            mask_white = new Mat();
            //Cv2.InRange(src, new Scalar(150, 150, 150), new Scalar(255, 255, 255), mask_white);
            Cv2.InRange(src, new Scalar(210, 210, 210), new Scalar(255, 255, 255), mask_white);

            // cv2.imshow("edges", thresh)
            //_, cnts, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
            //thresh.DrawContours()

            Mat[] contours = null;
            Mat dst = new Mat();
            //Cv2.FindContours(edged, out contours, new Mat(), RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask_white, out contours, dst, RetrievalModes.List, ContourApproximationModes.ApproxNone);
            //Cv2.ImShow("dst", dst);
            //if (config.Preview == 4)
                Cv2.ImShow("thresh", mask_white);
            //dst.Release();
            //thresh.Release();

            List<GameObject> list_result = new List<GameObject>();

            if (contours != null)
            {
                Console.WriteLine("contours.size= " + contours.Length);

                for (int i = 0; i < contours.Length; i++)
                {
                    Mat contour = contours[i];
                    double area = Cv2.ContourArea(contour);

                    // if the contour is not sufficiently large, ignore it
                    if (area < 9) continue;

                    double peri = Cv2.ArcLength(contour, true);
                    Mat approx = contour.ApproxPolyDP(0.04 * peri, true);

                    // Point p2f= approx.Get<Point>(0, 1);
                    // Console.WriteLine("p2x= " + p2f.X + ", " + p2f.Y);
                    // Console.WriteLine("cols= "+ approx.Cols + ", rows= " + approx.Rows);
                    //
                    // Point[] points_approx = new Point[approx.Rows];
                    // for (int j = 0; j < approx.Rows; j++) {
                    //     points_approx[j] = approx.Get<Point>(0, j);
                    // }

                    if (config.Preview > 0) Cv2.Polylines(preview, approx, true, new Scalar(0, 255, 0), 1);

                    //Rect rect = Cv2.BoundingRect(contour);
                    //Cv2.Rectangle(preview, rect, new Scalar(0, 255, 0));

                    // https://csharp.hotexamples.com/examples/OpenCvSharp/RotatedRect/-/php-rotatedrect-class-examples.html
                    // https://stackoverflow.com/questions/43342199/draw-rotated-rectangle-in-opencv-c
                    RotatedRect rrect = Cv2.MinAreaRect(contour);
                    Point2f[] points2f = rrect.Points();
                    //points2f = ImUtils.OrderPoints(points2f);

                    if (config.Preview > 0) ImUtils.PolylinesPoints2f(preview, points2f, Colors.green);

                    // https://www.pyimagesearch.com/2016/03/21/ordering-coordinates-clockwise-with-python-and-opencv/
                    // # loop over the original points and draw them
                    //for (int i1 = 0; i1 < points2f.Length; i1++) {
                    //    Cv2.Circle(preview, new Point((int)points2f[i1].X, (int)points2f[i1].Y), 5, colors[i1], -1);
                    //}


                    double h = ImUtils.Distance(points2f[0], points2f[1]);
                    double w = ImUtils.Distance(points2f[0], points2f[3]);
                    double ratio = h / w;
                    if (h > w) ratio = w / h;

                    // compute the center of the contour
                    Moments m = Cv2.Moments(contour);
                    Point pt_center = new Point((int)(m.M10 / m.M00), (int)(m.M01 / m.M00));
                    if (this.config.Preview > 0) Cv2.Circle(preview, pt_center, 2, new Scalar(255, 0, 0), -1);


                    //Console.Write(", " + Cv2.ContourArea(contour));
                    // if (Cv2.ContourArea(contour) > max_val && Cv2.ContourArea(contour) < 408)
                    // {
                    //    max_val = Cv2.ContourArea(contour);
                    //    max_i = i;
                    //    //Cv2.DrawContours(image, contours, i, new Scalar(255, 128, 128), 5);
                    // }
                    //Cv2.DrawContours(image, contours, i, new Scalar(255, 128, 128), 5);

                    // https://metanit.com/sharp/tutorial/7.5.php
                    //Console.WriteLine($"object: area={(int)area,5}, pts_count={approx.Rows,3}, ratio= {ratio:f4}, center={pt_center}              ");

                    GameObject obj = new GameObject(contour, area, approx, pt_center, ratio, rrect);
                    if (ratio > 0.05)
                    {
                        obj.type = GameObject.ObjectType.unknown;
                        list_result.Add(obj);
                    }
                    else
                    {
                        //obj.type = GameObject.ObjectType.noise;
                    }



                }

                dst.Release();
                //mask_white.Release();
            }

            //if (configPreview!=0) {
            //    Cv2.DrawContours(preview, contours, -1, new Scalar(0, 255, 0), 2);
            //}

            return list_result;
        }



        public Player FindPlayer(List<GameObject> list_obj)
        {
            int i_player = -1;
            int max_y = 0;
            GameObject obj;
            for (int i = 0; i < list_obj.Count; i++)
            {
                obj = list_obj[i];
                if (obj.type != GameObject.ObjectType.unknown) continue;

                if (obj.center.Y > max_y             // объект должен быть снизу других
                    && obj.center.Y > screenCenter.Y // должен быть в нижней половине экрана
                    && obj.points_approx.Rows > 2    // у него должно быть более 2 точек
                    && obj.area > 100                // должен быть относительно крупный
                    )
                {
                    max_y = obj.center.Y;
                    i_player = i;
                }
            }

            if (i_player == -1)
            {
                return null;
            }

            obj = list_obj[i_player];
            obj.type = GameObject.ObjectType.player;
            Player player = new Player(obj);
            player.pt_up = NearestPoint(obj.points_approx, screenCenter);
            player.pt_left = NearestPoint(obj.points_approx, screenLeft);
            player.pt_right = NearestPoint(obj.points_approx, screenRight);
            return player;
        }

        // возвращает ближайшую к ptB точку
        public static Point NearestPoint(Mat points, Point ptB)
        {
            double[] dists = new double[points.Rows];
            int i_min = 0;
            for (int i = 0; i < points.Rows; i++)
            {
                dists[i] = ImUtils.Distance(points.Get<Point>(0, i), ptB);
                if (dists[i] < dists[i_min]) { i_min = i; }
            }

            return points.Get<Point>(0, i_min);
        }

        public Line FindLine(Mat src, Point seed, Mat preview, String window_name)
        {
            // https://stackoverflow.com/questions/44063407/opencvsharp-2-floodfill-is-broken-and-corrupts-output-mask
            Mat mask = new Mat(src.Rows + 2, src.Cols + 2, MatType.CV_8UC1, new Scalar(0, 0, 0, 0));
            // floodflags = 4
            // #floodflags |= cv2.FLOODFILL_MASK_ONLY
            // floodflags |= (255 << 8)
            //FloodFillFlags flags = FloodFillFlags.Link4 | FloodFillFlags.MaskOnly | FloodFillFlags.FixedRange;
            // num,im,mask,rect = cv2.floodFill(orig2, mask, seed, (255,255,0), (5,)*3, (5,)*3, floodflags)
            Rect rect = new Rect();
            int flags = (int)FloodFillFlags.Link4 | (int)FloodFillFlags.MaskOnly | (255 << 8);
            Cv2.FloodFill(src, mask, seed, new Scalar(255, 255, 0), out rect, new Scalar(2, 2, 2), new Scalar(2, 2, 2), flags);
            Mat mask2 = new Mat(mask, new Rect(new Point(1, 1), new Size(src.Cols, src.Rows)));
            mask.Release();

            Scalar color_line = Cv2.Mean(src, mask2);
            // Mat mat_color = new Mat(50, 250, MatType.CV_8UC3, color_line);
            // Cv2.ImShow("color", mat_color);

            if (config.Preview == 4) Cv2.ImShow(window_name, mask2);

            // (_, cnts2, _) = cv2.findContours(mask_roi, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
            Mat[] contours = null;
            Mat dst = new Mat();
            Cv2.FindContours(mask2, out contours, dst, RetrievalModes.External, ContourApproximationModes.ApproxNone);
            //Cv2.ImShow("dst", dst);
            dst.Release();

            //Console.WriteLine($"line_contour_len={contours.Length}");
            Mat approx = null;
            if (contours.Length > 0)
            {
                double peri = Cv2.ArcLength(contours[0], true);
                approx = contours[0].ApproxPolyDP(0.04 * peri, true);
                //Console.WriteLine($"points_n={approx.Rows}");
            }

            if (config.Preview > 0)
            {
                Cv2.DrawContours(preview, contours, -1, new Scalar(0, 255, 255), 1);
                Cv2.Polylines(preview, approx, true, new Scalar(255, 0, 255), 2);
            }

            Point line_up = NearestPoint(approx, screenCenter);
            Point line_left = NearestPoint(approx, screenLeft);
            Point line_right = NearestPoint(approx, screenRight);

            if (config.Preview > 0)
            {
                Cv2.Circle(preview, line_up, 3, new Scalar(120, 255, 120), -1);
                Cv2.Circle(preview, line_left, 3, new Scalar(120, 255, 120), -1);
                Cv2.Circle(preview, line_right, 3, new Scalar(120, 255, 120), -1);
            }

            //Cv2.ImShow("mask", mask2);
            //Cv2.ImShow("src", src);
            // Console.WriteLine($"mask_size={mask2.Cols} x {mask2.Rows}   ");
            Console.WriteLine($"line_contour_len={contours.Length}, points_n={approx.Rows}, color={color_line}      ");

            //mask2.Release();

            return new Line(mask2, contours[0], approx, color_line, line_up, line_left, line_right);
        }

        public Point GetCheckedPointByMask(Mat mask, Point pt, int offsetX, int offsetY, bool mask_fix)
        {

            Point res = new Point(pt.X + offsetX, pt.Y + offsetY);
            //string msg = "PT1=" + res.X + "," + res.Y;
            for (int retry = 0; retry < 3; retry++)
            {
                if (CheckPointByMask(mask, res) && CheckPointByMask(mask_white, res))
                {
                    //Console.WriteLine(msg + ", PT2=" + res.X + "," + res.Y+"              ");
                    return res;
                }
                else
                {
                    if (mask_fix && retry == 0)
                    {
                        int deltaX = 1; if (offsetX < 0) deltaX = -1;
                        int x = pt.X;
                        while (x >= 0 && x < mask.Cols && mask.Get<Boolean>(pt.Y, x)) x += deltaX;
                        pt.X = x;
                    }
                    res.X += offsetX;
                    //res.Y += offsetY;
                }
            }

            return new Point(-1, -1);
        }

        public static bool CheckPointByMask_debug(Mat mask, Point pt)
        {
            Console.WriteLine($"TYPE{mask.Type()}");
            for (int i = -3; i < 3; i++)
            {
                String s = "";
                for (int j = -3; j < 3; j++)
                {
                    s += ", " + mask.Get<Byte>(pt.Y + i, pt.X + j);
                }

                Console.WriteLine($"LINE{i}: {s}                                         ");
            }

            return true;
        }


        public static bool CheckPointByMask(Mat mask, Point pt)
        {
            //CheckPointByMask_debug(mask, pt);

            if (pt.X - 3 < 0 || pt.X + 3 >= mask.Cols || pt.Y - 3 < 0 || pt.Y + 3 >= mask.Rows) return false;

            for (int i = -3; i < 3; i++)
            {
                for (int j = -3; j < 3; j++)
                {
                    if (mask.Get<Boolean>(pt.Y + i, pt.X + j)) return false;
                }
            }

            return true;
        }

        public static float GetAngle(Mat img, Mat preview, List<GameObject> list_obj)
        {
            List<float> list = new List<float>();
            foreach (GameObject obj in list_obj)
            {
                if (obj.type == GameObject.ObjectType.timer)
                {
                    RotatedRect rrect = obj.rrect;
                    //Console.WriteLine($"CENTER= {obj.center}, ANGLE= {rrect.Angle}");
                    list.Add(rrect.Angle);
                    Console.Write(rrect.Angle + ",");
                }
            }

            // https://bunkerbook.ru/csharp-lessons/4-sposoba-sortirovki-massiva-v-c/
            list.Sort();
            Console.WriteLine();
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i + 1] - list[i] < 2.0)
                {
                    return (list[i + 1] + list[i]) / 2;
                }
            }

            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i + 1] - list[i] < 5.0)
                {
                    return (list[i + 1] + list[i]) / 2;
                }
            }

            if (list.Count == 1)
            {
                return list[0];
            }

            return 0;
        }

    }
}
