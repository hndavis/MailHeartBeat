using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;

namespace MailHeartBeat
{
    partial class MailHeartBeatService : ServiceBase
    {
        HeartBeatListener hbl;
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static void Main(string[] args)
        {

            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //    {
            //        new MailHeartBeatService(args)
            //    };

            ServiceBase.Run(new MailHeartBeatService(args));
        }
    

        public MailHeartBeatService(string[] args)
        {
            log.Info(" Creating Service");
            InitializeComponent();
            string eventSourceName = "MailHeartBeatService";
            string logName = "MailHeartBeatLog";
            if (args.Count() > 0)
                { eventSourceName = args[0]; }
            if (args.Count() > 1)
                { logName = args[1]; }

           var  eventLog1 = new System.Diagnostics.EventLog(eventSourceName);
            //if (!System.Diagnostics.EventLog.SourceExists(eventSourceName))
            //{
            //    System.Diagnostics.EventLog.CreateEventSource(eventSourceName, logName);
            //}

            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
            hbl = new HeartBeatListener();
        }

        protected override void OnStart(string[] args)
        {
            log.Info(" Starting Service");
            hbl.Run();
            
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            hbl.StartShutDown();
        }

        

    }
}
