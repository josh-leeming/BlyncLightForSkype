using System;
using System.Windows.Forms;
using BlyncLightForSkype.App.Properties;
using BlyncLightForSkype.Client;
using TinyIoC;

namespace BlyncLightForSkype.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = TinyIoCContainer.Current;
            IoC.ConfigureContainer(container);

            var blyncLightForSkypeClient = container.Resolve<BlyncLightForSkypeClient>();
            container.BuildUp(blyncLightForSkypeClient);

            blyncLightForSkypeClient.StartClient();

            using (var icon = new ProcessIcon(blyncLightForSkypeClient))
            {
                icon.Display();

                Application.Run();
            }
        }

        public class ProcessIcon : IDisposable
        {
            private readonly BlyncLightForSkypeClient blyncLightForSkypeClient;
            private readonly NotifyIcon notifyIcon;

            public ProcessIcon(BlyncLightForSkypeClient blyncLightForSkypeClient)
            {
                this.blyncLightForSkypeClient = blyncLightForSkypeClient;
                this.notifyIcon = new NotifyIcon();
            }

            public void Display()
            {
                notifyIcon.Icon = Resources.TrayIcon_Running;
                notifyIcon.Text = Resources.TrayIcon_Text;
                notifyIcon.Visible = true;

                notifyIcon.ContextMenuStrip = new ContextMenuStrip();

                var start = new ToolStripMenuItem("Start") { Enabled = false };
                var stop = new ToolStripMenuItem("Stop") { Enabled = true };
                var exit = new ToolStripMenuItem("Exit");

                start.Click += (sender, args) =>
                {
                    if (blyncLightForSkypeClient.IsRunning == false)
                    {
                        blyncLightForSkypeClient.StartClient();
                        notifyIcon.Icon = Resources.TrayIcon_Running;
                        start.Enabled = false;
                        stop.Enabled = true;
                    }
                };

                stop.Click += (sender, args) =>
                {
                    if (blyncLightForSkypeClient.IsRunning)
                    {
                        blyncLightForSkypeClient.StopClient();
                        notifyIcon.Icon = Resources.TrayIcon_Stopped;
                        start.Enabled = true;
                        stop.Enabled = false;
                    }
                };

                exit.Click += (sender, args) =>
                {
                    if (blyncLightForSkypeClient.IsRunning)
                    {
                        blyncLightForSkypeClient.StopClient();
                    }
                    Application.Exit();
                };

                notifyIcon.ContextMenuStrip.Items.Add(start);
                notifyIcon.ContextMenuStrip.Items.Add(stop);
                notifyIcon.ContextMenuStrip.Items.Add(exit);
            }

            public void Dispose()
            {
                notifyIcon.Dispose();
            }
        }
    }
}
