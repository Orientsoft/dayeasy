using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Contracts.Enum.Question
{
    enum OrderArr:byte
    {
        [Description("错误率升序")]
        ErrRateAsc =1,
        [Description("错误率降序")]
        ErrRateDesc = 2,
        [Description("时间顺序")]
        Date =3
    }
}
