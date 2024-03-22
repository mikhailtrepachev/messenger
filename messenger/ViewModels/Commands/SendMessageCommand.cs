using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace messenger.ViewModels.Commands;

public class SendMessageCommand : ICommand
{
    private MainViewModel _mainViewModel;

    public SendMessageCommand(MainViewModel viewModel)
    {
        _mainViewModel = viewModel;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        _mainViewModel.SendMessage();
    }
}
