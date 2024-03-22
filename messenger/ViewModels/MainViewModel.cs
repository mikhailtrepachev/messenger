using messenger.Models;
using messenger.ViewModels.Commands;
using messenger.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Net;
using System.Collections.ObjectModel;

namespace messenger.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private ConnectionService _connectionService;

    public String IpAddress { get; set; } = "Zadejte IP...";
    public String Port { get; set; } = "Zadejte Port...";

    private ICommand _connectClick;
    private ICommand _receiveClick;
    private ICommand _sendMessageClick;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<MessageModel>? Messages { get; set; } = new ObservableCollection<MessageModel>();

    private string _connectionStatus;

    private String _messageText;

    public string ConnectionStatus
    {
        get
        {
            return _connectionStatus;
        }
        set
        {
            _connectionStatus = value;
            OnPropertyChanged(nameof(ConnectionStatus));
        }
    }

    public String MessageText
    {
        get
        {
            return _messageText;
        }
        set
        {
            _messageText = value;
            OnPropertyChanged(nameof(MessageText));
        }
    }

    public MainViewModel(ConnectionService connectionHandler)
    {
        _connectionService = connectionHandler;

        _connectionService.MessageSent += OnMessageSent;
        _connectionService.MessageReceived += OnMessageReceived;
        _connectionService.ConnectionStatus += OnConnectionStatusChange;

        this.ConnectionStatus = "Not Connected!";

        this.ConnectClick = new ConnectCommand(this);
        this.ReceiveClick = new ReceiveCommand(this);
        this.SendMessageClick = new SendMessageCommand(this);
    }

    public ICommand ConnectClick { get { return _connectClick; } set { _connectClick = value; } }

    public ICommand ReceiveClick { get { return _receiveClick; } set { _receiveClick = value; } }

    public ICommand SendMessageClick { get { return _sendMessageClick; } set { _sendMessageClick = value; } }

    public void Connect()
    {
        ConnectionStatus = "Connecting...";

        try
        {
            int.Parse(Port);
            IPAddress.Parse(IpAddress);
            _connectionService.ConnectToReceiver(IpAddress, Port);
        }
        catch
        {
            ConnectionStatus = "Nesprávně nastavené hodnoty připojení";
        }
    }

    public void Receive()
    {
        ConnectionStatus = "Connecting...";

        try
        {
            int.Parse(Port);
            _connectionService.StartReceive(Port);
        }
        catch
        {
            ConnectionStatus = "Nesprávně nastavené hodnoty připojení";
        }
    }

    public void SendMessage()
    {
        _connectionService.SendMessage(MessageText);
        MessageText = string.Empty;
    }

    public void Shutdown() => _connectionService.Shutdown();

    private void OnMessageSent(object sender, MessageModel message)
    {
        Messages.Add(message);
    }

    private void OnMessageReceived(object sender, MessageModel message)
    {
        Messages.Add(message);
    }

    private void OnConnectionStatusChange(object sender, string status) => ConnectionStatus = status;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
