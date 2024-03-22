using messenger.ViewModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace messenger;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    public MainWindow(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        InitializeComponent();
        DataContext = mainViewModel;
    }

    private void Border_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void SettingsClick(object sender, RoutedEventArgs e)
    {
        if (SettingsPanel.Visibility == Visibility.Collapsed)
        {
            SettingsPanel.Visibility = Visibility.Visible;
            SendButton.Visibility = Visibility.Collapsed;
            ChatHistory.Visibility = Visibility.Collapsed;
            Conversation.Visibility = Visibility.Collapsed;
        } else
        {
            SettingsPanel.Visibility = Visibility.Collapsed;
            SendButton.Visibility = Visibility.Visible;
            ChatHistory.Visibility = Visibility.Visible;
            Conversation.Visibility = Visibility.Visible;
        }
    }

    private void Shutdown(object sender, RoutedEventArgs e)
    {
        _mainViewModel.Shutdown();
        Application.Current.Shutdown();
    }
}