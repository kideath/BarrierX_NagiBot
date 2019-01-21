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
    class Program
    {
        static Random rnd = new Random();

        static Process proc;

        const UInt32 WM_KEYDOWN = 0x0100;
        const UInt32 WM_KEYUP = 0x0101;
        const int VK_E = 0x45;
        const int VK_A = 0x41;
        const int VK_D = 0x44;
        const int VK_Q = 0x51;
        /*
        #define VK_A 0x41
        #define VK_B 0x42
        #define VK_C 0x43
        #define VK_D 0x44
        #define VK_E 0x45
        #define VK_F 0x46
        #define VK_G 0x47
        #define VK_H 0x48
        #define VK_I 0x49
        #define VK_J 0x4A
        #define VK_K 0x4B
        #define VK_L 0x4C
        #define VK_M 0x4D
        #define VK_N 0x4E
        #define VK_O 0x4F
        #define VK_P 0x50
        #define VK_Q 0x51
        #define VK_R 0x52
        #define VK_S 0x53
        #define VK_T 0x54
        #define VK_U 0x55
        #define VK_V 0x56
        #define VK_W 0x57
        #define VK_X 0x58
        #define VK_Y 0x59
        #define VK_Z 0x5A
        // */

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, UInt32 wParam, UInt32 lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);


        //public static string folder1 = @"d:\src\BarrierX_NagiBot\sample\";
        //public static string folder2 = @"c:\Users\valentin.kovrov\OneDrive\OpenCV\sample\";
        //public static string folder3 = @"d:\src\NagiBot_BarrierX\";

        public static string configProcname = @"LSS";
        //public static int configX = 0;
        //public static int configY = 0;
        public static int configOffsetTop = 0;
        public static int configOffsetLeft = 0;
        public static int configWidth = 1280;
        public static int configHeight = 720;
        public static int configPreview = 0;
        public static int configTimeout = 50;

        public static Point screenCenter;
        public static Point screenLeft;
        public static Point screenRight;

        public static int ReadConfig(string configFilename)
        {
            string path = configFilename;
            if (!File.Exists(path))
            {
                path = "..\\..\\" + path; // for Debug Release folders
                if (!File.Exists(path))
                {
                    return -1;
                }
            }

            // https://stackoverflow.com/questions/7980456/reading-from-a-text-file-in-c-sharp
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Do stuff with your line here, it will be called for each 
                    // line of text in your file.
                    string[] arr = line.Split(new char[] { '=' });
                    if (arr.Length == 2)
                    {
                        string key = arr[0].Trim();
                        string value = arr[1].Trim();
                        if (key.Equals("procname")) { configProcname = value; }
                        //else if (key.Equals("x")) { configX = Convert.ToInt32(value); }
                        //else if (key.Equals("y")) { configY = Convert.ToInt32(value); }
                        else if (key.Equals("width")) { configWidth = Convert.ToInt32(value); }
                        else if (key.Equals("height")) { configHeight = Convert.ToInt32(value); }
                        else if (key.Equals("offset_top")) { configOffsetTop = Convert.ToInt32(value); }
                        else if (key.Equals("offset_left")) { configOffsetLeft = Convert.ToInt32(value); }
                        else if (key.Equals("preview")) { configPreview = Convert.ToInt32(value); }
                        else if (key.Equals("timeout")) { configTimeout = Convert.ToInt32(value); }
                    }
                }

                //Console.WriteLine("procname=" + configProcname + ", x=" + configX + ", y=" + configY + ", timeout=" + configTimeout);
            }

            return 1;
        }

        [STAThread]
        static void Main(string[] args)
        {
            // clear screen
            Console.SetCursorPosition(0, 0);
            Console.Write(new String(' ', 80*25));
            Console.SetCursorPosition(0, 0);
            // Console.ReadKey();

            Console.WriteLine("Nagibot for BarrierX by k1death, v2.0");
            string configFilename = "config.txt";
            if (args.Length == 1)
            {
                configFilename = args[0];
            }

            int res= ReadConfig(configFilename);
            if (res != 1) {
                Console.WriteLine("file not exist: " + configFilename);
                Console.WriteLine("usage: NagiBot <config_filename.txt>");
                return;
            }

            

            //int width = 1280; // Ширина ROI
            //int height = 720; // Высота ROI
            //Mat img = new Mat(new OpenCvSharp.Size(screen_width, screen_height), MatType.CV_8UC3);
            //Cv2.NamedWindow("DisplayPicture", WindowMode.AutoSize);
            //Cv2.NamedWindow("win0", WindowMode.AutoSize);
            //Cv2.NamedWindow("win1", WindowMode.AutoSize);
            //Cv2.NamedWindow("win2", WindowMode.AutoSize);
            //Cv2.NamedWindow("win3", WindowMode.AutoSize);
            //Cv2.NamedWindow("win4", WindowMode.AutoSize);
            //Cv2.PutText(img, "HELLO, World", new OpenCvSharp.Point(110, 110), HersheyFonts.HersheyScriptSimplex, 1, new Scalar(220.7, 0.1, 0.1));
            //Cv2.ImShow("DisplayPicture", img);
            //Cv2.WaitKey();
            //img.Release();
            //Cv2.DestroyWindow("DisplayPicture");
            //Mat img2 = Cv2.ImRead("d:\\Action!\\BarrierX_1.jpg", ImreadModes.Color);
            //Console.WriteLine(img2.);


            try
            {
                //proc = Process.GetProcessesByName("BarrierX")[0];
                proc = Process.GetProcessesByName(configProcname)[0];
                //int screenshot_num = 0;
                //int skip = 0;

                float proportionX = configWidth / 1280f;
                float proportionY = configHeight / 720f;
                screenCenter = new Point(configWidth / 2, configHeight / 2);
                screenLeft = new Point(0, configHeight / 2);
                screenRight = new Point(configWidth, configHeight / 2);

                int fps = 0;
                int fps_calc = 0;
                long fps_time = 0;
                long fps_time_dif = 0;
                Mat img_debug = new Mat();
                bool skip = false;

                string[] samples = {"1.png", "mini.png", "3.png", "red.png", "red2.png",
                           "blue.png", "4.png", "crash2.png", "0.png", "boost.png",
                           "5.png", "6.png", "7.png", "8.png", "ct_size.png",
                           "hard.png", "hard2.png", "", "", "",
                };

                while (true)
                {
                    long time1 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    skip = false;

                    Console.SetCursorPosition(0, 2);
                    //Mat img = grab_screen_GetPixelColors();
                    Mat img = grab_screen2();
                    //Mat img = Cv2.ImRead(@"c:\Users\valentin.kovrov\Pictures\BarrierX 16-03-2018 21-17-08.mp4_snapshot_00.04_[2018.06.15_13.19.13].png", ImreadModes.Color);
                    //Mat img = Cv2.ImRead(@"c:\Users\valentin.kovrov\Pictures\1\19.19.png", ImreadModes.Color);
                    //Mat img = Cv2.ImRead(@"d:\src\BarrierX_NagiBot\sample\BarrierX 20-01-2018 22-02-33.mp4_snapshot_05.23_[2018.06.16_15.20.02].png", ImreadModes.Color);
                    //Mat img = Cv2.ImRead(folder2 + "BarrierX boost.png", ImreadModes.Color);
                    //Mat img = Cv2.ImRead(@"d:\src\_workdir\PycharmProjects\PyNagiBot\sample\"+ samples[2], ImreadModes.Color);
                    Console.WriteLine($"check: width = {configWidth} | {img.Cols}, height = {configHeight} | {img.Rows}, type = {img.Type()}");
                    if (img.Type() != MatType.CV_8UC3) {
                        Mat tmp = img.CvtColor(ColorConversionCodes.BGRA2BGR);
                        img.Release();
                        img = tmp;
                    }

                    if (configPreview != 0) {
                        img.CopyTo(img_debug);
                    }

                    
                    List<GameObject> list_obj = FindByWhiteMask(img, img_debug); // определение белых контуров
                    Player player = FindPlayer(list_obj);  // поиск игрока

                    if (player != null)
                    {
                        if (configPreview > 0) {
                            Cv2.Circle(img_debug, player.pt_up, 5, new Scalar(255, 255, 0), -1); // purple
                            Cv2.Circle(img_debug, player.pt_left, 5, new Scalar(255, 0, 153), -1); // light blue
                            Cv2.Circle(img_debug, player.pt_right, 5, new Scalar(100, 0, 255), -1); // pink
                        }
                        Console.WriteLine($"Player found at {player.center}");
                    }
                    else {
                        Console.WriteLine("SKIP: player not found");
                        skip = true;
                    }
                    //rgb(153, 0, 255)


                    Line line_center = null;
                    if (!skip) {
                        // в зависимости от положения коробля определяю точку для определения линии
                        Point diff = new Point(player.pt_up.X, player.pt_up.Y - 10);
                        if (diff.X > screenCenter.X) { diff.X -= 10; }
                        else { diff.X += 10; }

                        if (diff.X >= 0 && diff.X <= configWidth
                            && diff.Y >= 0 && diff.Y <= configHeight)
                        {
                            line_center = FindLine(img, diff, img_debug);
                        }
                        else {
                            Console.WriteLine("SKIP: line center");
                            skip = true;
                        }
                            
                    }

                    // считаю середину слева
                    Line line_left = null;
                    if (!skip) {
                        Point mid_left = new Point(
                            (line_center.pt_up.X + line_center.pt_left.X) / 2,
                            (line_center.pt_up.Y + line_center.pt_left.Y) / 2);

                        // смещаю линию на 10 пикселей вверх и вбок
                        mid_left.X -= 30;
                        mid_left.Y -= 30;
                        
                        if (mid_left.X >= 0 && mid_left.X <= configWidth
                            && mid_left.Y >= 0 && mid_left.Y <= configHeight)
                        {
                            if (configPreview > 0) { Cv2.Circle(img_debug, mid_left, 8, new Scalar(20, 70, 25), -1); }
                            line_left = FindLine(img, mid_left, img_debug);
                        }
                        else {
                            Console.WriteLine("SKIP: line left");
                            skip = true;
                        }

                        
                    }

                    // считаю середину справа
                    Line line_right = null;
                    if (!skip)
                    {
                        Point mid_right = new Point(
                            (line_center.pt_up.X + line_center.pt_right.X) / 2,
                            (line_center.pt_up.Y + line_center.pt_right.Y) / 2);

                        // смещаю линию на 10 пикселей вверх и вбок
                        mid_right.X += 30;
                        mid_right.Y -= 30;

                        if (mid_right.X >= 0 && mid_right.X <= configWidth
                            && mid_right.Y >= 0 && mid_right.Y <= configHeight)
                        {
                            if (configPreview > 0) { Cv2.Circle(img_debug, mid_right, 8, new Scalar(20, 70, 25), -1); }
                            line_right = FindLine(img, mid_right, img_debug);
                        }
                        else {
                            Console.WriteLine("SKIP: line right");
                            skip = true;
                        }
                        
                    }

                    if (!skip && configPreview > 0) {
                        DrawLineColorBox(img_debug, 0, line_left.color);
                        DrawLineColorBox(img_debug, 1, line_center.color);
                        DrawLineColorBox(img_debug, 2, line_right.color);
                    }


                    int c = Cv2.WaitKey(1);

                    //Vec3b c8 = img.Get<Vec3b>(coords[15], coords[14]);  //Point Right 3


                    long time2 = DateTimeOffset.Now.ToUnixTimeMilliseconds(); // DateTime.UtcNow.Millisecond;
                    time2 = time2 - time1;
                    //Console.WriteLine("fps=" + fps);

                    //Clean
                    //Console.SetCursorPosition(0, 0);/*
                    Console.WriteLine($"proportionX= {proportionX}, proportionY= {proportionY}          ");
                    Console.WriteLine($"Frame takes(ms)={time2,3}, fps={fps,3}, time(ms)= {fps_time_dif,4}              ");

                    //Console.WriteLine($"R={R(line_left.color)}");
                    //Bot Logic
                    //*
                    // Item0 - B, Item1- G, Item2- R
                    //if ((R(c1.R > 240 && c1.G > 90) && (c2.R > 240 && c2.G > 90))
                    //if ((R(c1) > 240 && G(c1) > 90) && (R(c2) > 240 && G(c2) > 90))
                    //if (1==2)
                    if (!skip && R(line_center.color) > 240 && G(line_center.color) > 90)
                    {
                        Console.WriteLine("RED      ");
                        //Console.SetCursorPosition(0, 1);

                        //if (c3.R + c5.R + c7.R + c3.G + c5.G + c7.G < c4.R + c6.R + c8.R + c4.G + c6.G + c8.G)
                        //if (c3.Item2 + c5.Item2 + c7.Item2 + c3.Item1 + c5.Item1 + c7.Item1 < c4.Item2 + c6.Item2 + c8.Item2 + c4.Item1 + c6.Item1 + c8.Item1)
                        //if (R(c3) + R(c5) + R(c7) + G(c3) + G(c5) + G(c7) < R(c4) + R(c6) + R(c8) + G(c4) + G(c6) + G(c8))
                        //if (1==2)
                        if (R(line_right.color) + G(line_right.color) > R(line_left.color)+ G(line_left.color))
                        {
                            if (R(line_center.color) + G(line_center.color) > R(line_left.color) + G(line_left.color)) {
                                sendLeft();
                                Console.WriteLine("Controls: Left     ");
                            }
                        }
                        else
                        {
                            if (R(line_center.color) + G(line_center.color) > R(line_right.color) + G(line_right.color))
                            {
                                sendRight();
                                Console.WriteLine("Controls: Right    ");
                            }
                        }
                    }
                    else
                    {
                        //if (isBlack(c2) || isBlack(c1))
                        if (1==2)
                        {
                            sendOk();
                            //Console.SetCursorPosition(0, 2);
                            Console.WriteLine("RESPAWN ?");
                        }
                    }
                    // */

                    if (configPreview > 0) {
                        if (configPreview == 2) img_debug = DoPyrDown(img_debug);
                        Cv2.ImShow("debug", img_debug);
                        //img_debug.Release();
                    }
                    img.Release();

                    Console.WriteLine("                                             \n                                             \n                                             \n                                             ");

                    // Clean
                    //Console.WriteLine("                    \n                    ");
                    Thread.Sleep(configTimeout);
                    // */

                    // fps calc
                    time2 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    fps_calc++;
                    if (time2 - fps_time > 1000) {
                        fps_time_dif = time2 - fps_time;
                        fps_time = time2;
                        fps = fps_calc;
                        fps_calc = 0;
                    }


                }
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine("Game not started !");
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("Source: " + ex.Source);
                Console.WriteLine("StackTrace: " + ex.StackTrace);

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }

            Cv2.DestroyWindow("DisplayPicture");

        }

        public static List<GameObject> FindByWhiteMask(Mat src, Mat preview) {
            // thresh = cv2.inRange(image, (210, 210, 210), (255, 255, 255))
            var thresh = new Mat();
            Cv2.InRange(src, new Scalar(210, 210, 210), new Scalar(255, 255, 255), thresh);
            // cv2.imshow("edges", thresh)
            //_, cnts, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
            //thresh.DrawContours()

            Mat[] contours = null;
            Mat dst = new Mat();
            //Cv2.FindContours(edged, out contours, new Mat(), RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            Cv2.FindContours(thresh, out contours, dst, RetrievalModes.List, ContourApproximationModes.ApproxNone);
            //Cv2.ImShow("dst", dst);
            //Cv2.ImShow("dst", dst);
            //Cv2.ImShow("thresh", thresh);
            dst.Release();
            thresh.Release();

            List<GameObject> list_result = new List<GameObject>();

            if (contours != null)
            {
                Console.WriteLine("contours.size= " + contours.Length);

                for (int i = 0; i < contours.Length; i++)
                {
                    Mat contour = contours[i];
                    double area = Cv2.ContourArea(contour);

                    // if the contour is not sufficiently large, ignore it
                    if (area < 10) continue;

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

                    if (configPreview > 0) Cv2.Polylines(preview, approx, true, new Scalar(0, 255, 0), 4);

                    //Rect rect = Cv2.BoundingRect(contour);
                    //Cv2.Rectangle(preview, rect, new Scalar(0, 255, 0));

                    // https://csharp.hotexamples.com/examples/OpenCvSharp/RotatedRect/-/php-rotatedrect-class-examples.html
                    // https://stackoverflow.com/questions/43342199/draw-rotated-rectangle-in-opencv-c
                    RotatedRect rrect = Cv2.MinAreaRect(contour);
                    Point2f[] points2f = rrect.Points();
                    if (configPreview > 0) PolylinesPoints2f(preview, points2f);

                    double h = ImUtils.distance(points2f[0], points2f[1]);
                    double w = ImUtils.distance(points2f[0], points2f[3]);
                    double ratio = h / w;
                    if (h>w) ratio = w / h;

                    // compute the center of the contour
                    Moments m = Cv2.Moments(contour);
                    Point pt_center = new Point((int)(m.M10 / m.M00), (int)(m.M01 / m.M00));
                    if (configPreview > 0) Cv2.Circle(preview, pt_center, 5, new Scalar(255, 0, 0), -1);


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

                    if (ratio > 0.05)
                    {
                        GameObject obj = new GameObject(contour, area, approx, pt_center, ratio);
                        list_result.Add(obj);
                    }
                        
                    
                }
            }

            //if (configPreview!=0) {
            //    Cv2.DrawContours(preview, contours, -1, new Scalar(0, 255, 0), 2);
            //}

            return list_result;
        }

        public static void PolylinesPoints2f(Mat img, Point2f[] points2f) {
            // https://stackoverflow.com/questions/35969667/how-to-use-the-opencvsharp-3-polylines-fillpoly-library-function
            List<List<Point>> listOfListOfPoint = new List<List<Point>>();
            List<Point> points = new List<Point>();
            listOfListOfPoint.Add(points);
            for (int i = 0; i < points2f.Length; i++)
            {
                Point p = new Point((int)points2f[i].X, (int)points2f[i].Y);
                points.Add(p);
            }

            Cv2.Polylines(img, listOfListOfPoint, true, new Scalar(0, 255, 0), 4);
        }

        public static Player FindPlayer(List<GameObject> list_obj)
        {
            int i_player = -1;
            int max_y = 0;
            GameObject obj;
            for (int i = 0; i < list_obj.Count; i++)
            {
                obj = list_obj[i];

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

            if (i_player == -1) {
                return null;
            }

            obj = list_obj[i_player];
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
            for (int i = 0; i < points.Rows; i++) {
                dists[i] = ImUtils.distance(points.Get<Point>(0, i), ptB);
                if (dists[i] < dists[i_min]) { i_min = i; }
            }

            return points.Get<Point>(0, i_min);
        }

        public static Line FindLine(Mat src, Point seed, Mat preview)
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
            Cv2.FloodFill(src, mask, seed, new Scalar(255, 255, 0), out rect, new Scalar(5, 5, 5), new Scalar(5, 5, 5), flags);
            Mat mask2 = new Mat(mask, new Rect(new Point(1,1), new Size(src.Cols, src.Rows)));
            mask.Release();

            Scalar color_line = Cv2.Mean(src, mask2);
            // Mat mat_color = new Mat(50, 250, MatType.CV_8UC3, color_line);
            // Cv2.ImShow("color", mat_color);

            // (_, cnts2, _) = cv2.findContours(mask_roi, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
            Mat[] contours = null;
            Mat dst = new Mat();
            Cv2.FindContours(mask2, out contours, dst, RetrievalModes.External, ContourApproximationModes.ApproxNone);
            //Cv2.ImShow("dst", dst);
            dst.Release();

            //Console.WriteLine($"line_contour_len={contours.Length}");
            Mat approx = null;
            if (contours.Length > 0) {
                double peri = Cv2.ArcLength(contours[0], true);
                approx = contours[0].ApproxPolyDP(0.04 * peri, true);
                //Console.WriteLine($"points_n={approx.Rows}");
            }

            if (configPreview > 0) {
                Cv2.DrawContours(preview, contours, -1, new Scalar(0, 255, 255), 2);
                Cv2.Polylines(preview, approx, true, new Scalar(255, 0, 255), 7);
            }

            Point line_up = NearestPoint(approx, screenCenter);
            Point line_left = NearestPoint(approx, screenLeft);
            Point line_right = NearestPoint(approx, screenRight);

            if (configPreview > 0) {
                Cv2.Circle(preview, line_up, 5, new Scalar(120, 255, 120), -1);
                Cv2.Circle(preview, line_left, 5, new Scalar(120, 255, 120), -1);
                Cv2.Circle(preview, line_right, 5, new Scalar(120, 255, 120), -1);
            }

            //Cv2.ImShow("mask", mask2);
            //Cv2.ImShow("src", src);
            // Console.WriteLine($"mask_size={mask2.Cols} x {mask2.Rows}   ");
            Console.WriteLine($"line_contour_len={contours.Length}, points_n={approx.Rows}, color={color_line}      ");

            //mask2.Release();

            return new Line(mask2, contours[0], approx, color_line, line_up, line_left, line_right);
        }

        public static void DrawLineColorBox(Mat preview, int index, Scalar color)
        {
            Cv2.Rectangle(preview, new Rect(20 + index * 80, 20, 40, 80), new Scalar(0, 0, 0), -1);
            Cv2.Rectangle(preview, new Rect(20 + index * 80 + 4, 20 + 4, 40 - 8, 40 - 8), color, -1);

            preview.PutText("R=" + (int)color[2], new Point(20 + index * 80 + 4, 60+7), HersheyFonts.HersheySimplex, 0.4, new Scalar(255, 255, 255));
            preview.PutText("G=" + (int)color[1], new Point(20 + index * 80 + 4, 60+20), HersheyFonts.HersheySimplex, 0.4, new Scalar(255, 255, 255));
            preview.PutText("B=" + (int)color[0], new Point(20 + index * 80 + 4, 60+33), HersheyFonts.HersheySimplex, 0.4, new Scalar(255, 255, 255));


            //using (CvFont font = new CvFont(FontFace.HersheySimplex, 0.7, 0.7, 0, 1, LineType.AntiAlias))
            {
                //Cv2.PutText(img, "HELLO, World", new OpenCvSharp.Point(110, 110), HersheyFonts.HersheyScriptSimplex, 1, new Scalar(220.7, 0.1, 0.1));

                //mat.PutText("test", new OpenCvSharp.CPlusPlus.Point(x, y), FontFace.HersheySimplex, 2, new Scalar(b, g, r))
                
                //preview.PutText("R=" + 1, new Point(10, 30), HersheyFonts.HersheyScriptSimplex, 1, new Scalar(255, 255, 255));
                //preview.PutText("R=" + 1, new Point(10, 30), HersheyFonts.HersheyScriptSimplex, 1, new Scalar(255, 255, 255));
                //img.PutText(text_length, new CvPoint(10, img.Height - 10), font, CvColor.White);
            }
        }


        // 
        // https://stackoverflow.com/questions/34139450/getwindowrect-returns-a-size-including-invisible-borders
        static Mat grab_screen2()
        {
            //border should be `7, 0, 7, 7`
            // offset
            var rect = new Rect();
            //int width = 1280;
            //int height = 720;
            //int offset_top = 26;
            //int offset_left = 4;
            GetWindowRect(proc.MainWindowHandle, ref rect);
            if (rect.Right != 0 && rect.Top != 0)
            {
                Console.WriteLine("Window rect: l=" + rect.Left + ", t=" + rect.Top +
                    ", r=" + rect.Right + ", b=" + rect.Bottom +
                    ", w= " + rect.Width + ", h=" + rect.Height + "            ");
            }
            //System.Drawing.Rectangle bounds = new System.Drawing.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(configWidth, configHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //Console.WriteLine("Size= " + bounds.Size + "           ");
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bmp))
            {
                graphics.CopyFromScreen(rect.Left + configOffsetLeft, rect.Top + configOffsetTop, 0, 0,
                    new System.Drawing.Size(configWidth, configHeight), System.Drawing.CopyPixelOperation.SourceCopy);
            }

            //преобразование из bitmap (требуется подключение OpenCvSharp.Extensions)
            Mat res = bmp.ToMat();
            bmp.Dispose();

            return res;
        }

        // Сжатие исходного изображения
        public static Mat DoPyrDown(Mat in1) //  int filter = IPL_GAUSSIAN_5x5
        {
            Mat out1 = new Mat(new Size(in1.Width / 2, in1.Height / 2), MatType.CV_8U, 3);
            // Сжатие исходного изображения
            Cv2.PyrDown(in1, out1);
            in1.Release();
            return out1;
        }

        static int R(Vec3b color)
        {
            return color.Item2;
        }

        static int G(Vec3b color)
        {
            return color.Item1;
        }

        static int B(Vec3b color)
        {
            return color.Item0;
        }



        static int R(Scalar color)
        {
            return (int)color[2];
        }

        static int G(Scalar color)
        {
            return (int)color[1];
        }

        static int B(Scalar color)
        {
            return (int)color[0];
        }



        static void sendLeft()
        {
            PostMessage(proc.MainWindowHandle, WM_KEYUP, VK_A, 0);
            PostMessage(proc.MainWindowHandle, WM_KEYDOWN, VK_A, 0xC14B0001);
        }

        static void sendRight()
        {
            PostMessage(proc.MainWindowHandle, WM_KEYDOWN, VK_D, 0);
            PostMessage(proc.MainWindowHandle, WM_KEYUP, VK_D, 0xC14B0001);
        }

        static void sendOk()
        {
            PostMessage(proc.MainWindowHandle, WM_KEYDOWN, VK_E, 0);
            PostMessage(proc.MainWindowHandle, WM_KEYUP, VK_E, 0xC14B0001);
        }

        static void sendQ()
        {
            PostMessage(proc.MainWindowHandle, WM_KEYDOWN, VK_Q, 0);
            PostMessage(proc.MainWindowHandle, WM_KEYUP, VK_Q, 0xC14B0001);
        }

        static bool isBlack(Vec3b c)
        {
            if (c.Item0 + c.Item1 + c.Item2 < 10)
            {
                return true;
            }

            return false;
        }

    }



}
