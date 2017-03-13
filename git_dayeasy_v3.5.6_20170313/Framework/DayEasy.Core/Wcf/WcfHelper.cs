using DayEasy.Core.Dependency;
using DayEasy.Core.Reflection;
using DayEasy.Utility;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace DayEasy.Core.Wcf
{
    /// <summary> Wcf辅助类 </summary>
    public class WcfHelper
    {
        private readonly ILogger _logger = LogManager.Logger<WcfHelper>();
        private WcfHelper() { }
        public static WcfHelper Instance => (Singleton<WcfHelper>.Instance ?? (Singleton<WcfHelper>.Instance = new WcfHelper()));

        private string WcfHost => ConfigHelper.GetAppSetting(defaultValue: string.Empty);

        /// <summary> Type查找器 </summary>
        public ITypeFinder TypeFinder { private get; set; }
        /// <summary> 注入管理 </summary>
        public IIocManager IocManager { private get; set; }

        /// <summary> 开启WCF服务 </summary>
        public void StartService()
        {
            if (string.IsNullOrWhiteSpace(WcfHost))
                return;
            var types =
                TypeFinder.Find(
                    t => t.IsInterface && t != typeof(IWcfService) && typeof(IWcfService).IsAssignableFrom(t));
            if (types == null || types.Length == 0)
                return;
            foreach (var type in types)
            {
                var resolve = IocManager.Resolve(type);
                OpenService(type, resolve.GetType());
            }
        }

        /// <summary> 打开接口服务 </summary>
        /// <param name="interfaceType"></param>
        /// <param name="classType"></param>
        private void OpenService(Type interfaceType, Type classType)
        {
            var uri = new Uri(WcfHost + "/" + interfaceType.Name);
            var host = new ServiceHost(classType);
            host.AddServiceEndpoint(interfaceType, new WSHttpBinding(), uri);
            if (host.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
            {
                var behavior = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    HttpGetUrl = uri
                };
                host.Description.Behaviors.Add(behavior);
            }
            var debugBehavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (debugBehavior != null)
                debugBehavior.IncludeExceptionDetailInFaults = true;
            else
            {
                debugBehavior = new ServiceDebugBehavior
                {
                    IncludeExceptionDetailInFaults = true
                };
                host.Description.Behaviors.Add(debugBehavior);
            }
            host.Opened += delegate
            {
                _logger.Info("service:{0} 已启动！", uri);
            };
            host.Closed += delegate
            {
                _logger.Info("service:{0} 已关闭！", uri);
            };
            host.Open();
        }

        /// <summary> 调用WCF服务 </summary>
        /// <typeparam name="TContract">需要调用的WCF契约</typeparam>
        /// <param name="action">需要调用的WCF方法</param>
        public void Call<TContract>(Action<TContract> action)
        {
            var address = WcfHost + "/" + typeof(TContract).Name;
            var factory = new ChannelFactory<TContract>(new WSHttpBinding(), new EndpointAddress(new Uri(address)));
            var channel = factory.CreateChannel();
            var client = ((IClientChannel)channel);

            try
            {
                client.Open();
                action(channel);
                client.Close();
            }
            catch
            {
                client.Abort();
            }
        }
    }
}
