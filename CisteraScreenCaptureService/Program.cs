using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

//installing service:
//"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe" "d:\_d\_PROJECTS\CisteraScreenCapture\CisteraScreenCaptureService\bin\Debug\CisteraScreenCaptureService.exe"
//"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe" /u "d:\_d\_PROJECTS\CisteraScreenCapture\CisteraScreenCaptureService\bin\Debug\CisteraScreenCaptureService.exe"

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
            
            Log.Initialize(Log.Mode.ONLY_LOG, Log.CliverSoftCommonDataDir);

            //Config.Initialize(new string[] { "General" });
            Cliver.Config.Reload();
        }

        static void Main()
        {
            try
            {
                Log.Main.Inform("Application version: " + AssemblyRoutines.GetAppVersion());
                
                string m = "Appication user: " + WindowsUserRoutines.GetCurrentUserName3() + " (";
                if (WindowsUserRoutines.CurrentUserIsAdministrator())
                {
                    m += "administrator";
                    if (WindowsUserRoutines.CurrentUserHasElevatedPrivileges())
                        m += ", elevated privileges";
                    else
                        m += ", not elevated privileges";
                }
                else
                    m += "not administrator";
                Log.Main.Inform(m + ")");

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
