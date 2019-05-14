using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace BarrierX_NagiBot
{
    class Samples
    {
        public static void ConnectedComponentsSample(Mat src)
        {
            //Mat src = new Mat("Data/Image/shapes.png", ImreadModes.Color);
            Mat gray = src.CvtColor(ColorConversionCodes.BGR2GRAY);
            Mat binary = gray.Threshold(0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);
            Mat labelView = src.EmptyClone();
            Mat rectView = binary.CvtColor(ColorConversionCodes.GRAY2BGR);

            ConnectedComponents cc = Cv2.ConnectedComponentsEx(binary);
            if (cc.LabelCount <= 1)
                return;

            // draw labels
            cc.RenderBlobs(labelView);

            // draw bonding boxes except background
            foreach (var blob in cc.Blobs.Skip(1))
            {
                rectView.Rectangle(blob.Rect, Scalar.Red);
            }

            // filter maximum blob
            var maxBlob = cc.GetLargestBlob();
            var filtered = new Mat();
            cc.FilterByBlob(src, filtered, maxBlob);

            using (new Window("src", src))
            using (new Window("binary", binary))
            using (new Window("labels", labelView))
            using (new Window("bonding boxes", rectView))
            using (new Window("maximum blob", filtered))
            {
                Cv2.WaitKey();
            }
        }

        public static void DFT(Mat src)
        {
            Mat img = new Mat();
            Cv2.CvtColor(src, img, ColorConversionCodes.BGR2GRAY);  // CtvC ImRead(FilePath.Image.Lenna, ImreadModes.GrayScale);

            // expand input image to optimal size
            Mat padded = new Mat();
            int m = Cv2.GetOptimalDFTSize(img.Rows);
            int n = Cv2.GetOptimalDFTSize(img.Cols); // on the border add zero values
            Cv2.CopyMakeBorder(img, padded, 0, m - img.Rows, 0, n - img.Cols, BorderTypes.Constant, Scalar.All(0));

            // Add to the expanded another plane with zeros
            Mat paddedF32 = new Mat();
            padded.ConvertTo(paddedF32, MatType.CV_32F);
            Mat[] planes = { paddedF32, Mat.Zeros(padded.Size(), MatType.CV_32F) };
            Mat complex = new Mat();
            Cv2.Merge(planes, complex);

            // this way the result may fit in the source matrix
            Mat dft = new Mat();
            Cv2.Dft(complex, dft);

            // compute the magnitude and switch to logarithmic scale
            // => log(1 + sqrt(Re(DFT(I))^2 + Im(DFT(I))^2))
            Mat[] dftPlanes;
            Cv2.Split(dft, out dftPlanes);  // planes[0] = Re(DFT(I), planes[1] = Im(DFT(I))

            // planes[0] = magnitude
            Mat magnitude = new Mat();
            Cv2.Magnitude(dftPlanes[0], dftPlanes[1], magnitude);

            magnitude += Scalar.All(1);  // switch to logarithmic scale
            Cv2.Log(magnitude, magnitude);

            // crop the spectrum, if it has an odd number of rows or columns
            Mat spectrum = magnitude[
                new Rect(0, 0, magnitude.Cols & -2, magnitude.Rows & -2)];

            // rearrange the quadrants of Fourier image  so that the origin is at the image center
            int cx = spectrum.Cols / 2;
            int cy = spectrum.Rows / 2;

            Mat q0 = new Mat(spectrum, new Rect(0, 0, cx, cy));   // Top-Left - Create a ROI per quadrant
            Mat q1 = new Mat(spectrum, new Rect(cx, 0, cx, cy));  // Top-Right
            Mat q2 = new Mat(spectrum, new Rect(0, cy, cx, cy));  // Bottom-Left
            Mat q3 = new Mat(spectrum, new Rect(cx, cy, cx, cy)); // Bottom-Right

            // swap quadrants (Top-Left with Bottom-Right)
            Mat tmp = new Mat();
            q0.CopyTo(tmp);
            q3.CopyTo(q0);
            tmp.CopyTo(q3);

            // swap quadrant (Top-Right with Bottom-Left)
            q1.CopyTo(tmp);
            q2.CopyTo(q1);
            tmp.CopyTo(q2);

            // Transform the matrix with float values into a
            Cv2.Normalize(spectrum, spectrum, 0, 1, NormTypes.MinMax);

            // Show the result
            Cv2.ImShow("Input Image", img);
            Cv2.ImShow("Spectrum Magnitude", spectrum);

            // calculating the idft
            Mat inverseTransform = new Mat();
            Cv2.Dft(dft, inverseTransform, DftFlags.Inverse | DftFlags.RealOutput);
            Cv2.Normalize(inverseTransform, inverseTransform, 0, 1, NormTypes.MinMax);
            Cv2.ImShow("Reconstructed by Inverse DFT", inverseTransform);
            Cv2.WaitKey();

        }

        public static void HoughLinesSample(Mat imgStd, Mat imgGray)
        {
            // (1) Load the image
            //using (Mat imgGray = new Mat(FilePath.Image.Goryokaku, ImreadModes.GrayScale))
            //using (Mat imgStd = new Mat(FilePath.Image.Goryokaku, ImreadModes.Color))
            using (Mat imgProb = imgStd.Clone())
            {
                // Preprocess
                Cv2.Canny(imgGray, imgGray, 50, 200, 3, false);

                // (3) Run Standard Hough Transform 
                LineSegmentPolar[] segStd = Cv2.HoughLines(imgGray, 1, Math.PI / 180, 50, 0, 0);
                int limit = Math.Min(segStd.Length, 10);
                for (int i = 0; i < limit; i++)
                {
                    // Draws result lines
                    float rho = segStd[i].Rho;
                    float theta = segStd[i].Theta;
                    double a = Math.Cos(theta);
                    double b = Math.Sin(theta);
                    double x0 = a * rho;
                    double y0 = b * rho;
                    Point pt1 = new Point { X = (int)Math.Round(x0 + 1000 * (-b)), Y = (int)Math.Round(y0 + 1000 * (a)) };
                    Point pt2 = new Point { X = (int)Math.Round(x0 - 1000 * (-b)), Y = (int)Math.Round(y0 - 1000 * (a)) };
                    imgStd.Line(pt1, pt2, Scalar.Red, 1, LineTypes.AntiAlias, 0);
                }

                // (4) Run Probabilistic Hough Transform
                LineSegmentPoint[] segProb = Cv2.HoughLinesP(imgGray, 1, Math.PI / 180, 50, 50, 10);
                foreach (LineSegmentPoint s in segProb)
                {
                    imgProb.Line(s.P1, s.P2, Scalar.Red, 1, LineTypes.AntiAlias, 0);
                }

                // (5) Show results
                using (new Window("Hough_line_standard", WindowMode.AutoSize, imgStd))
                using (new Window("Hough_line_probabilistic", WindowMode.AutoSize, imgProb))
                {
                    Window.WaitKey(0);
                }
            }

        }

        public static void PhotoMethods(Mat src)
        {
            //Mat src = new Mat(FilePath.Image.Fruits, ImreadModes.Color);

            Mat normconv = new Mat(), recursFildered = new Mat();
            Cv2.EdgePreservingFilter(src, normconv, EdgePreservingMethods.NormconvFilter);
            Cv2.EdgePreservingFilter(src, recursFildered, EdgePreservingMethods.RecursFilter);

            Mat detailEnhance = new Mat();
            Cv2.DetailEnhance(src, detailEnhance);

            Mat pencil1 = new Mat(), pencil2 = new Mat();
            Cv2.PencilSketch(src, pencil1, pencil2);

            Mat stylized = new Mat();
            Cv2.Stylization(src, stylized);

            using (new Window("src", src))
            using (new Window("edgePreservingFilter - NormconvFilter", normconv))
            using (new Window("edgePreservingFilter - RecursFilter", recursFildered))
            using (new Window("detailEnhance", detailEnhance))
            using (new Window("pencilSketch grayscale", pencil1))
            using (new Window("pencilSketch color", pencil2))
            using (new Window("stylized", stylized))
            {
                Cv2.WaitKey();
            }
        }

        // https://github.com/VahidN/OpenCVSharp-Samples/blob/master/OpenCVSharpSample19/Program.cs
        public static void Gradient(Mat src)
        {

            var gray = new Mat();
            var channels = src.Channels();
            if (channels > 1)
            {
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGRA2GRAY);
            }
            else
            {
                src.CopyTo(gray);
            }


            // compute the Scharr gradient magnitude representation of the images
            // in both the x and y direction
            var gradX = new Mat();
            Cv2.Sobel(gray, gradX, MatType.CV_32F, xorder: 1, yorder: 0, ksize: -1);
            //Cv2.Scharr(gray, gradX, MatType.CV_32F, xorder: 1, yorder: 0);
            Cv2.ImShow("gradX", gradX);

            var gradY = new Mat();
            Cv2.Sobel(gray, gradY, MatType.CV_32F, xorder: 0, yorder: 1, ksize: -1);
            //Cv2.Scharr(gray, gradY, MatType.CV_32F, xorder: 0, yorder: 1);
            Cv2.ImShow("gradY", gradY);

            // subtract the y-gradient from the x-gradient
            var gradient = new Mat();
            Cv2.Subtract(gradX, gradY, gradient);
            Cv2.ConvertScaleAbs(gradient, gradient);

            Cv2.ImShow("Gradient", gradient);


            // blur and threshold the image
            var blurred = new Mat();
            Cv2.Blur(gradient, blurred, new Size(9, 9));

            double thresh = 127.0;
            var threshImage = new Mat();
            Cv2.Threshold(blurred, threshImage, thresh, 255, ThresholdTypes.Binary);

            
            bool debug = true;

            if (debug)
            {
                Cv2.ImShow("Thresh", threshImage);
                Cv2.WaitKey(1); // do events
            }


            // construct a closing kernel and apply it to the thresholded image
            var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(21, 7));
            var closed = new Mat();
            Cv2.MorphologyEx(threshImage, closed, MorphTypes.Close, kernel);

            if (debug)
            {
                Cv2.ImShow("Closed", closed);
                Cv2.WaitKey(1); // do events
            }


            // perform a series of erosions and dilations
            Cv2.Erode(closed, closed, null, iterations: 4);
            Cv2.Dilate(closed, closed, null, iterations: 4);

            if (debug)
            {
                Cv2.ImShow("Erode & Dilate", closed);
                Cv2.WaitKey(1); // do events
            }


            //find the contours in the thresholded image, then sort the contours
            //by their area, keeping only the largest one

            Point[][] contours;
            HierarchyIndex[] hierarchyIndexes;
            Cv2.FindContours(
                closed,
                out contours,
                out hierarchyIndexes,
                mode: RetrievalModes.CComp,
                method: ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
            {
                throw new NotSupportedException("Couldn't find any object in the image.");
            }

            var contourIndex = 0;
            var previousArea = 0;
            var biggestContourRect = Cv2.BoundingRect(contours[0]);
            while ((contourIndex >= 0))
            {
                var contour = contours[contourIndex];

                var boundingRect = Cv2.BoundingRect(contour); //Find bounding rect for each contour
                var boundingRectArea = boundingRect.Width * boundingRect.Height;
                if (boundingRectArea > previousArea)
                {
                    biggestContourRect = boundingRect;
                    previousArea = boundingRectArea;
                }

                contourIndex = hierarchyIndexes[contourIndex].Next;
            }

        }
    }
}
