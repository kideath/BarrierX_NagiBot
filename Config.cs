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
    class Config
    {
        public string Procname = @"LSS";
        //public static int configX = 0;
        //public static int configY = 0;
        public int OffsetTop = 0;
        public int OffsetLeft = 0;
        public int Width = 1280;
        public int Height = 720;
        public int Preview = 0;
        public int Timeout = 50;
        public int NoControl = 50;
        public int DebugSaveFrames = 0;
        public bool DebugNoControls = false;
        public string OCRFontDir = null;
        public bool AutoRestart = false;

        public int ReadConfig(string configFilename)
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
                        if (key.Equals("procname")) { Procname = value; }
                        //else if (key.Equals("x")) { configX = Convert.ToInt32(value); }
                        //else if (key.Equals("y")) { configY = Convert.ToInt32(value); }
                        else if (key.Equals("width")) { Width = Convert.ToInt32(value); }
                        else if (key.Equals("height")) { Height = Convert.ToInt32(value); }
                        else if (key.Equals("offset_top")) { OffsetTop = Convert.ToInt32(value); }
                        else if (key.Equals("offset_left")) { OffsetLeft = Convert.ToInt32(value); }
                        else if (key.Equals("preview")) { Preview = Convert.ToInt32(value); }
                        else if (key.Equals("timeout")) { Timeout = Convert.ToInt32(value); }
                        else if (key.Equals("debug_save_frames")) { DebugSaveFrames = Convert.ToInt32(value); }
                        else if (key.Equals("debug_no_controls")) { DebugNoControls = Convert.ToInt32(value) == 1 ? true : false; }
                        else if (key.Equals("ocr_font_dir")) { OCRFontDir = value; }
                        else if (key.Equals("auto_restart")) { AutoRestart = Convert.ToInt32(value) == 1 ? true : false; }
                    }
                }

                //Console.WriteLine("procname=" + configProcname + ", x=" + configX + ", y=" + configY + ", timeout=" + configTimeout);
            }

            return 1;
        }


    }
}
