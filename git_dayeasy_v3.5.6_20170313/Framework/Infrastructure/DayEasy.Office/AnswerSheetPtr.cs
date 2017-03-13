using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DayEasy.Office
{
    /// <summary> 答题卡类型 </summary>
    public enum AnswerSheetType
    {
        /// <summary> 不同类型 </summary>
        Normal = 0,

        /// <summary> 不换行 </summary>
        Nowrap = 1
    }

    /// <summary> 区域点 </summary>
    public class DPoint
    {
        /// <summary> 行 </summary>
        public int Row { get; set; }
        /// <summary> 列 </summary>
        public int Colunm { get; set; }

        public DPoint(int row, int colunm)
        {
            Row = row;
            Colunm = colunm;
        }
    }

    /// <summary> 答题卡指针 </summary>
    public class AnswerSheetPtr
    {
        private readonly Dictionary<string, int> _sheets;
        private readonly Dictionary<string, DPoint> _sheetPoints;
        private readonly AnswerSheetType _type;
        /// <summary> 每行列数 </summary>
        public const int Colunms = 24;

        /// <summary> 构造函数 </summary>
        /// <param name="sheets"></param>
        /// <param name="type"></param>
        public AnswerSheetPtr(Dictionary<string, int> sheets, AnswerSheetType type = AnswerSheetType.Nowrap)
        {
            _sheets = sheets;
            _type = type;
            _sheetPoints = new Dictionary<string, DPoint>();
            Init();
        }

        private void Init()
        {
            if (_sheets == null || !_sheets.Any())
            {
                Rows = 1;
                return;
            }
            foreach (var sheet in _sheets)
            {
                var count = sheet.Value;
                if (Colunm + count >= Colunms && _type == AnswerSheetType.Nowrap)
                {
                    //换行
                    Update(Colunms - Colunm);
                }
                _sheetPoints.Add(sheet.Key, new DPoint(Row, Colunm));
                Update(count + 1);
                if (Colunm != 0)
                    Update();
            }
            Rows = Row + (Colunm > 0 ? 1 : 0);
            Reset();
        }

        public void Set(DPoint point)
        {
            Row = point.Row;
            Colunm = point.Colunm;
        }
        /// <summary> 更新坐标 </summary>
        /// <param name="step"></param>
        public void Update(int step = 1)
        {
            Colunm += step;
            if (Colunm < Colunms)
                return;
            //换行
            var item = Colunm - Colunms;
            Row += (int)Math.Ceiling((item + 1) / (double)Colunms);
            Colunm = (item % Colunms);
        }

        /// <summary> 重置坐标 </summary>
        public void Reset()
        {
            Row = 0;
            Colunm = 0;
        }

        /// <summary> 总行数 </summary>
        public int Rows { get; private set; }
        /// <summary> 当前行 </summary>
        public int Row { get; private set; }
        /// <summary> 当前列 </summary>
        public int Colunm { get; private set; }

        /// <summary> 获取某题坐标 </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DPoint GetPoint(string key)
        {
            if (_sheetPoints.ContainsKey(key))
                return _sheetPoints[key];
            return new DPoint(-1, -1);
        }
    }
}
