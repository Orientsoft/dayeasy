using DayEasy.Framework;

namespace FrameworkTest
{
    public abstract class TestBase
    {
        //protected IContainer Container { get; private set; }
        protected TestBase()
        {
            var bootstrap = DayEasyBootstrap.Instance;
            bootstrap.Initialize();
        }
    }
}
