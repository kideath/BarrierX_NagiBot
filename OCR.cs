using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace BarrierX_NagiBot
{
    class OCR
    {
        private string[] charFileNames = { "0" , "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private int[] charValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        private Mat[] charMat = null;
        public string ocrTimerText = "";
        public long ocrTimerN = 0;

        public void Init(string ocrFontDir)
        {
            if (ocrFontDir != null && ocrFontDir.Length > 0) {
                if (!ocrFontDir.EndsWith("/") && !ocrFontDir.EndsWith("\\")) ocrFontDir += "\\";

                charMat = new Mat[charFileNames.Length];
                for (int i = 0; i < charFileNames.Length; i++)
                {
                    charMat[i] = Cv2.ImRead(ocrFontDir + charFileNames[i] + ".png", ImreadModes.GrayScale);
                    //Cv2.ImShow("CHAR", charMat[i]);
                    //Cv2.WaitKey(250);
                }
            }
        }

        public void ProcessOCR(Mat thresh, GameObject obj)
        {
            if (charMat == null) return;// "";

            // Apply a Perspective Transform & Threshold
            // apply the four point transform to obtain a top-down
            // view of the original image
            //Mat warped = ImUtils.FourPointTransformFast(thresh, obj.orderedPts, obj.rrect_w, obj.rrect_h);
            Mat warped = ImUtils.FourPointTransformFast(thresh, obj.orderedPts, 18, 36);

            // https://docs.opencv.org/3.0-beta/doc/py_tutorials/py_imgproc/py_template_matching/py_template_matching.html
            // All the 6 methods for comparison in a list
            // methods = ['cv2.TM_CCOEFF', 'cv2.TM_CCOEFF_NORMED', 'cv2.TM_CCORR',
            // 'cv2.TM_CCORR_NORMED', 'cv2.TM_SQDIFF', 'cv2.TM_SQDIFF_NORMED']
            //Cv2.ImShow("warped", warped);
            //Cv2.WaitKey(250);

            //Console.Write("RES: ");
            double score = -1;
            double score_prev = -1;
            int imax = -1;
            for (int i = 0; i < charMat.Length; i++)
            {
                Mat result = new Mat();
                Cv2.MatchTemplate(warped, charMat[i], result, TemplateMatchModes.CCoeff);
                double minVal, maxVal;
                Cv2.MinMaxLoc(result, out minVal, out maxVal);
                //Console.Write($", {(int)maxVal}");
                if (maxVal > score) {
                    score_prev = score;
                    score = maxVal;
                    imax = i;
                }
            }
            //Console.WriteLine("       ");
            if (imax!=-1)
            //Console.WriteLine($"MAX_SCORE = {score}, MAX_SCORE_PREV = {score_prev}, INDEX = {charValues[imax]}   ");

            //if (score > 3000000)
            {
                //Cv2.ImShow("ocr111", warped);
                //Cv2.WaitKey(500);
            }

            warped.Release();
            if (score > 3000000)
            {
                ocrTimerN = ocrTimerN * 10 + imax;
                ocrTimerText += charValues[imax];
                //return charValues[imax];
            }
            //else return "";

        }

        public void FindTimer(Mat thresh, List<GameObject> list_obj)
        {
            ocrTimerText = "";
            List<int> list_index = new List<int>(); // список индексов потенциальных цифр
            GameObject obj;
            for (int i = 0; i < list_obj.Count; i++)
            {
                obj = list_obj[i];
                if (obj.type != GameObject.ObjectType.unknown) continue;
                //Console.WriteLine($"CENTER={obj.center}, AREA={obj.area}");

                if (obj.type == GameObject.ObjectType.unknown
                    //&& obj.center.Y < Program.screenCenter.Y // должен быть в верхней половине экрана
                    && obj.center.Y < 240 // должен быть в верхней половине экрана
                    && obj.points_approx.Rows > 2    // у него должно быть более 2 точек
                    && obj.area > 4                // должен быть относительно крупный
                    )
                {
                    // скопировано из ImUtils
                    Point2f[] rect = ImUtils.OrderPoints(obj.rrect.Points());
                    Point2f tl = rect[0];
                    Point2f tr = rect[1];
                    Point2f br = rect[2];
                    Point2f bl = rect[3];

                    double widthA = ImUtils.Distance(br, bl);
                    double widthB = ImUtils.Distance(tr, tl);
                    int width = (int)(widthA > widthB ? widthA : widthB);

                    double heightA = ImUtils.Distance(tr, br);
                    double heightB = ImUtils.Distance(tl, bl);
                    int height = (int)(heightA > heightB ? heightA : heightB);

                    obj.orderedPts = rect;
                    obj.rrect_w = width;
                    obj.rrect_h = height;

                    //Console.WriteLine($"CENTER={obj.center}, W={width}, H={height}");
                    //if (height > 20 && height < 40 && width > 5 && width < 20)
                    {
                        list_index.Add(i);
                    }
                }
            }

            // FAST алгоритм проверяю только по размеру
            if (list_index.Count > 0 && list_index.Count < 36) {
                // сортировка
                int xmin = list_obj[list_index[0]].center.X;
                int imin = 0;
                for (int i1 = 0; i1 < list_index.Count -1; i1++)
                {
                    imin = i1;
                    xmin = list_obj[list_index[i1]].center.X;
                    for (int i2 = i1 + 1; i2 < list_index.Count; i2++)
                    {
                        if (xmin > list_obj[list_index[i2]].center.X)
                        {
                            imin = i2;
                            xmin = list_obj[list_index[i2]].center.X;
                        }
                    }
                    if (imin != i1)
                    {
                        xmin = list_index[i1];
                        list_index[i1] = list_index[imin];
                        list_index[imin] = xmin;
                    }
                }

                // OCR 
                for (int i = 0; i < list_index.Count; i++) {
                    //string txt= ProcessOCR(thresh, list_obj[list_index[i]]);
                    //if (txt.Length > 0) {
                    //    ocrTimerText += txt;
                    //    list_obj[list_index[i]].type = GameObject.ObjectType.timer;
                    //}
                    
                }
            }
        }

        // необходимо, чтобы в списке были только цифры и они уже были отсортированы
        public void FindTimer2(Mat thresh, List<GameObject> list_digits)
        {
            ocrTimerText = "";
            ocrTimerN = 0;

            // FAST алгоритм проверяю только по размеру
            if (list_digits.Count > 0 && list_digits.Count < 8)
            {
                // OCR 
                for (int i = 0; i < list_digits.Count; i++)
                {
                    GameObject obj = list_digits[i];
                    Point2f[] rect = ImUtils.OrderPoints(obj.rrect.Points());
                    obj.orderedPts = rect;
                    ProcessOCR(thresh, obj);
                }
            }



        }






    }
}
