using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SleepOnLan.Services
{
    public class ServerService
    {
        private TcpListener? _server;
        private CancellationTokenSource? _cts;
        private bool _isRunning = false;

        public bool IsRunning => _isRunning;
        
        public event Action<string>? LogReceived;
        public event Action<string, TcpClient>? CommandReceived;
        public event Action<bool, int>? StatusChanged;

        public async Task StartAsync(int port)
        {
            _cts = new CancellationTokenSource();
            _server = new TcpListener(IPAddress.Any, port);
            
            _server.Start();
            _isRunning = true;
            
            StatusChanged?.Invoke(true, port);
            LogReceived?.Invoke($"服务器已在端口 {port} 启动...");

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    TcpClient client = await _server.AcceptTcpClientAsync(_cts.Token);
                    _ = HandleClientAsync(client);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (SocketException) { }
            finally
            {
                if (_isRunning)
                {
                    _isRunning = false;
                    StatusChanged?.Invoke(false, 0);
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _cts?.Cancel();
            _server?.Stop();
            StatusChanged?.Invoke(false, 0);
            LogReceived?.Invoke("服务器已停止。");
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            {
                var stream = client.GetStream();
                byte[] buffer = new byte[4096];
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                string cmd = Encoding.UTF8.GetString(buffer, 0, read).Trim();

                CommandReceived?.Invoke(cmd, client);
            }
        }

        public static async Task SendResponseAsync(TcpClient client, string response)
        {
            var stream = client.GetStream();
            byte[] respBytes = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(respBytes, 0, respBytes.Length);
        }
    }
}
