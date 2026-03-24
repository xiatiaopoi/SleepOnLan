using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using SleepOnLan.Models;
using SleepOnLan.Services;

namespace SleepOnLan
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;

        public SettingsWindow(SettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            LoadSettings();
        }

        private void LoadSettings()
        {
            TxtPort.Text = _settingsService.Settings.DefaultPort.ToString();
            LstApps.ItemsSource = _settingsService.Settings.RegisteredApps;
        }

        private void BtnBrowseApp_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*",
                Title = "选择应用程序"
            };

            if (dialog.ShowDialog() == true)
            {
                TxtAppPath.Text = dialog.FileName;
                if (string.IsNullOrEmpty(TxtAppName.Text))
                {
                    TxtAppName.Text = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
        }

        private void BtnAddApp_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtAppName.Text.Trim();
            string path = TxtAppPath.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                System.Windows.MessageBox.Show("请输入应用名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                System.Windows.MessageBox.Show("请选择有效的应用路径", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _settingsService.AddApp(name, path);
            LstApps.ItemsSource = null;
            LstApps.ItemsSource = _settingsService.Settings.RegisteredApps;

            TxtAppName.Clear();
            TxtAppPath.Clear();
        }

        private void BtnRemoveApp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string appName)
            {
                _settingsService.RemoveApp(appName);
                LstApps.ItemsSource = null;
                LstApps.ItemsSource = _settingsService.Settings.RegisteredApps;
            }
        }

        private async void BtnTestStatus_Click(object sender, RoutedEventArgs e)
        {
            await TestCommandAsync("status");
        }

        private async void BtnTestScreenshot_Click(object sender, RoutedEventArgs e)
        {
            await TestCommandAsync("screenshot");
        }

        private async void BtnTestSleep_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
                "确定要测试睡眠功能吗？电脑将进入睡眠状态。",
                "确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await TestCommandAsync("sleep");
            }
        }

        private async void BtnTestShutdown_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
                "确定要测试关机功能吗？电脑将立即关机！",
                "警告",
                MessageBoxButton.YesNo,
                MessageBoxImage.Stop);

            if (result == MessageBoxResult.Yes)
            {
                await TestCommandAsync("shutdown");
            }
        }

        private async System.Threading.Tasks.Task TestCommandAsync(string command)
        {
            TxtTestResult.Text = $"正在发送指令: {command}\n";

            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync("127.0.0.1", _settingsService.Settings.DefaultPort);

                var stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(command);
                await stream.WriteAsync(data, 0, data.Length);

                byte[] lengthBytes = new byte[4];
                int readLen = await stream.ReadAsync(lengthBytes, 0, 4);
                if (readLen < 4)
                {
                    TxtTestResult.Text += "错误: 无法读取响应长度\n";
                    return;
                }
                
                int totalLength = BitConverter.ToInt32(lengthBytes, 0);
                
                byte[] buffer = new byte[totalLength];
                int totalRead = 0;
                while (totalRead < totalLength)
                {
                    int read = await stream.ReadAsync(buffer, totalRead, totalLength - totalRead);
                    if (read == 0) break;
                    totalRead += read;
                }
                
                string response = Encoding.UTF8.GetString(buffer, 0, totalRead);

                if (response.StartsWith("SCREENSHOT:"))
                {
                    string base64 = response.Substring(11);
                    TxtTestResult.Text = $"截图成功，Base64 长度: {base64.Length} 字符\n";
                    TxtTestResult.Text += $"完整 Base64:\n{base64}\n";
                    TxtTestResult.Text += "测试完成";
                }
                else
                {
                    TxtTestResult.Text += $"响应: {response}\n";
                    TxtTestResult.Text += "测试完成";
                }
            }
            catch (Exception ex)
            {
                TxtTestResult.Text += $"错误: {ex.Message}\n";
                TxtTestResult.Text += "请确保服务器正在运行";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtPort.Text, out int port) && port >= 1024 && port <= 65535)
            {
                _settingsService.UpdatePort(port);
                DialogResult = true;
                Close();
            }
            else
            {
                System.Windows.MessageBox.Show("请输入有效的端口号 (1024-65535)", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnCopyResult_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtTestResult.Text))
            {
                System.Windows.Clipboard.SetText(TxtTestResult.Text);
                System.Windows.MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
