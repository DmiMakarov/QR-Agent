using System;
using System.ServiceProcess;
using System.Timers;
using System.Runtime.InteropServices;
using System.Configuration;
using System.IO;

namespace QR_Agent
{
    public partial class QR_Agent : ServiceBase
    {

        private string log_path; 
        private string url_server_get; 
        private string url_server_post;
        private string url_keeper;
        private string auth_keeper_name;
        private string auth_keeper_password;
        private string token_server;
        private Request request = new Request();
        public QR_Agent()
        {
            InitializeComponent();
            
        }
        private void writeLog(string message) {
            using (StreamWriter sw = File.AppendText(log_path + "\\QR-agent.log"))
            {
                sw.WriteLine($"{DateTime.Now.ToString()} " + message);
            }
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Сheck if the logging path exists
            log_path = ConfigurationManager.AppSettings.Get("log_path");
            if (log_path == "") log_path = ".\\log";
            if (!Directory.Exists(log_path))
            {
                Directory.CreateDirectory(log_path);
            }
            if (!Directory.Exists(log_path + "\\XML"))
            {
                Directory.CreateDirectory(log_path + "\\XML");
            }

            // Get params from config file
            url_server_get = ConfigurationManager.AppSettings.Get("url_server_get");
            url_server_post = ConfigurationManager.AppSettings.Get("url_server_post");
            url_keeper = ConfigurationManager.AppSettings.Get("url_keeper");
            auth_keeper_name = ConfigurationManager.AppSettings.Get("auth_keeper_name");
            auth_keeper_password = ConfigurationManager.AppSettings.Get("auth_keeper_pass");
            token_server = ConfigurationManager.AppSettings.Get("token_server");
           
            // Try to read and set period of sending requests  
            int time_s;
            try
            {
                time_s = int.Parse(ConfigurationManager.AppSettings.Get("time_s"));

            }
            catch (Exception e)
            {
                time_s = 5;
                writeLog("When start service get error: " + e.Message);
            }
            Timer timer = new Timer();
            timer.Interval = time_s*1000 ; 
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            writeLog("Start service");
        }

        protected override void OnStop()
        {
            writeLog("Stop service");
        }
        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            request.RunAsync(log_path, url_server_get, url_server_post, url_keeper, auth_keeper_name, 
                auth_keeper_password, token_server).GetAwaiter().GetResult();
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
    }
}

public enum ServiceState
{
    SERVICE_STOPPED = 0x00000001,
    SERVICE_START_PENDING = 0x00000002,
    SERVICE_STOP_PENDING = 0x00000003,
    SERVICE_RUNNING = 0x00000004,
    SERVICE_CONTINUE_PENDING = 0x00000005,
    SERVICE_PAUSE_PENDING = 0x00000006,
    SERVICE_PAUSED = 0x00000007,
}

[StructLayout(LayoutKind.Sequential)]
public struct ServiceStatus
{
    public int dwServiceType;
    public ServiceState dwCurrentState;
    public int dwControlsAccepted;
    public int dwWin32ExitCode;
    public int dwServiceSpecificExitCode;
    public int dwCheckPoint;
    public int dwWaitHint;
};

