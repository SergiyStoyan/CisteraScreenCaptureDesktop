using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Cliver.CisteraScreenCaptureService
{
    static class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args)
            {
                Exception e = (Exception)args.ExceptionObject;
                Log.Main.Error(e);
            };            

            Log.Initialize(Log.Mode.SESSIONS, Log.GetAppCommonDataDir());

            //Config.Initialize(new string[] { "General" });
            Cliver.Config.Reload();
        }

        static void Main()
        {
            try
            {
                Log.Main.Inform("Version: " + AssemblyRoutines.GetAppVersion());

                ServiceBase.Run(new Service());
            }
            catch(Exception e)
            {
                Log.Main.Error(e);
            }
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
