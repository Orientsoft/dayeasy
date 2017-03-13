
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Utility.Extend;

namespace DayEasy.Services.Helper
{
    /// <summary> 序号池辅助 </summary>
    public class CodePoolHelper
    {
        private readonly Version3Repository<TS_CodePool, int> _codePoolRepository;

        private CodePoolHelper()
        {
            _codePoolRepository = CurrentIocManager.Resolve<Version3Repository<TS_CodePool, int>>();
        }

        /// <summary> 获取序号 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public long Code(byte type)
        {
            Expression<Func<TS_CodePool, bool>> condition = t => t.Type == type && t.Level == 0;
            var model = _codePoolRepository.Where(condition)
                .RandomSort()
                .FirstOrDefault();
            if (model == null)
                return 0L;
            var code = model.Code;
            _codePoolRepository.Delete(model);
            return code;
        }

        public void GenerateCode(byte type)
        {
            var min = 1001L;
            if (_codePoolRepository.Exists(t => t.Type == type))
                min = _codePoolRepository.Where(t => t.Type == type).Max(t => t.Code) + 1;
            var models = new List<TS_CodePool>();
            for (var i = 0; i < 5000; i++)
            {
                var code = min + i;
                byte level = 0;
                models.Add(new TS_CodePool
                {
                    Code = code,
                    Type = type,
                    Level = level
                });
            }
            _codePoolRepository.Insert(models);
        }
    }
}
