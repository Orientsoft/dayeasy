
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace DayEasy.Office
{
    /// <summary> 得一号填涂区域辅助 </summary>
    public class DcodeHelper
    {
        private readonly int _length;
        private readonly int _zoom;
        private int _lineHeight;
        private int _preWidth;
        private int _borderWidth;
        private int _wordWidth;
        private int _width;
        private int _height;
        private const int Resolution = 96;
        private const string TipWord = "填涂得一号";

        private void Reset()
        {
            _lineHeight = 20 * _zoom;
            _preWidth = 28 * _zoom;
            _borderWidth = 2 * _zoom;
            _wordWidth = 30 * _zoom;
            _width = _preWidth * 11 + _wordWidth + _borderWidth * 2;
            _height = _lineHeight * _length;
        }

        /// <summary> 构造函数 </summary>
        /// <param name="length"></param>
        /// <param name="zoom"></param>
        public DcodeHelper(int length = 5, int zoom = 1)
        {
            _length = length;
            _zoom = zoom;
            Reset();
        }

        /// <summary> 画图 </summary>
        public Bitmap Draw()
        {
            var pen = new Pen(new SolidBrush(Color.Black), 1.0F * _zoom);
            var doublePen = new Pen(new SolidBrush(Color.Black), 2.0F * _zoom);
            var dashPen = new Pen(new SolidBrush(Color.Black), 1.0F * _zoom)
            {
                DashStyle = DashStyle.Custom,
                DashPattern = new[] { 5.0F, 6.0F }
            };
            var font = new Font("宋体", 12.0F * _zoom, FontStyle.Bold);
            var optionFont = new Font("Microsoft Himalaya", 14.0F * _zoom, FontStyle.Bold, GraphicsUnit.Pixel);
            var brush = new SolidBrush(Color.Black);

            var bmp = new Bitmap(_width, _height);
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
                //文字居中
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(TipWord, font, brush, new RectangleF(5 * _zoom, 5 * _zoom, _wordWidth, _height));

                //矩形线框
                g.DrawRectangle(doublePen,
                    new Rectangle(_wordWidth + _borderWidth / 2, _borderWidth / 2, _width - _wordWidth - _borderWidth,
                        _height - _borderWidth));

                for (var i = 0; i < _length; i++)
                {
                    //数字
                    for (int j = 0; j < 11; j++)
                    {
                        if (j == 0)
                        {
                            g.DrawLine(pen, new Point(_wordWidth + _preWidth, 0),
                                new Point(_wordWidth + _preWidth, _height));
                        }
                        else
                        {
                            var word = string.Format("[ {0} ]", j - 1);
                            var rect = new RectangleF(j * _preWidth + _wordWidth + _borderWidth, i * _lineHeight + 3 * _zoom, _preWidth,
                                _lineHeight);
                            g.DrawString(word, optionFont, brush, rect, sf);
                        }
                    }
                    if (i > 0)
                    {
                        //虚线横线
                        g.DrawLine(dashPen, _wordWidth + _borderWidth / 2, (_lineHeight * i) + _borderWidth / 2, _width,
                            _lineHeight * i + _borderWidth / 2);
                    }
                }
            }
            return bmp;
        }
    }
}
