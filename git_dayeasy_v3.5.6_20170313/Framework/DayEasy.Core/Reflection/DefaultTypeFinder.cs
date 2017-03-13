using DayEasy.Utility.Extend;
using DayEasy.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Core.Reflection
{
    public class DefaultTypeFinder : ITypeFinder
    {
        public IAssemblyFinder AssemblyFinder { get; set; }

        public Type[] Find(Func<Type, bool> expression)
        {
            return FindTypes(expression).ToArray();
        }

        public Type[] FindAll()
        {
            return FindTypes().ToArray();
        }

        private List<Type> FindTypes(Func<Type, bool> expression = null)
        {
            if (expression == null)
                expression = t => true;
            var types = new List<Type>();
            try
            {
                foreach (var assembly in AssemblyFinder.FindAll().Distinct())
                {
                    var assTypes = assembly.GetTypes();
                    if (!assTypes.Any())
                        continue;
                    var list = assTypes.Where(expression).ToList();
                    if (!list.Any())
                        continue;
                    types.AddRange(list.Where(t => t != null));
                }
            }
            catch (Exception ex)
            {
                LogManager.Logger<DefaultTypeFinder>().Error(ex.Message, ex);
            }
            return types;
        }
    }
}
