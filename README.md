# SleepOnLan

一个用于远程控制电脑的 WPF 桌面应用程序。可以通过网络指令让电脑睡眠、关机、截图或启动应用程序。

本项目最初只是一个简单的远程关机工具，后来扩展了更多实用功能。

## 功能

- 远程睡眠控制
- 远程关机控制
- 远程截图（支持多屏幕）
- 远程启动自定义应用程序
- 开机自启动并最小化到系统托盘
- 可视化设置界面

## 安装

需要安装 .NET 10 SDK。

```
git clone https://github.com/xiatiaopoi/SleepOnLan.git
cd SleepOnLan
dotnet build
```

## 使用

启动程序：

```
dotnet run
```

程序启动后会自动启动 TCP 服务器，默认监听端口 9999。

## TCP 指令

| 指令 | 说明 |
|------|------|
| `status` | 查询服务器状态 |
| `sleep` | 让电脑进入睡眠 |
| `shutdown` | 关闭电脑 |
| `app:名称` | 启动已注册的应用 |
| `screenshot` | 截取屏幕1 |
| `screenshot:N` | 截取指定屏幕（N为屏幕编号） |

## Python 示例

基本连接：

```python
import socket
import struct

def send_command(host, port, command):
    s = socket.socket()
    s.connect((host, port))
    s.send(command.encode())
    
    length_bytes = s.recv(4)
    length = struct.unpack('<I', length_bytes)[0]
    
    data = b''
    while len(data) < length:
        data += s.recv(min(65536, length - len(data)))
    
    s.close()
    return data.decode()

response = send_command('192.168.1.100', 9999, 'status')
print(response)
```

截图处理：

```python
import socket
import struct
import base64
from PIL import Image
import io

def send_command(host, port, command):
    s = socket.socket()
    s.connect((host, port))
    s.send(command.encode())
    
    length_bytes = s.recv(4)
    length = struct.unpack('<I', length_bytes)[0]
    
    data = b''
    while len(data) < length:
        data += s.recv(min(65536, length - len(data)))
    
    s.close()
    return data.decode()

response = send_command('192.168.1.100', 9999, 'screenshot')

if response.startswith('SCREENSHOT:'):
    base64_data = response[11:]
    image_data = base64.b64decode(base64_data)
    image = Image.open(io.BytesIO(image_data))
    image.save('screenshot.png')
    print('截图已保存')
```

## 设置

点击主界面的「设置」按钮可以：

- 修改默认端口
- 添加或删除可远程启动的应用程序
- 测试指令功能

## 项目结构

```
SleepOnLan/
├── Models/              # 数据模型
├── Services/            # 服务层
├── MainWindow.xaml      # 主窗口
├── SettingsWindow.xaml  # 设置窗口
└── README.md
```

## 技术栈

- .NET 10
- WPF
- Hardcodet.NotifyIcon.Wpf

## 许可证

MIT License
