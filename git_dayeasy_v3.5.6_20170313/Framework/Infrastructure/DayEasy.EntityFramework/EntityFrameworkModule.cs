using DayEasy.Core;
using DayEasy.Core.Modules;
using DayEasy.Core.Reflection;
using DayEasy.Utility.Extend;

namespace DayEasy.EntityFramework
{
    [DependsOn(typeof(CoreModule))]
    public class EntityFrameworkModule : DModule
    {
        private readonly ITypeFinder _typeFinder;

        public EntityFrameworkModule(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        public override void Initialize()
        {
            var dbContextTypes =
                _typeFinder.Find(
                    type =>
                        type.IsPublic && !type.IsAbstract && type.IsClass &&
                        typeof (CodeFirstDbContext).IsAssignableFrom(type));
            if(dbContextTypes.IsNullOrEmpty())
                return;
        }
    }
}
