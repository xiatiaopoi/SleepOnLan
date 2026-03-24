using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace SleepOnLan.Services
{
    public class ScreenshotService
    {
        public async Task<(bool Success, string Data, string Message)> CaptureAsync(int screenIndex)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var screens = System.Windows.Forms.Screen.AllScreens;
                    
                    if (screenIndex < 0 || screenIndex >= screens.Length)
                    {
                        return (false, "", $"无效屏幕索引 {screenIndex + 1}，共 {screens.Length} 个屏幕");
                    }

                    var screen = screens[screenIndex];
                    var bounds = screen.Bounds;

                    using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
                    {
                        using (var g = Graphics.FromImage(bitmap))
                        {
                            g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
                        }

                        using (var ms = new MemoryStream())
                        {
                            bitmap.Save(ms, ImageFormat.Png);
                            string base64 = Convert.ToBase64String(ms.ToArray());
                            return (true, base64, $"截图成功: 屏幕 {screenIndex + 1}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return (false, "", $"截图失败: {ex.Message}");
                }
            });
        }

        public int GetScreenCount()
        {
            return System.Windows.Forms.Screen.AllScreens.Length;
        }
    }
}
