using System;
using System.Runtime.InteropServices;
using BlyncLightForSkype.App;
using BlyncLightForSkype.Client;
using TinyIoC;

namespace BlyncLightForSkype.Console
{
    class Program
    {
        private static BlyncLightForSkypeClient blyncLightForSkypeClient = null;

        private static ConsoleEventDelegate consoleEventCallbackHandler;   
        private delegate bool ConsoleEventDelegate(int eventType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        static void Main(string[] args)
        {
            Start(args);

            consoleEventCallbackHandler = ConsoleEventCallback;
            SetConsoleCtrlHandler(consoleEventCallbackHandler, true);

            System.Console.WriteLine("Press any key to stop...");
            System.Console.ReadKey(true);

            Stop();
        }

        private static void Start(string[] args)
        {
            var container = TinyIoCContainer.Current;
            try
            {
                IoC.ConfigureContainer(container);

                blyncLightForSkypeClient = container.Resolve<BlyncLightForSkypeClient>();
                container.BuildUp(blyncLightForSkypeClient);

                blyncLightForSkypeClient.StartClient();
            }
            catch (Exception e)
            {
                if (blyncLightForSkypeClient != null && blyncLightForSkypeClient.IsRunning)
                {
                    blyncLightForSkypeClient.StopClient();
                }
                System.Console.WriteLine(e.Message);
            }
        }

        private static void Stop()
        {
            try
            {
                blyncLightForSkypeClient.StopClient();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }

        private static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                Stop();
            }
            return false;
        }
    }
}
