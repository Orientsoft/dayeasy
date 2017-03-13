using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;

namespace DayEasy.Utility
{
    /// <summary> 图片处理辅助类 </summary>
    public class ImageCls : IDisposable
    {
        #region 私有属性

        private static Bitmap _oldImg;
        private static string _ext = ".jpeg";
        private const int Threshold = 125;
        private readonly long _size;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="oldPath">图片路径</param>
        public ImageCls(string oldPath)
        {
            _ext = Path.GetExtension(oldPath);
            _oldImg = (Bitmap)Image.FromFile(oldPath);
            _size = new FileInfo(oldPath).Length;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="oldStream">图片流</param>
        public ImageCls(Stream oldStream)
        {
            _oldImg = (Bitmap)Image.FromStream(oldStream);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bitmap"></param>
        public ImageCls(Bitmap bitmap)
        {
            _oldImg = bitmap;
        }

        #endregion

        /// <summary> 原图 </summary>
        public Bitmap SourceBmp
        {
            get { return _oldImg; }
        }

        #region 压缩图片

        /// <summary>
        /// 压缩图片，返回map
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns></returns>
        public Bitmap ResizeImg(int width, int height)
        {
            if (_size <= 50 * 1024)
                return _oldImg;
            return ImageHelper.ResizeImg(_oldImg, width, height);
        }

        /// <summary>
        /// 压缩图片(简化)，返回map
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public Bitmap ResizeImg(int width)
        {
            return ResizeImg(width, -2);
        }

        /// <summary>
        /// 压缩图片，返回结果
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="newPath">新路径</param>
        /// <param name="qt">压缩质量(0-100)</param>
        /// <returns></returns>
        public bool ResizeImg(int width, int height, string newPath, int qt)
        {
            try
            {
                var bm = ResizeImg(width, height);
                var encoder = GetEncoderInfo(_ext);
                if (encoder == null)
                    return false;
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, qt);
                bm.Save(newPath, encoder, encoderParameters);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 压缩图片(简化)，返回结果
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="newPath">新路径</param>
        /// <param name="qt">压缩质量(0-100)</param>
        /// <returns></returns>
        public bool ResizeImg(int width, string newPath, int qt)
        {
            return ResizeImg(width, -2, newPath, qt);
        }

        /// <summary>
        /// 压缩图片(简化)，返回结果
        /// </summary>
        /// <param name="newPath">新路径</param>
        /// <param name="qt">压缩质量(0-100)</param>
        /// <returns></returns>
        public bool ResizeImg(string newPath, int qt)
        {
            return ResizeImg(-1, -1, newPath, qt);
        }

        /// <summary>
        /// 压缩图片(简化)，返回结果
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="newPath">新路径</param>
        /// <returns></returns>
        public bool ResizeImg(int width, string newPath)
        {
            return ResizeImg(width, -2, newPath, 80);
        }

        #endregion

        #region 剪切图片

        /// <summary>
        /// 剪切图片
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Bitmap MakeImage(int x, int y, int width, int height)
        {
            return ImageHelper.MakeImage(_oldImg, x, y, width, height);
        }

        #endregion

        #region 图像二值化

        /// <summary>
        /// 图像二值化
        /// </summary>
        /// <param name="threshold">阈值</param>
        public Bitmap BinarizeImage(byte threshold = Threshold)
        {
            return ImageHelper.BinarizeImage(_oldImg, threshold);
        }

        #endregion

        #region 图片旋转

        /// <summary>
        /// 任意旋转角度
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public Bitmap RotateImage(float angle, Color color)
        {
            return ImageHelper.RotateImage(_oldImg, angle, color);
        }

        /// <summary>
        /// 任意旋转角度
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public Bitmap RotateImage(float angle)
        {
            return ImageHelper.RotateImage(_oldImg, angle);
        }

        #endregion

        #region 图片纠偏

        /// <summary>
        /// 图像纠偏
        /// </summary>
        /// <returns></returns>
        public Bitmap Deskew()
        {
            return ImageHelper.Deskew(_oldImg);
        }

        #endregion

        #region 私有方法

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            //根据 mime 类型，返回编码器
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            mimeType = "image/" + mimeType.Replace(".", string.Empty).ToLower();
            mimeType = mimeType.Replace("jpg", "jpeg");
            return encoders.FirstOrDefault(t => t.MimeType == mimeType);
        }

        #endregion

        #region IDisposable 成员

        void IDisposable.Dispose()
        {
            if (_oldImg != null)
                _oldImg.Dispose();
        }

        #endregion

        /// <summary> 计算合并后的区域 </summary>
        /// <param name="rects"></param>
        /// <param name="round">边缘距离</param>
        /// <returns></returns>
        public static RectangleF CombineRegion(List<RectangleF> rects, int round = 0)
        {

            if (rects.IsNullOrEmpty())
                return new RectangleF();
            RectangleF combine;
            if (rects.Count == 1)
            {
                combine = rects.First();
            }
            else
            {
                combine = new RectangleF
                {
                    X = rects.Min(t => t.X),
                    Y = rects.Min(t => t.Y)
                };
                foreach (var region in rects)
                {
                    var width = region.X + region.Width - combine.X;
                    var height = region.Y + region.Height - combine.Y;
                    if (width > combine.Width)
                        combine.Width = width;
                    if (height > combine.Height)
                        combine.Height = height;
                }
            }
            //扩展边界
            if (round <= 0)
                return combine;
            if (combine.X - round > 0)
            {
                combine.X -= round;
                combine.Width += 2 * round;
            }
            else
            {
                combine.Width += combine.X + round;
                combine.X = 0;
            }
            if (combine.Y - round > 0)
            {
                combine.Y -= round;
                combine.Height += 2 * round;
            }
            else
            {
                combine.Height += combine.Y + round;
                combine.Y = 0;
            }
            return combine;
        }
    }
}
