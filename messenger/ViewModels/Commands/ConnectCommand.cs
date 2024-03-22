using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace messenger.ViewModels.Commands;

public class ConnectCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    private MainViewModel _mainViewModel;

    public ConnectCommand(MainViewModel viewModel)
    {
        _mainViewModel = viewModel;

    }

    public bool CanExecute(object? parameters)
    {
        return true;
    }

    public void Execute(object? parameters)
    {
        _mainViewModel.Connect();
    }
}
