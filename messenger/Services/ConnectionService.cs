using messenger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace messenger.Services;

public class ConnectionService
{
    TcpClient tcpClient;
    TcpListener tcpListener;
    NetworkStream stream;

    string parsedJsonObject;
    bool isAlive = true;

    public event EventHandler<MessageModel>? MessageSent;

    public event EventHandler<MessageModel>? MessageReceived;

    public event EventHandler<string>? ConnectionStatus;

    public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();

    public async void ConnectToReceiver(string ip, string port)
    {
        MessageModel message = new()
        {
            MessageType = "connection",
            Time= DateTime.Now,
            Message = "",
            Sender = "ping"
        };

        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));

        using (TcpClient tcpClient = new TcpClient())
        {
            try
            {
                await tcpClient.ConnectAsync(ipEndPoint);
                stream = tcpClient.GetStream();

                var messageJson = JsonSerializer.Serialize<MessageModel>(message);
                var bytesMessageJson = Encoding.UTF8.GetBytes(messageJson);

                await stream.WriteAsync(bytesMessageJson);

                var isConnectionAlive = true;

                ConnectionStatus?.Invoke(this, "Connected!");

                while (isConnectionAlive)
                {
                    var streamBuffer = new byte[1_024];
                    int received = await stream.ReadAsync(streamBuffer);
                    var receivedResponseJson = Encoding.UTF8.GetString(streamBuffer, 0, received);

                    int jsonObjectsCount = receivedResponseJson.Count(c => c == '{');

                    if (jsonObjectsCount > 1) 
                    {
                        parsedJsonObject = ParseJsonObjects(receivedResponseJson);
                    }

                    if (received > 0)
                    {
                        if (jsonObjectsCount > 1)
                        {
                            var receivedMessage = JsonSerializer.Deserialize<MessageModel>(parsedJsonObject);

                            receivedMessage.Sender = "chatterer";

                            Messages.Add(receivedMessage);

                            MessageReceived?.Invoke(this, receivedMessage);
                        } else
                        {
                            var receivedMessage = JsonSerializer.Deserialize<MessageModel>(receivedResponseJson);

                            if (receivedMessage.MessageType == "message")
                            {
                                receivedMessage.Sender = "chatterer";

                                Messages.Add(receivedMessage);

                                MessageReceived?.Invoke(this, receivedMessage);
                            }
                        }

                        await Task.Run(() => CheckSession());
                    }

                }
            }
            catch
            {
                ConnectionStatus?.Invoke(this, "Ztráta spojení!");
            }
        }
    }

    public async void StartReceive(string port)
    {
        var ipEndPoint = new IPEndPoint(IPAddress.Any, int.Parse(port));
        
        using (TcpListener tcpListener = new TcpListener(ipEndPoint))
        {
            try
            {
                while (true)
                {
                    tcpListener.Start();

                    TcpClient receiver = await tcpListener.AcceptTcpClientAsync();

                    stream = receiver.GetStream();

                    isAlive = true;

                    ConnectionStatus?.Invoke(this, "Connected!");

                    while (isAlive)
                    {
                        var streamBuffer = new byte[1_024];
                        int receivedBytes = await stream.ReadAsync(streamBuffer);
                        var receivedResponseJson = Encoding.UTF8.GetString(streamBuffer, 0, receivedBytes);


                        int jsonObjectsCount = receivedResponseJson.Count(c => c == '{');

                        if (jsonObjectsCount > 1)
                        {
                            parsedJsonObject = ParseJsonObjects(receivedResponseJson);
                        }

                        if (receivedBytes > 0)
                        {
                            if (jsonObjectsCount > 1)
                            {
                                var receivedMessage = JsonSerializer.Deserialize<MessageModel>(parsedJsonObject);

                                receivedMessage.Sender = "chatterer";

                                Messages.Add(receivedMessage);

                                MessageReceived?.Invoke(this, receivedMessage);
                            }
                            else
                            {
                                var receivedMessage = JsonSerializer.Deserialize<MessageModel>(receivedResponseJson);

                                if (receivedMessage.MessageType == "message")
                                {
                                    receivedMessage.Sender = "chatterer";

                                    Messages.Add(receivedMessage);

                                    MessageReceived?.Invoke(this, receivedMessage);
                                }
                            }

                            await Task.Run(() => CheckSession());
                        }
                    }
                }
            }
            catch
            {
                tcpListener.Stop();
                ConnectionStatus?.Invoke(this, "Ztráta spojení!");
            }
        }
    }

    public async void SendMessage(String messageText)
    {
        if (stream == null)
        {
            MessageModel message = new()
            {
                MessageType = "notification",
                Message = "S účastníkem konverzace nebylo navázáno žádné spojení. Přejděte do nastavení a nastavte spojení.",
                Time = DateTime.Now,
                Sender = "MESSENGER"
            };

            Messages.Add(message);
            MessageSent?.Invoke(this, message);

            return;
        }

        try
        {
            MessageModel message = new()
            {
                MessageType = "message",
                Message = messageText,
                Time = DateTime.Now,
                Sender = "me"
            };

            Messages.Add(message);

            MessageSent?.Invoke(this, message);

            var serializedMessage = JsonSerializer.Serialize<MessageModel>(message);
            var bytesMessage = Encoding.UTF8.GetBytes(serializedMessage);
            await stream.WriteAsync(bytesMessage);
            await stream.FlushAsync();
        }
         catch
        {
            MessageModel message = new()
            {
                MessageType = "notification",
                Message = "Vaše zpráva nebyla doručena. Zkontrolujte spojení s účastníkem konverzace a zkuste to znovu.",
                Time = DateTime.Now,
                Sender = "MESSENGER"
            };

            Messages.Add(message);
            MessageSent?.Invoke(this, message);
        }

    }

    public void Shutdown()
    {
        if (stream != null) { stream.Close(); }
    }

    private string ParseJsonObjects(string jsonObjects)
    {
        List<string> separatedJsonObjects = new List<string>();

        int startIndex = 0;
        while (true)
        {
            int openBraceIndex = jsonObjects.IndexOf('{', startIndex);
            if (openBraceIndex == -1)
                break;

            int closeBraceIndex = jsonObjects.IndexOf('}', openBraceIndex);
            if (closeBraceIndex == -1)
                break;

            string jsonObject = jsonObjects.Substring(openBraceIndex, closeBraceIndex - openBraceIndex + 1);
            separatedJsonObjects.Add(jsonObject);

            startIndex = closeBraceIndex + 1;
        }

        foreach (var jsonObject in separatedJsonObjects)
        {
            if (jsonObject.Contains("MessageType\":\"message\""))
            {
                return jsonObject;
            }
        }

        return string.Empty;
    }

    private async Task CheckSession()
    {
        Thread.Sleep(1000);
        var messageToSend = new MessageModel
        {
            Message = "",
            MessageType = "sessionCheck",
            Time = DateTime.Now,
            Sender = "ping"
        };

        var serialized = JsonSerializer.Serialize<MessageModel>(messageToSend);
        var bytes = Encoding.UTF8.GetBytes(serialized);

        await stream.WriteAsync(bytes);
    }
}
