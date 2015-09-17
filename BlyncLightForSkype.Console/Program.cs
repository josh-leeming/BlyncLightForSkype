using System;
using System.ServiceProcess;
using BlyncLightForSkype.Client;
using TinyIoC;

namespace BlyncLightForSkype.Console
{
    class Program
    {
        #region Nested classes to support running as service
        public const string ServiceName = "BlyncLightForSkype";

        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = Program.ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                Program.Start(args);
            }

            protected override void OnStop()
            {
                Program.Stop();
            }
        }
        #endregion

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                // running as service
                using (var service = new Service())
                {
                    ServiceBase.Run(service);
                }
            }
            else
            {
                // running as console app
                Start(args);

                System.Console.WriteLine("Press any key to stop...");
                System.Console.ReadKey(true);

                Stop();
            }
        }

        private static void Start(string[] args)
        {
            var container = TinyIoCContainer.Current;
            try
            {
                IoC.ConfigureContainer(container);

                var blyncLightForSkypeClient = container.Resolve<BlyncLightForSkypeClient>();
                container.BuildUp(blyncLightForSkypeClient);

                blyncLightForSkypeClient.StartClient();

                System.Console.ReadLine();

                blyncLightForSkypeClient.StopClient();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }

        private static void Stop()
        {
            // onstop code here
        }
    }
}
