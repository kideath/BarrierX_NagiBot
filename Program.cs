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



        //public static float proportionX = configWidth / 1280f;
        //public static float proportionY = configHeight / 720f;





        static void Sandbox()
        {
//            Mat dest2 = dest.Clone();
//            // 540 x 360
//            Point center1 = new Point(270, 180);
//            Point pt1 = new Point(80, 300);
//            Point pt2 = new Point(480, 300);
//            //Cv2.Circle()
//
//
//            Mat imgGray = new Mat();
//            Cv2.CvtColor(dest, imgGray, ColorConversionCodes.BGR2GRAY, 0);
//            //Cv2.Threshold()
//
//            // color thresh
//            Mat gray_thresh = new Mat();
//            int delta = 5;
//
//            Cv2.InRange(imgGray, new Scalar(174 - delta), new Scalar(174 + delta), gray_thresh);
//            //Cv2.Erode(gray_thresh, gray_thresh, 4);
//            Cv2.Dilate(gray_thresh, gray_thresh, 4);
//            Cv2.ImShow("gray_thresh", gray_thresh);
//
//            //Samples.ConnectedComponentsSample(dest);
//            //Samples.DFT(dest);
//            //Samples.HoughLinesSample(dest, imgGray);
//            Samples.PhotoMethods(dest);
//
//
//            Cv2.ImShow("rotated Gray", imgGray);
//
//            /*
//            KeyPoint[] keypoints = Cv2.FAST(imgGray, 50, true);
//
//            foreach (KeyPoint kp in keypoints)
//            {
//                dest2.Circle((Point)kp.Pt, 3, Scalar.Red, -1, LineTypes.AntiAlias, 0);
//            }
//            // */
//
//            BRISK brisk = BRISK.Create();
//            KeyPoint[] keypoints = brisk.Detect(imgGray);
//
//            if (keypoints != null)
//            {
//                var color = new Scalar(0, 255, 0);
//                foreach (KeyPoint kpt in keypoints)
//                {
//                    float r = kpt.Size / 2;
//                    Cv2.Circle(dest2, (Point)kpt.Pt, (int)r, color);
//                    Cv2.Line(dest2,
//                        (Point)new Point2f(kpt.Pt.X + r, kpt.Pt.Y + r),
//                        (Point)new Point2f(kpt.Pt.X - r, kpt.Pt.Y - r),
//                        color);
//                    Cv2.Line(dest2,
//                        (Point)new Point2f(kpt.Pt.X - r, kpt.Pt.Y + r),
//                        (Point)new Point2f(kpt.Pt.X + r, kpt.Pt.Y - r),
//                        color);
//                }
//            }
//
//
//            Cv2.Circle(dest2, center1, 5, new Scalar(255, 255, 0), -1); // purple
//            Cv2.Line(dest2, center1, pt1, new Scalar(255, 255, 0), 1); // purple)
//            Cv2.Line(dest2, center1, pt2, new Scalar(255, 255, 0), 1); // purple)
//            Cv2.ImShow("rotated ed", dest2);
//
//            dest.Release();
//            imgGray.Release();
        }

        [STAThread]
        static void Main(string[] args)
        {
            // clear screen
            Console.SetCursorPosition(0, 0);
            Console.Write(new String(' ', 80*25));
            Console.SetCursorPosition(0, 0);
            // Console.ReadKey();

            Console.WriteLine("Nagibot for BarrierX by k1death, v2.1.0");
            string configFilename = "config.txt";
            if (args.Length == 1)
            {
                configFilename = args[0];
            }

            Config config = new Config();
            int res= config.ReadConfig(configFilename);
            if (res != 1) {
                Console.WriteLine("file not exist: " + configFilename);
                Console.WriteLine("usage: NagiBot <config_filename.txt>");
                return;
            }

            try
            {
                //proc = Process.GetProcessesByName("BarrierX")[0];
                proc = Process.GetProcessesByName(config.Procname)[0];
                //int screenshot_num = 0;
                //int skip = 0;

                bool b_init = false;
                int fps = 0;
                int fps_calc = 0;
                long fps_time = 0;
                long fps_time_dif = 0;

                bool skip = false;
                int debug_frames_count = 0;
                long debug_frame_time = 0;
                long debug_next_frame_delay = 500; // millisec
                OCR ocr = new OCR();
                ocr.Init(config.OCRFontDir);

                //         public static Scalar[] colors;
                /*        public static Scalar ;
                        public static Scalar color_green;
                        public static Scalar color_blue;
                        public static Scalar color_purple;
                         */



                string[] samples = {"", "", "screen0070_right.png", "screen0021__linecenter.png", "screen0023__linecenter.png",
                            "screen0032__lineleft.png", "screen0015.png", "screen0001__linecenter.png", "screen0025.png", "screen0200.png",
                            //"screen0676.png", "screen0736.png", "screen0787.png", "screen0801.png", "screen0864.png", "screen0883.png", "screen0888.png", "screen0903.png", "", "",
                            "screen0019_right.png", "screen0076.png", "screen0073_left.png", "screen0148_right.png", "screen0265_right.png", "screen0327_right.png", "screen0344.png", "screen0349.png",
                            "screen0044__linecenter.png", "", "", "", "",
                };

                long ocr_last_ostime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                long ocr_digits = 0;
                long ocr_digits_calc = 0;
                ocr_digits = 0;

                //FillLineDetector detector = new FillLineDetector();
                RotatedLineDetector detector = new RotatedLineDetector();


                while (true)
                {
                    long time1 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    skip = false;

                    Console.SetCursorPosition(0, 2);
                    //Mat img = grab_screen_GetPixelColors();
                    Mat img = grab_screen2(config);
                    //Mat img = Cv2.ImRead(@"d:\src\NagiBot_BarrierX\sample\640x480_4\" + samples[15], ImreadModes.Color);
                    Console.WriteLine($"check: width = {config.Width} | {img.Cols}, height = {config.Height} | {img.Rows}, type = {img.Type()}   ");

                    // init
                    if (config.Width != img.Cols || config.Height != img.Rows || !b_init)
                    {
                        config.Width = img.Cols;
                        config.Height = img.Rows;
                        //proportionX = configWidth / 1280f;
                        //proportionY = configHeight / 720f;

                        detector.Init(ocr, config);

                        b_init = true;
                    }

                    if (img.Type() != MatType.CV_8UC3)
                    {
                        Mat tmp = img.CvtColor(ColorConversionCodes.BGRA2BGR);
                        img.Release();
                        img = tmp;
                    }

                    if (config.Preview == 4)
                    {
                        Cv2.ImShow("source", img);
                        //Samples.Gradient(img);
                    }

                    if (config.DebugSaveFrames == 4 && time1 - debug_frame_time > debug_next_frame_delay)
                    {
                        img.ImWrite(String.Format("screen{0:d4}.png", debug_frames_count));
                        //if (configPreview > 0) { img_debug.ImWrite(String.Format("screen{0:d4}_debug.png", debug_frames_count)); }
                        debug_frames_count++;
                        debug_frame_time = time1;
                    }


                    if (ocr_digits_calc >= 1500)
                    {
                        int sizeX = 15;
                        if (ocr_digits_calc >= 13500) sizeX = 190;
                        else if (ocr_digits_calc >= 12000) sizeX = 170;
                        else if (ocr_digits_calc >= 10500) sizeX = 150;
                        else if (ocr_digits_calc >= 9000) sizeX = 130;
                        else if (ocr_digits_calc >= 7500) sizeX = 110;
                        else if (ocr_digits_calc >= 6000) sizeX = 100;
                        else if (ocr_digits_calc >= 4500) sizeX = 70;
                        else if (ocr_digits_calc >= 3000) sizeX = 40;
                        //else if (ocr_digits_calc >= 1500) sizeX = 15;
                        // crop
                        // https://github.com/VahidN/OpenCVSharp-Samples/blob/master/OpenCVSharpSample19/Program.cs
                        //double ratio = config.Height / config.Width;
                        int sizeY = (int)(sizeX * ((double)config.Height / (double)config.Width));
                        //Rect roi = new Rect(size, (int)(size * ratio), 540 - size * 2, 360 - (int)(size * ratio) * 2);
                        Rect roi = new Rect(sizeX, sizeY, config.Width - sizeX * 2, config.Height - sizeY * 2);
                        Console.WriteLine($"ROI={roi.Left}, {roi.Top}, {roi.Width}, {roi.Height}, sizeX={sizeX}, sizeY={sizeY}   ");
                        var cropped = new Mat(img, roi); //Crop the image
                        Mat resized2 = new Mat();
                        //Cv2.CvtColor(barcode, barcode, ColorConversionCodes.BGRA2GRAY);
                        //Cv2.Resize(cropped, resized2, new Size(540, 360), 0, 0, InterpolationFlags.Lanczos4);
                        Cv2.Resize(cropped, resized2, new Size(config.Width, config.Height), 0, 0, InterpolationFlags.Lanczos4);
                        //img.Release();
                        //img = resized;

                        if (config.Preview == 4) Cv2.ImShow("cropped", cropped);
                        if (config.Preview == 4) Cv2.ImShow("resized2", resized2);
                        img.Release();
                        cropped.Release();
                        img = resized2;
                        // */
                    }

                    detector.ProcessFrame(img);
                    if (!detector.isGameOver)
                    {
                        if (ocr.ocrTimerText.Length >= 4 
                            && Math.Abs(ocr.ocrTimerN - ocr_digits_calc) < 1000) // FIX для неправильно распознанного нуля
                        {
                            ocr_digits = ocr.ocrTimerN;
                            ocr_last_ostime = time1;
                            ocr_digits_calc = ocr_digits;
                        }
                        else
                        {
                            ocr_digits_calc = (time1 - ocr_last_ostime) / 10 + ocr_digits;
                        }
                    }
                    else {
                        ocr_last_ostime = time1;
                        ocr_digits_calc = ocr_digits = 0;
                    }

                    Player player = detector.GetPlayer();
                    Line line_center= detector.GetLineCenter();
                    Line line_left = detector.GetLineLeft();
                    Line line_right= detector.GetLineRight();
                    Mat img_debug = null;
                    img_debug = detector.Get_img_debug();

                    if (player == null || line_left == null || line_center == null || line_right == null) { skip = true; }

                    if (config.DebugSaveFrames == 1 && time1 - debug_frame_time > debug_next_frame_delay)
                    {
                        string reason = "";
                        if (player == null) reason += "_player";
                        else if (line_center == null) reason += "_linecenter";
                        else if (line_left == null) reason += "_lineleft";
                        else if (line_right == null) reason += "_lineright";
                        if (reason.Length > 0) {
                            img.ImWrite(String.Format("screen{0:d4}_{1}.png", debug_frames_count, reason));
                            debug_frames_count++;
                            debug_frame_time = time1;
                        }
                    }

                    

                    int c = Cv2.WaitKey(1);

                    //Vec3b c8 = img.Get<Vec3b>(coords[15], coords[14]);  //Point Right 3


                    long time2 = DateTimeOffset.Now.ToUnixTimeMilliseconds(); // DateTime.UtcNow.Millisecond;
                    time2 = time2 - time1;
                    //Console.WriteLine("fps=" + fps);

                    //Clean
                    //Console.SetCursorPosition(0, 0);/*
                    //Console.WriteLine($"proportionX= {proportionX}, proportionY= {proportionY}          ");
                    Console.WriteLine($"Frame takes(ms)={time2,3}, fps={fps,3}, time(ms)= {fps_time_dif,4}              ");
                    //detector.is

                    if (config.Preview > 0 && img_debug != null)
                    {
                        if (line_left != null) DrawLineColorBox(img_debug, 0, line_left.color);
                        if (line_center != null) DrawLineColorBox(img_debug, 1, line_center.color);
                        if (line_right != null) DrawLineColorBox(img_debug, 2, line_right.color);

                        Cv2.Rectangle(img_debug, new Rect(20, 110, 170, 30), new Scalar(0, 0, 0), -1);
                        img_debug.PutText("TIMER=" + ocr_digits_calc + "/" + ocr.ocrTimerText, new Point(20, 130), HersheyFonts.HersheySimplex, 0.6, new Scalar(255, 255, 255));
                    }



                    if (next_move_pause > 0) { next_move_pause--; }


                    //Console.WriteLine($"R={R(line_left.color)}");
                    //Bot Logic
                    //*
                    // Item0 - B, Item1- G, Item2- R
                    //if ((R(c1.R > 240 && c1.G > 90) && (c2.R > 240 && c2.G > 90))
                    //if ((R(c1) > 240 && G(c1) > 90) && (R(c2) > 240 && G(c2) > 90))
                    //if (1==2)
                    if (!skip && !config.DebugNoControls && R(line_center.color) > 220 && G(line_center.color) > 70)
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
                                if ((config.DebugSaveFrames == 2 && time1-debug_frame_time> debug_next_frame_delay) || config.DebugSaveFrames == 4) {
                                    img.ImWrite(String.Format("screen{0:d4}_left.png", debug_frames_count));
                                    if (config.Preview > 0 && img_debug != null) { img_debug.ImWrite(String.Format("screen{0:d4}_debug.png", debug_frames_count)); }
                                    debug_frames_count++;
                                    debug_frame_time = time1;
                                }
                            }
                        }
                        else
                        {
                            if (R(line_center.color) + G(line_center.color) > R(line_right.color) + G(line_right.color))
                            {
                                sendRight();
                                Console.WriteLine("Controls: Right    ");
                                if ((config.DebugSaveFrames == 2 && time1 - debug_frame_time > debug_next_frame_delay) || config.DebugSaveFrames == 4) {
                                    img.ImWrite(String.Format("screen{0:d4}_right.png", debug_frames_count));
                                    if (config.Preview > 0 && img_debug!=null) { img_debug.ImWrite(String.Format("screen{0:d4}_debug.png", debug_frames_count));  }
                                    debug_frames_count++;
                                    debug_frame_time = time1;
                                }
                            }
                        }
                    }
                    else
                    {
                        //if (isBlack(c2) || isBlack(c1))
                        if (config.AutoRestart && detector.isGameOver)
                        {
                            sendOk();
                            //Console.SetCursorPosition(0, 2);
                            Console.WriteLine("RESPAWN ?   ");
                        }
                    }
                    // */



                    if (config.Preview > 0 && img_debug != null) {
                        if (config.Preview == 2) img_debug = DoPyrDown(img_debug);
                        Cv2.ImShow("debug", img_debug);
                        //img_debug.Release();
                    }
                    img.Release();
                    //if (line_center != null) { line_center.contour.Release(); line_center.mask.Release(); }
                    //if (line_left != null) { line_left.contour.Release(); line_left.mask.Release(); }
                    //if (line_right != null) { line_right.contour.Release(); line_right.mask.Release(); }
                    if (line_center != null) { line_center.mask.Release(); }
                    if (line_left != null) { line_left.mask.Release(); }
                    if (line_right != null) { line_right.mask.Release(); }
                    if (config.Preview > 0 && img_debug != null) { img_debug.Release(); }


                    Console.WriteLine("                                             \n                                             \n                                             \n                                             ");

                    // Clean
                    //Console.WriteLine("                    \n                    ");
                    Thread.Sleep(config.Timeout);
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



        public static void DrawLineColorBox(Mat preview, int index, Scalar color)
        {
            Cv2.Rectangle(preview, new Rect(20 + index * 80, 20, 40, 80), Colors.black, -1);
            Cv2.Rectangle(preview, new Rect(20 + index * 80 + 4, 20 + 4, 40 - 8, 40 - 8), color, -1);

            preview.PutText("R=" + (int)color[2], new Point(20 + index * 80 + 4, 60+7), HersheyFonts.HersheySimplex, 0.4, Colors.white);
            preview.PutText("G=" + (int)color[1], new Point(20 + index * 80 + 4, 60+20), HersheyFonts.HersheySimplex, 0.4, Colors.white);
            preview.PutText("B=" + (int)color[0], new Point(20 + index * 80 + 4, 60+33), HersheyFonts.HersheySimplex, 0.4, Colors.white);


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
        static Mat grab_screen2(Config config)
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

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(config.Width, config.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //Console.WriteLine("Size= " + bounds.Size + "           ");
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bmp))
            {
                graphics.CopyFromScreen(rect.Left + config.OffsetLeft, rect.Top + config.OffsetTop, 0, 0,
                    new System.Drawing.Size(config.Width, config.Height), System.Drawing.CopyPixelOperation.SourceCopy);
            }

            //преобразование из bitmap (требуется подключение OpenCvSharp.Extensions)
            Mat res = new Mat(new Size(config.Width, config.Height), MatType.CV_8UC3);
            bmp.ToMat(res);
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

        static int next_move_pause = 0;

        static void sendLeft()
        {
            if (next_move_pause != 0) return;
            PostMessage(proc.MainWindowHandle, WM_KEYUP, VK_A, 0);
            PostMessage(proc.MainWindowHandle, WM_KEYDOWN, VK_A, 0xC14B0001);
            next_move_pause = 2;
        }

        static void sendRight()
        {
            if (next_move_pause != 0) return;
            PostMessage(proc.MainWindowHandle, WM_KEYDOWN, VK_D, 0);
            PostMessage(proc.MainWindowHandle, WM_KEYUP, VK_D, 0xC14B0001);
            next_move_pause = 2;
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
