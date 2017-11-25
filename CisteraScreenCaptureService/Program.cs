using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace CisteraScreenCaptureService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
        }

        //[ServiceContract(Namespace = "http://Microsoft.ServiceModel.Samples", SessionMode = SessionMode.Required, CallbackContract = typeof(IClientApi))]
        //public interface IServerApi
        //{
        //    [OperationContract(IsOneWay = true)]
        //    void ReloadSetting();
        //}
        //public interface IClientApi
        //{
        //    [OperationContract(IsOneWay = true)]
        //    void ServiceStarted();
        //    [OperationContract(IsOneWay = true)]
        //    void ServiceStopped();
        //}

        //[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
        //public class CalculatorService : IServerApi
        //{
        //    public void ReloadSetting()
        //    {
        //    }
        //}
    }
}
