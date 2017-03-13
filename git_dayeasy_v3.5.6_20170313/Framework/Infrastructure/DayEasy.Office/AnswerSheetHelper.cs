using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;

namespace DayEasy.Office
{
    /// <summary> 答题卡辅助类 </summary>
    public class AnswerSheetHelper
    {
        private static int _size = 1;
        private static int _lineHeight;
        private static int _preWidth;
        private int _borderWidth;
        private int _halfBorder;
        private static int _width;
        private int _tipsHeight;
        private const int Resolution = 96;
        private const string TipsWord = "请使用2B铅笔规范填涂！";
        private readonly Dictionary<string, int> _answerSheetData;
        private AnswerSheetPtr _ptr;

        //private class SheetPtr
        //{
        //    private int Colunm { get; set; }
        //    private int Row { get; set; }

        //    public void Update()
        //    {
        //        if (Colunm == LineCount - 1)
        //        {
        //            Colunm = 0;
        //            Row++;
        //        }
        //        else
        //        {
        //            Colunm++;
        //        }
        //    }

        //    public PointF Point(int wordLenght)
        //    {
        //        return new PointF(Colunm * _preWidth + 14 * _size - wordLenght * 3 * _size, Row * _lineHeight + 4 * _size);
        //    }
        //}

        private void Reset()
        {
            _lineHeight = 20 * _size;
            _tipsHeight = 15 * _size;
            _preWidth = 30 * _size;
            _borderWidth = 2 * _size;
            _halfBorder = _size;
            _width = _preWidth * AnswerSheetPtr.Colunms;
        }

        public AnswerSheetHelper(Dictionary<string, int> data, AnswerSheetType sheetType = AnswerSheetType.Nowrap)
        {
            _answerSheetData = data;
            Reset();
            _ptr = new AnswerSheetPtr(data, sheetType);
        }

        private PointF GetPointF(DPoint point, int wordLength)
        {
            return new PointF(point.Colunm * _preWidth + 14 * _size - wordLength * 3 * _size, point.Row * _lineHeight + 4 * _size);
        }

        public Bitmap GenerateBmp(int size = 0, AnswerSheetTips tips = AnswerSheetTips.None)
        {
            if (size > 0)
            {
                _size = size;
                Reset();
            }
            var row = _ptr.Rows;
            var height = row * _lineHeight;
            //var pen = new Pen(new SolidBrush(Color.Black), 1.0F * _size);
            var doublePen = new Pen(new SolidBrush(Color.Black), 2.0F * _size);
            //var dashPen = new Pen(new SolidBrush(Color.Black), 1.0F * _size)
            //{
            //    DashStyle = DashStyle.Custom,
            //    DashPattern = new[] { 5.0F, 6.0F }
            //};
            var font = new Font("Arial", 9.0F * _size, FontStyle.Regular);
            var optionFont = new Font("Microsoft Himalaya", 14.0F * _size, FontStyle.Bold, GraphicsUnit.Pixel);
            var brush = new SolidBrush(Color.Black);

            var bmp = new Bitmap(_width + _borderWidth, height + _borderWidth);
            bmp.SetResolution(Resolution, Resolution);
            using (var g = Graphics.FromImage(bmp))
            {
                //呈现质量
                g.CompositingQuality = CompositingQuality.HighQuality;
                //像素偏移方式
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //平滑处理
                g.SmoothingMode = SmoothingMode.HighQuality;
                //插补模式,双三次插值法
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //背景填充
                g.Clear(Color.Transparent);
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;

                //矩形线框
                g.DrawRectangle(doublePen, new Rectangle(_halfBorder, _halfBorder, _width, height));
                ////虚线横线
                //for (int i = 1; i < row; i++)
                //{
                //    g.DrawLine(dashPen, _halfBorder, _lineHeight * i + _halfBorder, _width, _lineHeight * i + _halfBorder);
                //}
                //实线竖线
                //for (int i = 1; i < AnswerSheetPtr.Colunms; i++)
                //{
                //    g.DrawLine(pen, _preWidth * i, _halfBorder, _preWidth * i, height);
                //}
                if (_answerSheetData != null && _answerSheetData.Any())
                {
                    //填充答题卡
                    foreach (var key in _answerSheetData.Keys)
                    {
                        var keyPoint = _ptr.GetPoint(key);
                        g.DrawString(key, font, brush, GetPointF(keyPoint, key.Length));
                        _ptr.Set(keyPoint);
                        _ptr.Update();
                        for (int i = 0; i < _answerSheetData[key]; i++)
                        {
                            var word = string.Format("[ {0} ]",
                                Convert.ToChar(65 + i).ToString(CultureInfo.InvariantCulture));
                            var point = GetPointF(new DPoint(_ptr.Row, _ptr.Colunm), word.Length - 1); //ptr.Point(word.Length - 1);
                            point.Y += _size * 1;
                            point.X += _size * 2;
                            g.DrawString(word, optionFont, brush, point);
                            _ptr.Update();
                        }
                        _ptr.Update();
                    }
                }
            }
            if (tips != AnswerSheetTips.None)
            {
                bmp = MakeTips(bmp, tips);
            }
            return bmp;
        }

        /// <summary>
        /// 设置提示
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="tips"></param>
        /// <returns></returns>
        private Bitmap MakeTips(Bitmap bmp, AnswerSheetTips tips)
        {
            if (_answerSheetData == null || !_answerSheetData.Any())
                return bmp;
            using (bmp)
            {
                var tipBmp = new Bitmap(bmp.Width, bmp.Height + _tipsHeight);
                tipBmp.SetResolution(Resolution, Resolution);
                using (var g = Graphics.FromImage(tipBmp))
                {
                    //呈现质量
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    //像素偏移方式
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    //平滑处理
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    //插补模式,双三次插值法
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    //背景填充
                    g.Clear(Color.Transparent);
                    var y = 0;
                    if ((byte)tips <= 3)
                        y = _tipsHeight;
                    g.DrawImage(bmp, new PointF(0, y));
                    var tipsFont = new Font("宋体", 11.0F * _size, FontStyle.Regular, GraphicsUnit.Pixel);
                    var brush = new SolidBrush(Color.Black);
                    var len = 110 * _size;
                    var point = new PointF(10 * _size, 3 * _size);
                    switch (tips)
                    {
                        case AnswerSheetTips.Top:
                            point.X = (_width - len) / 2.0F;
                            break;
                        case AnswerSheetTips.TopRight:
                            point.X = _width - len;
                            break;
                        case AnswerSheetTips.BottomLeft:
                            point.Y += bmp.Height - _size;
                            break;
                        case AnswerSheetTips.Bottom:
                            point.X = (_width - len) / 2.0F;
                            point.Y += bmp.Height - _size;
                            break;
                        case AnswerSheetTips.BottomRight:
                            point.X = _width - len;
                            point.Y += bmp.Height - _size;
                            break;
                    }
                    g.FillRectangle(brush, point.X, point.Y + (1F * _size), 10 * _size, 8 * _size);
                    point.X += 12 * _size;
                    g.DrawString(TipsWord, tipsFont, brush, point);
                }
                return tipBmp;
            }
        }
    }

    public enum AnswerSheetTips : byte
    {
        None = 0,
        TopLeft = 1,
        Top = 2,
        TopRight = 3,
        BottomLeft = 4,
        Bottom = 5,
        BottomRight = 6,
    }
}
