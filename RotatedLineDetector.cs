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
    class RotatedLineDetector
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

        public bool isGameOver= false;
        //private double angle;
        private List<GameObject> list_digits;

        public void ProcessFrame(Mat source)
        {
            Mat img = source;
            //Mat img_debug= null;
            //bool skip = false;

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

            isGameOver = false;
            //angle = 0;

            Mat deleteme = null;
            Mat mask_white = new Mat();
            Cv2.InRange(img, Colors.rgb220, Colors.rgb255, mask_white);
            // perform a series of erosions and dilations
            
            //Cv2.Dilate(mask_white, mask_white, null);
            //Cv2.Erode(mask_white, mask_white, new Mat(), iterations: 1);
            //if (config.Preview == 4)
            //Cv2.ImShow("threshSSS", mask_white);

            //Mat preview = new Mat();
            //if (config.Preview != 0)
            //{
            //    img.CopyTo(preview);
            //    Cv2.Circle(preview, screenCenter, 3, colors[4], -1);
            //}
            if (config.Preview == 4) Cv2.ImShow("threshR", mask_white);

            this.list_digits = new List<GameObject>();
            this.player = new Player();

            PreProcessByWhiteMask(img, img_debug, mask_white);

            if (isGameOver) {
                Console.WriteLine("SKIP: Game Over detected!    ");
                return;
            }

            double angle = GetAngle2(list_digits);
            double angle3 = -GetAngle3(list_digits);

            if (angle <= -45) angle += 90;
            else if (angle >= 45) angle -= 90;
            if (angle3 <= -45) angle3 += 90;
            else if (angle3 >= 45) angle3 -= 90;

            if (Math.Abs(angle - angle3) < 10) {
                angle = (angle + angle3)/ 2;
            }
            
            Console.WriteLine($"angle={angle}, angle3={angle3}   ");

            if (config.Preview ==4 && img_debug != null) { Cv2.ImShow("pre rotate", img_debug); }

            if (angle != 0)
            {
                // https://docs.opencv.org/3.0-beta/doc/py_tutorials/py_imgproc/py_geometric_transformations/py_geometric_transformations.html
                Mat M = Cv2.GetRotationMatrix2D(screenCenter, angle, 1);
                Mat dest = new Mat();
                Cv2.WarpAffine(img, dest, M, new Size(config.Width, config.Height));

                Cv2.Line(dest, screenLeft, screenRight, Colors.blue, 2);
                //Cv2.Circle(dest, screenCenter, 5, new Scalar(255, 255, 0), -1);
                if (config.Preview == 4) Cv2.ImShow("rotated", dest);

                //img.Release();
                deleteme = dest;
                img = dest;

                Mat mask_white2 = new Mat();
                Cv2.WarpAffine(mask_white, mask_white2, M, new Size(config.Width, config.Height));
                mask_white.Release();
                mask_white = mask_white2;
                if (config.Preview == 4) Cv2.ImShow("rotated_mask", mask_white);

                if (config.Preview != 0)
                {
                    img_debug.Release();

                    img_debug = new Mat();
                    img.CopyTo(img_debug);
                    //Cv2.Circle(img_debug, screenCenter, 5, new Scalar(255, 255, 0), -1);
                    Cv2.Circle(img_debug, screenCenter, 5, Colors.cyan, -1);
                }

                if (player == null) player = new Player();
                list_digits = new List<GameObject>();
                PreProcessByWhiteMask(img, img_debug, mask_white);
            } // */

            if (player != null)
            {
                if (config.Preview > 0)
                {
                    //Cv2.Circle(img_debug, player.pt_up, 5, new Scalar(255, 255, 0), -1); // purple
                    //Cv2.Circle(img_debug, player.pt_left, 5, new Scalar(255, 0, 153), -1); // light blue
                    //Cv2.Circle(img_debug, player.pt_right, 5, new Scalar(100, 0, 255), -1); // pink
                }
                Console.WriteLine($"Player found at {player.center}   ");
                //if (config.Preview > 0) ImUtils.PolylinesPoints2f(preview, rrpoints);
                if (config.Preview > 0) Cv2.Polylines(img_debug, player.points_approx, true, Colors.cyan, 2);
                //if (config.Preview > 0) ImUtils.PolylinesPoints2f(img_debug, player.points_approx, colors[5]);
                //if (this.config.Preview > 0) Cv2.Circle(preview, pt_center, 2, new Scalar(255, 0, 0), -1);
                if (config.Preview > 0) Cv2.Circle(img_debug, player.center, 2, Colors.blue, -1);
            }
            else
            {
                Console.WriteLine("SKIP: player not found   ");
                return;
            }





            //if (is)

            // необходимо проанализировать кадр, определить является ли конец игры (меню) или определить цифры


            /*
            List<GameObject> list_obj = FindByWhiteMask(img, img_debug); // определение белых контуров
            Player player = FindPlayer(list_obj);  // поиск игрока
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
                Cv2.WarpAffine(img, dest, M, new Size(configWidth, configHeight));

                Cv2.Line(dest, screenLeft, screenRight, colors[2], 3);
                Cv2.Circle(dest, screenCenter, 5, new Scalar(255, 255, 0), -1);
                Cv2.ImShow("rotated", dest);


            }
            Console.WriteLine($"ANGLE= {angle}");
            // */

            // сортировка
            // https://docs.microsoft.com/ru-ru/dotnet/api/system.collections.generic.list-1.sort?view=netcore-3.0
            // This shows calling the Sort(Comparison(T) overload using 
            // an anonymous method for the Comparison delegate. 
            // This method treats null as the lesser of two values.
            list_digits.Sort(delegate (GameObject el1, GameObject el2)
            {
                if (el1 == null && el2 == null) return 0;
                else if (el1 == null) return -1;
                else if (el2 == null) return 1;
                else return el1.center.X.CompareTo(el2.center.X);
            });

            ocr.FindTimer2(mask_white, list_digits);

            // 
            int timerCenterX = GetTimerDimention(list_digits, img_debug);
            if (timerCenterX!=-1 && player!=null)
            {
                Point ptLineCenter = new Point(timerCenterX, 240);
                if (config.Preview > 0) Cv2.Circle(img_debug, ptLineCenter, 5, Colors.color1, -1);

                GetLines(img, img_debug, mask_white, player, ptLineCenter);
            }
            // */
            mask_white.Release();
            if (deleteme != null) deleteme.Release();
        }











        //public Mat mask_white = null;
        // за первый проход наполняю с сортировкой по X
        // за второй ищу цифры по высоте
        // https://answers.opencv.org/question/74777/how-to-use-approxpolydp-to-close-contours/
        public void PreProcessByWhiteMask(Mat src, Mat preview, Mat mask_white)
        {
            // количество гиганских объектов
            int big_count = 0;
            int max_y = 0;
            //Point pt_tmp = new Point(0, 0);

            Mat[] contours = null;
            Mat dst = new Mat();
            //Cv2.FindContours(edged, out contours, new Mat(), RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(mask_white, out contours, dst, RetrievalModes.External, ContourApproximationModes.ApproxNone);

            if (contours != null)
            {
                Console.WriteLine($"contours.size={contours.Length}   ");

                for (int i = 0; i < contours.Length; i++)
                {
                    Mat contour = contours[i];
                    double area = Cv2.ContourArea(contour);

                    // if the contour is not sufficiently large, ignore it
                    if (area < 1) continue;

                    double peri = Cv2.ArcLength(contour, true);
                    Mat points_approx = contour.ApproxPolyDP(0.04 * peri, true);

                    //                    Vec2i pt_top= points_approx.Get<Vec2i>(0, 0), pt_bottom = points_approx.Get<Vec2i>(0, 0);
                    //                    for (int y = 0; y < points_approx.Height; y++)
                    //                    {
                    //                        for (int x = 0; x < points_approx.Width; x++)
                    //                        {
                    //                            Vec2i pt = points_approx.Get<Vec2i>(y, x);
                    //                            //if (i==3) { 
                    //                            //Console.WriteLine("" + y + "=" + pt.Item0 + ", "+ pt.Item1+ "                             ");
                    //                            if (this.config.Preview > 0) Cv2.Circle(preview, new Point(pt.Item0, pt.Item1), 3, new Scalar(0, 255, 0), -1);
                    //                            //}
                    //
                    //                            if (pt.Item1 < pt_top.Item1) { pt_top = pt; }
                    //                            if (pt.Item1 > pt_bottom.Item1) { pt_bottom = pt; }
                    //                        }
                    //                    }

                    // Point[] points_approx = new Point[approx.Rows];
                    // for (int j = 0; j < approx.Rows; j++) {
                    //     points_approx[j] = approx.Get<Point>(0, j);
                    // }

                    // if (config.Preview > 0) Cv2.Polylines(preview, points_approx, true, new Scalar(0, 0, 255), 4);

                    //Rect rect = Cv2.BoundingRect(contour);
                    //Cv2.Rectangle(preview, rect, new Scalar(0, 255, 0));

                    // https://csharp.hotexamples.com/examples/OpenCvSharp/RotatedRect/-/php-rotatedrect-class-examples.html
                    // https://stackoverflow.com/questions/43342199/draw-rotated-rectangle-in-opencv-c
                    RotatedRect rrect = Cv2.MinAreaRect(contour);

                    // скопировано из ImUtils
                    Point2f[] rrpoints = ImUtils.OrderPoints(rrect.Points());
                    Point2f tl = rrpoints[0];
                    Point2f tr = rrpoints[1];
                    Point2f br = rrpoints[2];
                    Point2f bl = rrpoints[3];
                    //if (config.Preview > 0) ImUtils.PolylinesPoints2f(preview, rrpoints);

                    double widthA = ImUtils.Distance(br, bl);
                    double widthB = ImUtils.Distance(tr, tl);
                    double width = (widthA > widthB ? widthA : widthB);

                    double heightA = ImUtils.Distance(tr, br);
                    double heightB = ImUtils.Distance(tl, bl);
                    double height = (heightA > heightB ? heightA : heightB);

                    double ratio = height / width;
                    if (height > width) ratio = width / height;

                    int top = (int)tl.Y;
                    if (top > (int)tr.Y) { top = (int)tr.Y;  }
                    int bottom = (int)bl.Y;
                    if (bottom < (int)br.Y) { bottom = (int)br.Y; }

                    // compute the center of the contour
                    Moments m = Cv2.Moments(contour);
                    Point pt_center = new Point((int)(m.M10 / m.M00), (int)(m.M01 / m.M00));
                    //if (this.config.Preview > 0) Cv2.Circle(preview, pt_center, 2, new Scalar(255, 0, 0), -1);

                    // ищу большие буквы - конец игры
                    if (top > 15 && top < 125 
                        && bottom > 300 && bottom < 390
                        && height > 180
                        && area > 1000)
                    {
                        // рисую голубым большую цифру 	rgb(0, 255, 255) new Scalar(51, 0, 153)
                        //if (this.config.Preview > 0) Cv2.Circle(preview, pt_center, 2, new Scalar(255, 255, 0), -1);
                        //if (config.Preview > 0) ImUtils.PolylinesPoints2f(preview, rrpoints, colors[1]);
                        if (config.Preview > 0) Cv2.DrawContours(preview, contours, i, Colors.red, 2);
                        //if (config.Preview > 0) Cv2.Polylines(preview, points_approx, true, colors[0], 2);
                        big_count++;
                    }

                    // проверяю цифры
                    if (
                        //&& obj.center.Y < Program.screenCenter.Y // должен быть в верхней половине экрана
                        bottom < 240 // должен быть в верхней половине экрана
                        && points_approx.Rows >= 2    // у него должно быть более 2 точек
                        && area > 4 && area < 1000    // должен быть относительно крупный
                        && ratio > 0.05
                        && height > 20 && height < 41 && width > 5 && width < 126
                        )
                    {
                        //if (config.Preview > 0) Cv2.Polylines(preview, points_approx, true, colors[1], 2);
                        if (config.Preview > 0) ImUtils.PolylinesPoints2f(preview, rrpoints, Colors.green2);
                        if (config.Preview > 0) Cv2.DrawContours(preview, contours, i, Colors.green2, 2);

                        GameObject obj = new GameObject(contour, area, points_approx, pt_center, ratio, rrect);
                        list_digits.Add(obj);
                    }

                    // ищу игрока
                    if (player!=null  // игнорирую проход до поворота
                        && top > screenCenter.Y
                        && pt_center.Y > max_y             // объект должен быть снизу других
                        && pt_center.Y > screenCenter.Y // должен быть в нижней половине экрана
                        && points_approx.Rows > 2    // у него должно быть более 2 точек
                        && area > 100                // должен быть относительно крупный
                        && ratio > 0.19
                        )
                    {
                        max_y = pt_center.Y;

                        player.contour = contour;
                        player.area = area;
                        player.points_approx = points_approx; // точки контура
                        player.center = pt_center;
                        //player.ratio; // width / height to skip lines

                        //Console.WriteLine("PLAYER" + i);
                    }


                    // https://metanit.com/sharp/tutorial/7.5.php
                    //Console.WriteLine($"object{i}: area={(int)area,5}, pts_count={points_approx.Rows,3}, ratio= {ratio:f4}, top={top}, bottom={bottom}, h={height}, w= {width}, center={pt_center}              ");

                    // https://www.pyimagesearch.com/2016/03/21/ordering-coordinates-clockwise-with-python-and-opencv/
                    // # loop over the original points and draw them
                    //for (int i1 = 0; i1 < points2f.Length; i1++) {
                    //    Cv2.Circle(preview, new Point((int)points2f[i1].X, (int)points2f[i1].Y), 2, colors[i1], -1);
                    //}

                    //double h = ImUtils.Distance(points2f[0], points2f[1]);
                    //double w = ImUtils.Distance(points2f[0], points2f[3]);
                }

                //mask_white.Release();
            }

            //Cv2.WaitKey(5000);
            //if (configPreview!=0) {
            //    Cv2.DrawContours(preview, contours, -1, new Scalar(0, 255, 0), 2);
            //}

            dst.Release();

            if (big_count >= 2) isGameOver = true;
            if (max_y == 0) player = null;
            Console.WriteLine($"big_count={big_count}, isGameOver={isGameOver}   ");
        }

        // получение угла по rrect углу
        public static float GetAngle2(List<GameObject> list_digits)
        {
            List<float> list = new List<float>();
            Console.Write("ANGLES: ");
            foreach (GameObject obj in list_digits)
            {
                RotatedRect rrect = obj.rrect;
                //Console.WriteLine($"CENTER= {obj.center}, ANGLE= {rrect.Angle}");
                list.Add(rrect.Angle);
                Console.Write(rrect.Angle + ",");
            }
            Console.WriteLine("   ");

            // https://bunkerbook.ru/csharp-lessons/4-sposoba-sortirovki-massiva-v-c/
            list.Sort();
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

        // получение угла по верху и низу
        public double GetAngle3(List<GameObject> list_digits)
        {
            if (list_digits.Count == 0) return 0;

            // получаю крайние точки
            GameObject obj1 = list_digits[0];
            Point pt_top1= new Point();
            Point pt_bottom1 = new Point();
            for (int i = 0; i < obj1.points_approx.Rows; i++) {
                Point pt= obj1.points_approx.Get<Point>(0, i);
                if (i == 0 || pt.Y < pt_top1.Y) { pt_top1 = pt; }
                if (i == 0 || pt.Y > pt_bottom1.Y) { pt_bottom1 = pt; }
            }

            GameObject obj2 = list_digits[list_digits.Count-1];
            Point pt_top2 = new Point();
            Point pt_bottom2 = new Point();
            for (int i = 0; i < obj2.points_approx.Rows; i++)
            {
                Point pt = obj2.points_approx.Get<Point>(0, i);
                if (i == 0 || pt.Y < pt_top2.Y) { pt_top2 = pt; }
                if (i == 0 || pt.Y > pt_bottom2.Y) { pt_bottom2 = pt; }
            }

            // далее считаю углы
            // https://ru.wikihow.com/%D0%BD%D0%B0%D0%B9%D1%82%D0%B8-%D1%83%D0%B3%D0%BE%D0%BB-%D0%BD%D0%B0%D0%BA%D0%BB%D0%BE%D0%BD%D0%B0-%D0%BF%D1%80%D1%8F%D0%BC%D0%BE%D0%B9-%D0%BF%D0%BE-%D0%B4%D0%B2%D1%83%D0%BC-%D1%82%D0%BE%D1%87%D0%BA%D0%B0%D0%BC
            double d1 = 0;
            if (pt_top1.Y != pt_top2.Y) {
                d1= Math.Atan((pt_top1.X - pt_top2.X) / (pt_top1.Y - pt_top2.Y));
                d1 = d1 * 180 / Math.PI;
            }

            double d2 = 0;
            if (pt_bottom1.Y != pt_bottom2.Y) {
                d2= Math.Atan((pt_bottom1.X - pt_bottom2.X) / (pt_bottom1.Y - pt_bottom2.Y));
                d2 = d2 * 180 / Math.PI;
            }
            Console.WriteLine($"d1= {d1}, d2= {d2}");

            if (config.Preview > 0)
            {
                Cv2.Circle(img_debug, pt_top1, 4, new Scalar(0, 255, 255), -1);
                Cv2.Circle(img_debug, pt_top2, 4, new Scalar(0, 0, 255), -1);
                Cv2.Circle(img_debug, pt_bottom1, 4, new Scalar(0, 0, 255), -1);
                Cv2.Circle(img_debug, pt_bottom2, 4, new Scalar(0, 0, 255), -1);
            }

            if (d1 == 0 && d2 != 0) return d2;
            if (d1 != 0 && d2 == 0) return d1;
            return (d1 + d2) / 2;

            // для 2-х верхних точек
            //                    Vec2i pt_top= points_approx.Get<Vec2i>(0, 0), pt_bottom = points_approx.Get<Vec2i>(0, 0);
            //                    for (int y = 0; y < points_approx.Height; y++)
            //                    {
            //                        for (int x = 0; x < points_approx.Width; x++)
            //                        {
            //                            Vec2i pt = points_approx.Get<Vec2i>(y, x);
            //                            //if (i==3) { 
            //                            //Console.WriteLine("" + y + "=" + pt.Item0 + ", "+ pt.Item1+ "                             ");
            //                            if (this.config.Preview > 0) Cv2.Circle(preview, new Point(pt.Item0, pt.Item1), 3, new Scalar(0, 255, 0), -1);
            //                            //}
            //
            //                            if (pt.Item1 < pt_top.Item1) { pt_top = pt; }
            //                            if (pt.Item1 > pt_bottom.Item1) { pt_bottom = pt; }
            //                        }
            //                    }
            // Point[] points_approx = new Point[approx.Rows];
            // for (int j = 0; j < approx.Rows; j++) {
            //     points_approx[j] = approx.Get<Point>(0, j);
            // }

            //if (list_digits.Count == 1)
           // {
           //     return list_digits[0];
           // }

            return 0;
        }

        private int GetTimerDimention(List<GameObject> list_digits, Mat img_debug) {
            // для определения середины необходимо, чтобы цифр было достаточно (4)
            // тогда серединой будет наибольшая разница по расстоянию?

            // возможно, считать середину через моменты и центр по rrect ?
            //if (list_digits.Count < 4 || list_digits.Count > 5) return -1;
            if (list_digits.Count == 0) return -1;

            Point pt_left = screenRight;
            Point pt_right = screenLeft;

            GameObject el = list_digits[0];
            Mat approx = el.points_approx;
            for (int j = 0; j < approx.Rows; j++)
            {
                Point p = approx.Get<Point>(0, j);
                if (p.X < pt_left.X) pt_left = p;
            }

            el = list_digits[list_digits.Count-1];
            approx = el.points_approx;
            for (int j = 0; j < approx.Rows; j++)
            {
                Point p = approx.Get<Point>(0, j);
                if (p.X > pt_right.X) pt_right = p;
            }

            if (config.Preview > 0) {
                Cv2.Circle(img_debug, pt_left, 4, Colors.red, -1);
                Cv2.Circle(img_debug, pt_right, 4, Colors.red, -1);
            }

            return (pt_left.X + pt_right.X) / 2;
            //return new Scalar(pt_left.X, center, pt_right.X);

        }

        private void GetLines(Mat img, Mat img_debug, Mat mask_white, Player player, Point ptLineCenter)
        {
            Console.WriteLine($"PLAYER POS= {player.center}, TIMER POS={ptLineCenter}   ");
            int offset = screenCenter.X - ptLineCenter.X;

            /*
            if (Math.Abs(player.center.X - ptLineCenter.X) < 10)
            {

                Point ptc1 = new Point(80 - offset, 420);
                Point ptc2 = new Point(560 - offset, 420);
                line_center = GetLine(img, img_debug, mask_white, player, ptLineCenter, ptc1, ptc2, "mask_center");
                //Cv2.Line(img_debug, ptLineCenter, ptc1, colors[1]);
                //Cv2.Line(img_debug, ptLineCenter, ptc2, colors[1]);

                Point ptl1 = new Point(70 - offset, 315);
                Point ptl2 = new Point(100 - offset, 350);
                line_left = GetLine(img, img_debug, mask_white, player, ptLineCenter, ptl1, ptl2, "mask_left");
                //Cv2.Line(img_debug, ptLineCenter, ptl1, colors[2]);
                //Cv2.Line(img_debug, ptLineCenter, ptl2, colors[2]);

                Point ptr1 = new Point(570 - offset, 315);
                Point ptr2 = new Point(540 - offset, 350);
                line_right = GetLine(img, img_debug, mask_white, player, ptLineCenter, ptr1, ptr2, "mask_right");
                //Cv2.Line(img_debug, ptLineCenter, ptr1, colors[2]);
                //Cv2.Line(img_debug, ptLineCenter, ptr2, colors[2]);
            } // */
            // PLAYER POS= (x:311 y:332), TIMER POS=(x:400 y:240)
            // PLAYER POS= (x:320 y:320), TIMER POS=(x:259 y:240)
            //else 
            if (Math.Abs(player.center.X - screenCenter.X) <= 20 || Math.Abs(ptLineCenter.X - screenCenter.X) <= 20) {
                Point ptc1 = new Point(80, 420);
                Point ptc2 = new Point(560, 420);
                line_center = GetLine(img, img_debug, mask_white, player, ptLineCenter, ptc1, ptc2, "mask_center");

                Point ptl1 = new Point(70, 315);
                Point ptl2 = new Point(100, 350);
                line_left = GetLine(img, img_debug, mask_white, player, ptLineCenter, ptl1, ptl2, "mask_left");

                Point ptr1 = new Point(570, 315);
                Point ptr2 = new Point(540, 350);
                line_right = GetLine(img, img_debug, mask_white, player, ptLineCenter, ptr1, ptr2, "mask_right");

            }

        }

        private Line GetLine(Mat img, Mat img_debug, Mat mask_white, Player player, Point pt0, Point pt1, Point pt2, string line_name)
        {
            //Point ptc1 = new Point(70, 390);
            //Point ptc2 = new Point(520, 390);
            if (config.Preview > 0) {
                Cv2.Line(img_debug, pt0, pt1, Colors.purple, 2);
                Cv2.Line(img_debug, pt0, pt2, Colors.purple, 2);
            }

            // https://stackoverflow.com/questions/51875114/triangle-filling-in-opencv
            // triangle_cnt = np.array( [pt1, pt2, pt3] )
            // cv2.drawContours(image, [triangle_cnt], 0, (0, 255, 0), -1)
            // cv2.imshow("image", image)
            // cv2.waitKey()
            // https://stackoverflow.com/questions/44063407/opencvsharp-2-floodfill-is-broken-and-corrupts-output-mask
            Mat mask = new Mat(img.Rows, img.Cols, MatType.CV_8UC1, new Scalar(0, 0, 0, 0));
            // floodflags = 4
            // #floodflags |= cv2.FLOODFILL_MASK_ONLY
            // floodflags |= (255 << 8)
            //FloodFillFlags flags = FloodFillFlags.Link4 | FloodFillFlags.MaskOnly | FloodFillFlags.FixedRange;
            // num,im,mask,rect = cv2.floodFill(orig2, mask, seed, (255,255,0), (5,)*3, (5,)*3, floodflags)
            //Rect rect = new Rect();
            //int flags = (int)FloodFillFlags.Link4 | (int)FloodFillFlags.MaskOnly | (255 << 8);
            //Cv2.FloodFill(src, mask, seed, new Scalar(255, 255, 0), out rect, new Scalar(2, 2, 2), new Scalar(2, 2, 2), flags);
            //Mat mask2 = new Mat(mask, new Rect(new Point(1, 1), new Size(src.Cols, src.Rows)));
            //mask.Release();
            // https://stackoverflow.com/questions/35969667/how-to-use-the-opencvsharp-3-polylines-fillpoly-library-function
            List<List<Point>> listOfListOfPoint = new List<List<Point>>();
            List<Point> points = new List<Point>();
            listOfListOfPoint.Add(points);
            points.Add(pt0);
            points.Add(pt1);
            points.Add(pt2);
            //Cv2.C Polylines(mask, listOfListOfPoint, true, new Scalar(255), 1);
            Cv2.DrawContours(mask, listOfListOfPoint, 0, new Scalar(255), -1);
            Mat mask2 = new Mat();
            Cv2.BitwiseNot(mask_white, mask2, mask);
            //Cv2.BitwiseAnd(mask2, mask, mask);
            Scalar color_line = Cv2.Mean(img, mask2);
            if (config.Preview ==4) Cv2.ImShow(line_name, mask2);
            Line line = new Line();
            line.mask = mask2;
            line.color = color_line;

            return line;
        }





    }
}
