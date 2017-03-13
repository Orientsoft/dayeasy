
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DayEasy.Utility;

namespace DayEasy.Services.Helper
{
    public class SystemCache
    {
        private static ConcurrentDictionary<int, string> _subjectCache;

        private SystemCache()
        {
            _subjectCache = new ConcurrentDictionary<int, string>();
        }

        public static SystemCache Instance
        {
            get
            {
                return Singleton<SystemCache>.Instance
                       ?? (Singleton<SystemCache>.Instance = new SystemCache());
            }
        }

        public Dictionary<int, string> Subjects(IEnumerable<int> subjectIds)
        {
            return _subjectCache.Where(t => subjectIds.Contains(t.Key)).ToDictionary(k => k.Key, v => v.Value);
        }

        public string Subject(int subjectId)
        {
            string name;
            if (!_subjectCache.TryGetValue(subjectId, out name))
                name = string.Empty;
            return name;
        }

        public void InitSubjects(IDictionary<int, string> subjects)
        {
            foreach (var subject in subjects)
            {
                _subjectCache.TryAdd(subject.Key, subject.Value);
            }
        }
    }
}
