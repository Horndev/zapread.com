using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using zapread.com.Helpers;

namespace zapread.com.Services
{
    /// <summary>
    /// 
    /// </summary>
    public static class CaptchaService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="charCount"></param>
        /// <returns></returns>
        public static string GenerateCaptchaCode(int charCount)
        {
            Random r = new Random();
            string s = "";
            while (s.Length < charCount)
            {
                int a = r.Next(3);
                int c;
                switch (a)
                {
                    case 1:
                        c = r.Next(0, 9);
                        s += c.ToString();
                        break;
                    case 2:
                        c = r.Next(65, 90);
                        s += Convert.ToChar(c).ToString();
                        break;
                    case 3:
                        c = r.Next(97, 122);
                        s += Convert.ToChar(c).ToString();
                        break;
                }
            }
            return s;
        }

        /// <summary>
        /// based on code from https://thecodeprogram.com/build-your-own-captcha-in-asp-net-and-c-
        /// </summary>
        /// <param name="code"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap GenerateCaptchaImage(string code, int width = 350, int height = 80)
        {
            Random rnd = new Random();
            //First declare a bitmap and declare graphic from this bitmap
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bitmap);
            //And create a rectangle to delegete this image graphic 
            Rectangle rect = new Rectangle(0, 0, width, height);
            //And create a brush to make some drawings
            HatchBrush hatchBrush = new HatchBrush(HatchStyle.DottedGrid, Color.Aqua, Color.White);
            g.FillRectangle(hatchBrush, rect);

            // Randomize colors
            var colors = new Color[] { Color.LightGreen, Color.LightSalmon, Color.LightSkyBlue, Color.LightCyan };
            for (int i = 0; i < (int)(rect.Width * rect.Height / 30F); i++)
            {
                using (var dotsBrush = new HatchBrush(HatchStyle.Percent10, foreColor: Color.Black, backColor: colors[rnd.Next(colors.Length)]))
                {
                    int x = rnd.Next(width+5)-5;
                    int y = rnd.Next(height+5)-5;
                    int w = 3+rnd.Next(10);
                    int h = 3+rnd.Next(10);
                    g.FillEllipse(dotsBrush, x, y, w, h);
                }
            }

            //here we make the text configurations
            GraphicsPath graphicPath = new GraphicsPath();
            //add this string to image with the rectangle delegate
            graphicPath.AddString(" " + code, FontFamily.GenericMonospace, (int)FontStyle.Bold, 75, rect, null);
            //And the brush that you will write the text
            hatchBrush = new HatchBrush(HatchStyle.Percent20, Color.White, Color.Teal);
            g.FillPath(hatchBrush, graphicPath);

            var linePen = new Pen(color: Color.Teal, width: 2.0f);
            for (int i = 0; i < 10; i++)
            {
                int x1 = rnd.Next(width);
                int y1 = rnd.Next(height);
                int x2 = rnd.Next(width);
                int y2 = rnd.Next(height);
                g.DrawLine(linePen, x1, y1, x2, y2);
            }

            hatchBrush.Dispose();
            linePen.Dispose();
            g.Dispose();

            return bitmap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetCaptchaB64(int len, out string code)
        {
            code = GenerateCaptchaCode(len);
            var image = GenerateCaptchaImage(code);
            byte[] imgdata = image.ToByteArray(ImageFormat.Png);
            var base64String = Convert.ToBase64String(imgdata);
            return base64String;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetCaptchaB64(string code)
        {
            var image = GenerateCaptchaImage(code);
            byte[] imgdata = image.ToByteArray(ImageFormat.Png);
            var base64String = Convert.ToBase64String(imgdata);
            return base64String;
        }
    }
}