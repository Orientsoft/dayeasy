using System;
using System.Drawing;

namespace DayEasy.Utility.Helper
{
    /// <summary> 验证码类型 </summary>
    public enum VCodeType
    {
        /// <summary> 数字 </summary>
        Number = 0,

        /// <summary> 字母 </summary>
        Letter = 1,

        /// <summary> 数字和字母 </summary>
        NumberAndLetter = 2,

        /// <summary> 中文 </summary>
        ChineseWord = 3,

        /// <summary> 计算 </summary>
        Calculate = 4
    }

    /// <summary> 验证码辅助 </summary>
    public class VCodeHelper
    {
        private const string VcodeCacheKey = "__dayeasy_vcode_key";
        private readonly VCodeType _vcodeType;
        private readonly int _length;
        private string _vcode;
        private int _result = -1;
        private int _letterWidth = 22;
        private int _height = 32;

        private static readonly string[] FontFamilys = { "Arial", "宋体", "黑体" };
        private static readonly string[] Operators = { "+", "-", "×" };
        private static readonly FontStyle[] FontStyles = { FontStyle.Bold, FontStyle.Italic, FontStyle.Regular };

        public VCodeHelper(VCodeType type, int length)
        {
            _vcodeType = type;
            _length = length;
        }

        public Bitmap VCode(int letterWidth = 22, int height = 32)
        {
            GetCode();
            _letterWidth = letterWidth;
            _height = height;
            SessionHelper.Add(VcodeCacheKey, _result >= 0 ? _result.ToString() : _vcode);
            return CreateBmp();
        }

        public static bool Verify(string code, bool remove = true)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;
            var cacheCode = SessionHelper.Get<string>(VcodeCacheKey);
            var result = string.Equals(cacheCode, code, StringComparison.CurrentCultureIgnoreCase);
            if (result && remove)
                SessionHelper.Remove(VcodeCacheKey);
            return result;
        }

        private void GetCode()
        {
            _result = -1;
            switch (_vcodeType)
            {
                case VCodeType.Number:
                    _vcode = RandomHelper.RandomNums(_length);
                    break;
                case VCodeType.Letter:
                    _vcode = RandomHelper.RandomLetters(_length);
                    break;
                case VCodeType.NumberAndLetter:
                    _vcode = RandomHelper.RandomNumAndLetters(_length);
                    break;
                case VCodeType.ChineseWord:
                    _vcode = RandomHelper.RandomHanzi(_length);
                    break;
                case VCodeType.Calculate:
                    GetCalculate();
                    break;
                default:
                    _vcode = RandomHelper.RandomNums(_length);
                    break;
            }
        }

        private Bitmap CreateBmp()
        {
            var intImageWidth = _vcode.Length * _letterWidth;
            var image = new Bitmap(intImageWidth, _height);
            var g = Graphics.FromImage(image);
            g.Clear(Color.White);
            var random = RandomHelper.Random();
            int i, x = (0 - _letterWidth) / 2, y;
            for (i = 0; i < _vcode.Length; i++)
            {
                var font = RandomFont();
                x += random.Next(_letterWidth / 2, _letterWidth);
                y = random.Next(2, (int)Math.Floor(_height - font.Size) - 2);
                var strChar = _vcode.Substring(i, 1);
                Brush newBrush = new SolidBrush(RandomColor(0, 120));
                var thePos = new Point(x, y);
                g.DrawString(strChar, font, newBrush, thePos);
            }
            DrawPoints(image, 50);
            DrawArc(image, 8);
            g.DrawRectangle(new Pen(Color.LightGray, 1), 0, 0, intImageWidth - 1, (_height - 1));
            return image;
        }

        /// <summary>
        /// 正弦曲线Wave扭曲图片
        /// </summary>
        /// <param name="srcBmp">图片路径</param>
        /// <param name="count">数量</param>
        private static void DrawArc(Image srcBmp, int count = 2)
        {
            using (var g = Graphics.FromImage(srcBmp))
            {
                var random = RandomHelper.Random();
                for (int i = 0; i < count; i++)
                {
                    int x = random.Next(1, srcBmp.Width / 2),
                        y = random.Next(1, srcBmp.Height / 2),
                        width = random.Next(3, srcBmp.Width - x - 1),
                        height = random.Next(3, srcBmp.Height - y - 1),
                        startX = random.Next(50, 260),
                        startY = random.Next(50, 260);

                    g.DrawArc(new Pen(RandomColor(150, 220), 1F), x, y, width, height, startX, startY);
                }
            }
        }

        private static void DrawPoints(Bitmap bmp, int count)
        {
            int x, y;
            var random = RandomHelper.Random();
            for (int i = 0; i < count; i++)
            {
                x = random.Next(bmp.Width - 1);
                y = random.Next(bmp.Height - 1);
                bmp.SetPixel(x, y, RandomColor());
            }
        }

        /// <summary> 字体随机颜色 </summary>
        private static Color RandomColor(int min = 0, int max = 180)
        {
            var random = RandomHelper.Random();
            var intRed = random.Next(min, max);
            var intGreen = random.Next(min, max);
            var intBlue = random.Next(min, max);
            return Color.FromArgb(intRed, intGreen, intBlue);
        }

        /// <summary> 随机字体 </summary>
        /// <returns></returns>
        private static Font RandomFont()
        {
            var random = RandomHelper.Random();
            var fontFamily = new FontFamily(FontFamilys[random.Next(FontFamilys.Length - 1)]);
            var fontStyle = FontStyles[random.Next(FontStyles.Length - 1)];
            return new Font(fontFamily, random.Next(14, 17), fontStyle);
        }

        private void GetCalculate()
        {
            var random = RandomHelper.Random();
            var op = random.Next(Operators.Length);
            int x, y;
            switch (op)
            {
                case 0:
                    x = random.Next(1, 50);
                    y = random.Next(1, 50);
                    _vcode = string.Concat(x, Operators[op], y);
                    _result = x + y;
                    break;
                case 1:
                    x = random.Next(1, 100);
                    y = random.Next(1, x - 1);
                    _vcode = string.Concat(x, Operators[op], y);
                    _result = x - y;
                    break;
                case 2:
                    x = random.Next(1, 10);
                    y = random.Next(1, 10);
                    _vcode = string.Concat(x, Operators[op], y);
                    _result = x * y;
                    break;
            }
        }
    }
}
