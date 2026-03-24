using System.Windows;

namespace SleepOnLan
{
    public partial class App : System.Windows.Application
    {
        public static bool StartMinimized { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            foreach (string arg in e.Args)
            {
                if (arg.Equals("/minimized", System.StringComparison.OrdinalIgnoreCase))
                {
                    StartMinimized = true;
                    break;
                }
            }
            base.OnStartup(e);
        }
    }
}
