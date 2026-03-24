using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SleepOnLan.Services;

namespace SleepOnLan
{
    public partial class MainWindow : Window
    {
        private readonly ServerService _serverService;
        private readonly CommandHandler _commandHandler;
        private readonly AutoStartManager _autoStartManager;
        private readonly SettingsService _settingsService;

        public MainWindow()
        {
            InitializeComponent();
            
            _settingsService = new SettingsService();
            _serverService = new ServerService();
            _commandHandler = new CommandHandler(_settingsService);
            _autoStartManager = new AutoStartManager();

            SetupEventHandlers();
            LoadSettings();
            Loaded += MainWindow_Loaded;
        }

        private void SetupEventHandlers()
        {
            _serverService.LogReceived += msg => Log(msg);
            _serverService.StatusChanged += (running, port) => UpdateStatus(running, port);
            _serverService.CommandReceived += HandleCommandAsync;
        }

        private void LoadSettings()
        {
            TxtPort.Text = _settingsService.Settings.DefaultPort.ToString();
            ChkAutoStart.IsChecked = _autoStartManager.IsEnabled;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.StartMinimized)
            {
                Hide();
            }
            await AutoStartServer();
        }

        private async Task AutoStartServer()
        {
            int port = GetPort();
            try
            {
                await _serverService.StartAsync(port);
            }
            catch (SocketException)
            {
                Log($"自动启动失败: 端口 {port} 已被占用");
                UpdateStatus(false);
            }
        }

        private async void BtnSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (!_serverService.IsRunning)
            {
                int port = GetPort();
                if (port == -1)
                {
                    Log("错误: 请输入有效的端口号 (1024-65535)");
                    return;
                }

                try
                {
                    await _serverService.StartAsync(port);
                }
                catch (SocketException)
                {
                    Log($"启动失败: 端口 {port} 已被占用");
                    UpdateStatus(false);
                }
            }
            else
            {
                _serverService.Stop();
            }
        }

        private async Task HandleCommandAsync(string cmd, TcpClient client)
        {
            Log($"收到指令: {cmd}");
            
            var result = await _commandHandler.HandleAsync(cmd);
            
            if (!string.IsNullOrEmpty(result.LogMessage))
            {
                Log(result.LogMessage);
            }

            await ServerService.SendResponseAsync(client, result.Response);
        }

        private int GetPort()
        {
            if (!int.TryParse(TxtPort.Text, out int port) || port < 1024 || port > 65535)
            {
                port = _settingsService.Settings.DefaultPort;
                TxtPort.Text = port.ToString();
            }
            return port;
        }

        private void UpdateStatus(bool running, int port = 0)
        {
            Dispatcher.Invoke(() =>
            {
                if (running)
                {
                    StatusIndicator.Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4CAF50"));
                    TxtStatus.Text = $"运行中 (端口 {port})";
                    TxtPort.IsEnabled = false;
                    BtnSwitch.Content = "停止服务器";
                    BtnSwitch.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#f44336"));
                }
                else
                {
                    StatusIndicator.Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9E9E9E"));
                    TxtStatus.Text = "未运行";
                    TxtPort.IsEnabled = true;
                    BtnSwitch.Content = "启动服务器";
                    BtnSwitch.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4CAF50"));
                }
            });
        }

        private void Log(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
                TxtLog.ScrollToEnd();
            });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void MenuShow_Click(object sender, RoutedEventArgs e) => Show();
        private void MenuExit_Click(object sender, RoutedEventArgs e) => System.Windows.Application.Current.Shutdown();
        private void BtnClearLog_Click(object sender, RoutedEventArgs e) => TxtLog.Clear();

        private void ChkAutoStart_Click(object sender, RoutedEventArgs e)
        {
            var (success, message) = ChkAutoStart.IsChecked == true 
                ? _autoStartManager.Enable() 
                : _autoStartManager.Disable();
            
            Log(message);
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_settingsService)
            {
                Owner = this
            };

            if (settingsWindow.ShowDialog() == true)
            {
                TxtPort.Text = _settingsService.Settings.DefaultPort.ToString();
                _commandHandler.ReloadApps();
                Log("设置已保存");
            }
        }
    }
}
