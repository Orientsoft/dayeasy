﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DayEasy.Utility.Helper
{
    /// <summary>
    /// 图片辅助方法
    /// </summary>
    public static class ImageHelper
    {
        private const int Threshold = 125;

        #region 压缩图片

        /// <summary>
        /// 压缩图片，返回map
        /// </summary>
        /// <param name="source"></param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns></returns>
        public static Bitmap ResizeImg(Bitmap source, int width, int height)
        {
            if (source == null)
                return null;
            int ow = source.Width, oh = source.Height; //原始大小
            var rect = CalcResizeRect(ow, oh, width, height);
            Bitmap bm;
            if (rect.X == 0 && rect.Y == 0)
            {
                bm = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppRgb);
            }
            else
            {
                bm = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            }
            using (var g = Graphics.FromImage(bm))
            {
                //呈现质量
                g.CompositingQuality = CompositingQuality.HighQuality;
                //像素偏移方式
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //平滑处理
                g.SmoothingMode = SmoothingMode.HighQuality;
                //插补模式,双三次插值法
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (rect.X == 0 && rect.Y == 0)
                {
                    g.DrawImage(source, rect, new Rectangle(0, 0, ow, oh),
                        GraphicsUnit.Pixel);
                }
                else
                {
                    g.DrawImage(source, new Rectangle(0, 0, width, height), rect,
                        GraphicsUnit.Pixel);
                }

                #region gif

                //                //帧数
                //                var frameCount = source.GetFrameCount(FrameDimension.Time);
                //                if (frameCount > 1)
                //                {
                //                    var frameBmp = new Bitmap(width, height, PixelFormat.Format32bppRgb);
                //                    var frameGraphics = Graphics.FromImage(frameBmp);
                //                    //呈现质量
                //                    frameGraphics.CompositingQuality = CompositingQuality.HighQuality;
                //                    //像素偏移方式
                //                    frameGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //                    //平滑处理
                //                    frameGraphics.SmoothingMode = SmoothingMode.HighQuality;
                //                    //插补模式,双三次插值法
                //                    frameGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //
                //                    var rg = new Rectangle(0, 0, width, height);
                //                    var encoder = Encoder.SaveFlag;
                //                    foreach (var guid in source.FrameDimensionsList)
                //                    {
                //                        var fd = new FrameDimension(guid);
                //                        var count = source.GetFrameCount(fd);
                //                        //gif动图
                //                        EncoderParameters eps;
                //                        for (int i = 0; i < count; i++)
                //                        {
                //                            //选中某一帧
                //                            source.SelectActiveFrame(FrameDimension.Time, i);
                //                            if (i == 0)
                //                            {
                //                                g.DrawImage(source, rg);
                //                                BindProperty(source, bm);
                //                                eps = new EncoderParameters(1);
                //                                //如果是GIF这里设置为FrameDimensionTime  
                //                                //如果为TIFF则设置为FrameDimensionPage
                //                                eps.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.MultiFrame);
                //                                bm.Save("2.gif", GetEncoder(ImageFormat.Gif), eps);
                //                                //bm.Save();
                //                            }
                //                            else
                //                            {
                //                                frameGraphics.DrawImage(source, rg);
                //                                BindProperty(source, frameBmp);
                //                                eps = new EncoderParameters(1);
                //                                //如果是GIF这里设置为FrameDimensionTime  
                //                                //如果为TIFF则设置为FrameDimensionPage
                //                                eps.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.FrameDimensionTime);
                //                                //向新图添加一帧  
                //                                bm.SaveAdd(frameBmp, eps);
                //                            }
                //                        }
                //                        eps = new EncoderParameters(1);
                //                        eps.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.Flush);
                //                        bm.SaveAdd(eps);
                //                    }
                //                } 

                #endregion
            }
            return bm;
        }

        private static void BindProperty(Image source, Image target)
        {
            //复制属性
            foreach (var prop in source.PropertyItems)
            {
                target.SetPropertyItem(prop);
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        private static Rectangle CalcResizeRect(int ow, int oh, int width, int height)
        {
            var rect = new Rectangle(0, 0, width, height);
            if (ow <= 0 || oh <= 0)
            {
                return rect;
            }

            if (width <= 0 && height <= 0)
            {
                //宽度和高度都没有指明时用原始值
                rect.Width = ow;
                rect.Height = oh;
            }
            else if (width <= 0)
            {
                var scaleY = (double)height / oh;
                rect.Width = (int)Math.Ceiling(scaleY * ow);
            }
            else if (height <= 0)
            {
                var scaleX = (double)width / ow;
                var scaleY = (double)width / oh;
                if (scaleX > scaleY)
                {
                    rect.Height = width;
                    rect.Width = (int)Math.Ceiling(scaleY * ow);
                }
                else
                {
                    rect.Height = (int)Math.Ceiling(scaleX * oh);
                }
            }
            else
            {
                //                var scaleX = (double)width / ow;
                //                var scaleY = (double)height / oh;
                //                if (scaleX > scaleY)
                //                {
                //                    rect.Width = ow;
                //                    rect.Height = (height * ow) / width;
                //                    rect.Y = (oh - (height * ow) / width) / 2;
                //                }
                //                else if (scaleX < scaleY)
                //                {
                //                    rect.Width = (width * oh) / height;
                //                    rect.Height = oh;
                //                    rect.X = (ow - rect.Width) / 2;
                //                }
            }
            return rect;
        }

        #endregion

        #region 剪切图片

        /// <summary> 剪切图片 </summary>
        /// <param name="source">原图片</param>
        /// <param name="x">起始x坐标</param>
        /// <param name="y">起始y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns></returns>
        public static Bitmap MakeImage(Bitmap source, int x, int y, int width, int height)
        {
            return MakeImage(source, new RectangleF(x, y, width, height));
        }

        /// <summary> 裁剪图片 </summary>
        /// <param name="source"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Bitmap MakeImage(Bitmap source, RectangleF rect)
        {
            if (source == null)
                return null;
            var width = (int)Math.Ceiling(Math.Min(rect.Width, source.Width - rect.X));
            var height = (int)Math.Ceiling(Math.Min(rect.Height, source.Height - rect.Y));

            var newBmp = new Bitmap(width, height); // 生成新的画布
            using (var g = Graphics.FromImage(newBmp))
            {
                // 将画布读取到图像中
                var origRect = new RectangleF(rect.X, rect.Y, width, height);
                var destRect = new RectangleF(0, 0, width, height); // 新画布图像矩形框
                //呈现质量
                g.CompositingQuality = CompositingQuality.HighQuality;
                //像素偏移方式
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //平滑处理
                g.SmoothingMode = SmoothingMode.HighQuality;
                //插补模式,双三次插值法
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // 将原始图像矩形框中的内容生成到新画布中去
                g.DrawImage(source, destRect, origRect, GraphicsUnit.Pixel);
                return newBmp;
            }
        }

        /// <summary>
        /// 剪切图片
        /// </summary>
        /// <param name="stream">原图片流</param>
        /// <param name="x">起始x坐标</param>
        /// <param name="y">起始y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>剪切后的图片 Base64 流</returns>
        public static string MakeImage(Stream stream, int x, int y, int width, int height)
        {
            if (!(x >= 0 && y > 0 && width > 0 && height > 0))
                return string.Empty;
            try
            {
                var source = new Bitmap(stream);
                var bmp = MakeImage(source, x, y, width, height);
                if (bmp == null)
                {
                    source.Dispose();
                    return string.Empty;
                }
                var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Jpeg);
                source.Dispose();
                bmp.Dispose();
                return ms.ConvertToBase64();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary> 裁剪图片 </summary>
        /// <param name="stream"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static string MakeImage(Stream stream, RectangleF rect)
        {
            try
            {
                var source = new Bitmap(stream);
                var bmp = MakeImage(source, rect);
                if (bmp == null)
                {
                    source.Dispose();
                    return string.Empty;
                }
                var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Jpeg);
                source.Dispose();
                bmp.Dispose();
                return ms.ConvertToBase64();
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region 图像二值化

        /// <summary>
        /// 图像二值化
        /// </summary>
        /// <param name="source">原图片</param>
        /// <param name="threshold">阈值</param>
        public static Bitmap BinarizeImage(Bitmap source, byte threshold = Threshold)
        {
            int width = source.Width;
            int height = source.Height;
            BitmapData data = source.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            unsafe
            {
                var p = (byte*)data.Scan0;
                int offset = data.Stride - width * 4;
                byte R, G, B, gray;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        R = p[2];
                        G = p[1];
                        B = p[0];
                        gray = (byte)((R * 19595 + G * 38469 + B * 7472) >> 16);
                        if (gray >= threshold)
                        {
                            p[0] = p[1] = p[2] = 255;
                        }
                        else
                        {
                            p[0] = p[1] = p[2] = 0;
                        }
                        p += 4;
                    }
                    p += offset;
                }
                source.UnlockBits(data);
                return source;
            }
        }

        #endregion

        #region 图片旋转

        /// <summary>
        /// 任意旋转角度
        /// </summary>
        /// <param name="source">原图片</param>
        /// <param name="angle">旋转角度</param>
        /// <param name="color">背景颜色</param>
        /// <returns></returns>
        public static Bitmap RotateImage(Bitmap source, float angle, Color color)
        {
            PixelFormat pixelFormat = source.PixelFormat;
            PixelFormat pixelFormatOld = pixelFormat;
            if (source.Palette.Entries.Count() > 0)
            {
                pixelFormat = PixelFormat.Format24bppRgb;
            }
            var tmpBitmap = new Bitmap(source.Width, source.Height, pixelFormat);
            tmpBitmap.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using (var g = Graphics.FromImage(tmpBitmap))
            {

                g.FillRectangle(new SolidBrush(color), 0, 0, source.Width, source.Height);
                g.RotateTransform(angle);
                g.DrawImage(source, 0, 0);
            }
            switch (pixelFormatOld)
            {
                case PixelFormat.Format8bppIndexed:
                    tmpBitmap = CopyTo8Bpp(tmpBitmap);
                    break;
                case PixelFormat.Format1bppIndexed:
                    tmpBitmap = CopyTo1Bpp(tmpBitmap);
                    break;
            }
            return tmpBitmap;
        }

        /// <summary>
        /// 任意旋转角度
        /// </summary>
        /// <param name="source">原图片</param>
        /// <param name="angle">旋转角度</param>
        /// <returns></returns>
        public static Bitmap RotateImage(Bitmap source, float angle)
        {
            return RotateImage(source, angle, Color.White);
        }

        #endregion

        #region 图片纠偏

        /// <summary>
        /// 图像纠偏
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap Deskew(Bitmap source)
        {
            var max = Math.Max(source.Width, source.Height);
            Bitmap tempBmp;
            if (max > 1000)
            {
                tempBmp = MakeImage(source, source.Width / 4, source.Height / 4, source.Width / 2, source.Height / 2);
                tempBmp = BinarizeImage(tempBmp);
            }
            else
            {
                tempBmp = BinarizeImage(source);
            }
            var des = new Deskew(tempBmp);
            var angle = des.GetSkewAngle();
            return RotateImage(tempBmp, (float)-angle);
        }

        #endregion

        #region 黑阶色判断

        /// <summary>
        /// 是否是黑阶色
        /// </summary>
        /// <param name="bmap"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsBlack(Bitmap bmap, int x, int y)
        {
            Color c = bmap.GetPixel(x, y);
            return IsBlack(c);
        }

        /// <summary>
        /// 是否是黑阶色
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsBlack(Color color)
        {
            return (color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114) < 140;
        }

        #endregion

        #region 图像灰度化

        /// <summary>
        /// 图像灰度化
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap ToGray(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    //利用公式计算灰度值
                    var gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    Color newColor = Color.FromArgb(gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        /// <summary>
        /// 图像灰度反转
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap GrayReverse(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    Color newColor = Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        #endregion

        #region 图片相似度

        /// <summary>
        /// 比较图片相似度
        /// </summary>
        /// <param name="sourceBitmap">原图片</param>
        /// <param name="distBitmap">比较的图片</param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static float CheckSimilarity(Bitmap sourceBitmap, Bitmap distBitmap, int size = 0)
        {
            //判断图像是否相同大小
            if (sourceBitmap.Width != distBitmap.Width || sourceBitmap.Height != distBitmap.Height || size > 0)
            {
                size = (size == 0 ? 256 : size);
                sourceBitmap = new Bitmap(sourceBitmap, size, size);
                distBitmap = new Bitmap(distBitmap, size, size);
            }
            return CompareHisogram(GetHisogram(sourceBitmap), GetHisogram(distBitmap));
        }

        #endregion

        #region 图片直方图

        /// <summary>
        /// 计算图片直方图
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static int[] GetHisogram(Bitmap img)
        {
            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            var histogram = new int[256];
            unsafe
            {
                var ptr = (byte*)data.Scan0;
                int remain = data.Stride - data.Width * 3;
                for (int i = 0; i < histogram.Length; i++)
                    histogram[i] = 0;
                for (int i = 0; i < data.Height; i++)
                {
                    for (int j = 0; j < data.Width; j++)
                    {
                        int mean = ptr[0] + ptr[1] + ptr[2];
                        mean /= 3;
                        histogram[mean]++;
                        ptr += 3;
                    }
                    ptr += remain;
                }
            }
            img.UnlockBits(data);
            return histogram;
        }

        #endregion

        #region 图像包含判断

        /// <summary>
        /// 判断图形里是否存在另外一个图形 并返回所在位置
        /// </summary>
        /// <param name="pSourceBitmap">原始图形</param>
        /// <param name="pPartBitmap">小图形</param>
        /// <param name="pFloat">溶差</param>
        /// <returns>坐标</returns>
        public static Point ImageContains(Bitmap pSourceBitmap, Bitmap pPartBitmap, int pFloat)
        {
            int sourceWidth = pSourceBitmap.Width;
            int sourceHeight = pSourceBitmap.Height;

            int partWidth = pPartBitmap.Width;
            int partHeight = pPartBitmap.Height;

            var sourceBitmap = new Bitmap(sourceWidth, sourceHeight);
            Graphics graphics = Graphics.FromImage(sourceBitmap);
            graphics.DrawImage(pSourceBitmap, new Rectangle(0, 0, sourceWidth, sourceHeight));
            graphics.Dispose();
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceWidth, sourceHeight),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var sourceByte = new byte[sourceData.Stride * sourceHeight];
            //复制出p_SourceBitmap的相素信息 
            Marshal.Copy(sourceData.Scan0, sourceByte, 0, sourceByte.Length);

            for (int i = 2; i < sourceHeight; i++)
            {
                //如果 剩余的高 比需要比较的高 还要小 就直接返回
                if (sourceHeight - i < partHeight) return new Point(-1, -1);
                //临时存放坐标 需要保证找到的是在一个X点上
                int pointX = -1;
                //是否都比配的上
                bool sacnOver = true;
                //循环目标进行比较
                for (int z = 0; z < partHeight - 1; z++)
                {
                    int trueX = GetImageContains(sourceByte, i * sourceData.Stride, sourceWidth, partWidth, pFloat);
                    //如果没找到
                    if (trueX == -1)
                    {
                        //设置坐标为没找到
                        pointX = -1;
                        //设置不进行返回
                        sacnOver = false;
                        break;
                    }
                    if (z == 0) pointX = trueX;
                    //如果找到了 也的保证坐标和上一行的坐标一样 否则也返回
                    if (pointX != trueX)
                    {
                        //设置坐标为没找到
                        pointX = -1;
                        //设置不进行返回
                        sacnOver = false;
                        break;
                    }
                }

                if (sacnOver) return new Point(pointX, i);
            }
            return new Point(-1, -1);
        }

        #endregion

        #region 私有方法

        private static Bitmap CopyTo1Bpp(Bitmap b)
        {
            int w = b.Width, h = b.Height;
            var r = new Rectangle(0, 0, w, h);
            if (b.PixelFormat != PixelFormat.Format32bppPArgb)
            {
                var temp = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
                temp.SetResolution(b.HorizontalResolution, b.VerticalResolution);
                Graphics g = Graphics.FromImage(temp);
                g.DrawImage(b, r, 0, 0, w, h, GraphicsUnit.Pixel);
                g.Dispose();
                b = temp;
            }
            BitmapData bdat = b.LockBits(r, ImageLockMode.ReadOnly, b.PixelFormat);
            var b0 = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
            b0.SetResolution(b.HorizontalResolution, b.VerticalResolution);
            BitmapData b0Dat = b0.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int index = y * bdat.Stride + (x * 4);
                    if (
                        Color.FromArgb(Marshal.ReadByte(bdat.Scan0, index + 2), Marshal.ReadByte(bdat.Scan0, index + 1),
                            Marshal.ReadByte(bdat.Scan0, index)).GetBrightness() > 0.5f)
                    {
                        int index0 = y * b0Dat.Stride + (x >> 3);
                        byte p = Marshal.ReadByte(b0Dat.Scan0, index0);
                        var mask = (byte)(0x80 >> (x & 0x7));
                        Marshal.WriteByte(b0Dat.Scan0, index0, (byte)(p | mask));
                    }
                }
            }
            b0.UnlockBits(b0Dat);
            b.UnlockBits(bdat);
            return b0;
        }

        private static Bitmap CopyTo8Bpp(Bitmap bmp)
        {
            if (bmp == null) return null;

            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

            int width = bmpData.Width;
            int height = bmpData.Height;
            int stride = bmpData.Stride;
            int offset = stride - width * 3;
            IntPtr ptr = bmpData.Scan0;
            int scanBytes = stride * height;

            int posScan = 0, posDst = 0;
            var rgbValues = new byte[scanBytes];
            Marshal.Copy(ptr, rgbValues, 0, scanBytes);
            var grayValues = new byte[width * height];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double temp = rgbValues[posScan++] * 0.11 +
                                  rgbValues[posScan++] * 0.59 +
                                  rgbValues[posScan++] * 0.3;
                    grayValues[posDst++] = (byte)temp;
                }
                posScan += offset;
            }

            Marshal.Copy(rgbValues, 0, ptr, scanBytes);
            bmp.UnlockBits(bmpData);

            var bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            bitmap.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            int offset0 = bitmapData.Stride - bitmapData.Width;
            int scanBytes0 = bitmapData.Stride * bitmapData.Height;
            var rawValues = new byte[scanBytes0];

            int posSrc = 0;
            posScan = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    rawValues[posScan++] = grayValues[posSrc++];
                }
                posScan += offset0;
            }

            Marshal.Copy(rawValues, 0, bitmapData.Scan0, scanBytes0);
            bitmap.UnlockBits(bitmapData);

            ColorPalette palette;
            using (var bmp0 = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                palette = bmp0.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;

            return bitmap;
        }

        /// <summary>
        /// 比较数组相似度
        /// </summary>
        /// <param name="firstNum"></param>
        /// <param name="scondNum"></param>
        /// <returns></returns>
        private static float CompareHisogram(int[] firstNum, int[] scondNum)
        {
            if (firstNum.Length != scondNum.Length)
                return 0;
            float result = 0;
            int j = firstNum.Length;
            for (int i = 0; i < j; i++)
            {
                result += 1 - GetAbs(firstNum[i], scondNum[i]);
            }
            return result / j;
        }

        /// <summary>
        /// 计算相减后的绝对值
        /// </summary>
        /// <param name="firstNum"></param>
        /// <param name="secondNum"></param>
        /// <returns></returns>
        private static float GetAbs(int firstNum, int secondNum)
        {
            int abs = Math.Abs(firstNum - secondNum);
            int result = Math.Max(firstNum, secondNum);
            if (result == 0)
                result = 1;
            return abs / (float)result;
        }

        /// <summary>
        /// 判断图形里是否存在另外一个图形 所在行的索引
        /// </summary>
        /// <param name="pSource">原始图形数据</param>
        /// <param name="pSourceIndex">开始位置</param>
        /// <param name="pSourceWidth">原始图形宽</param>
        /// <param name="pPartWidth">小图宽</param>
        /// <param name="pFloat">溶差</param>
        /// <returns>所在行的索引 如果找不到返回-1</returns>
        private static int GetImageContains(byte[] pSource, int pSourceIndex, int pSourceWidth, int pPartWidth,
            int pFloat)
        {
            int sourceIndex = pSourceIndex;
            for (int i = 0; i < pSourceWidth; i++)
            {
                if (pSourceWidth - i < pPartWidth) return -1;
                Color currentlyColor = Color.FromArgb(pSource[sourceIndex + 3], pSource[sourceIndex + 2],
                    pSource[sourceIndex + 1], pSource[sourceIndex]);
                sourceIndex += 4;
                bool scanColor = ScanColor(currentlyColor, pFloat);

                if (scanColor)
                {
                    int sourceRva = sourceIndex;
                    bool equals = true;
                    for (int z = 0; z != pPartWidth - 1; z++)
                    {
                        currentlyColor = Color.FromArgb(pSource[sourceRva + 3], pSource[sourceRva + 2],
                            pSource[sourceRva + 1], pSource[sourceRva]);
                        if (!ScanColor(currentlyColor, pFloat))
                        {
                            equals = false;
                            break;
                        }
                        sourceRva += 4;
                    }
                    if (equals) return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 检查色彩(可以根据这个更改比较方式)
        /// </summary>
        /// <param name="pCurrentlyColor">当前色彩</param>
        /// <param name="pFloat">溶差</param>
        /// <returns></returns>
        private static bool ScanColor(Color pCurrentlyColor, int pFloat)
        {
            return (pCurrentlyColor.R + pCurrentlyColor.G + pCurrentlyColor.B) / 3 < pFloat;
        }

        #endregion

        #region 图片打马赛克

        public static string MosaicImage(Stream stream, int x, int y, int width, int height)
        {
            if (stream == null) return string.Empty;
            Bitmap image = null;
            try
            {
                image = new Bitmap(stream);
                using (var g = Graphics.FromImage(image))
                {
                    if (width == 0) width = image.Width;
                    if (height == 0) height = image.Height;
                    width = Math.Min(width, image.Width - x);
                    height = Math.Min(height, image.Height - y);
                    if (width < 1 || height < 1) return string.Empty;

                    //背景
                    var color1 = Color.FromArgb(55, 162, 202);
                    var color2 = Color.FromArgb(255, 245, 214);
                    var rect = new Rectangle(x, y, width, height);
                    var brush = new LinearGradientBrush(
                        rect, color1, color2, LinearGradientMode.Vertical);
                    g.FillRectangle(brush, rect);
                    //g.FillRectangle(new SolidBrush(Color.Gray), x, y, width, height);

                    var ms = new MemoryStream();
                    image.Save(ms, ImageFormat.Jpeg);
                    return ms.ConvertToBase64();
                }
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                if (image != null) image.Dispose();
            }
        }

        #endregion

        #region 图片流转换为Base64

        public static string ConvertToBase64(this Stream stream, bool isDispose = true)
        {
            if (stream == null) return string.Empty;
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                if (!isDispose) return @"data:image/png;base64," + Convert.ToBase64String(bytes);
                stream.Close();
                stream.Dispose();
                return @"data:image/png;base64," + Convert.ToBase64String(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion
    }
}
