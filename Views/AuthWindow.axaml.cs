using Avalonia.Controls;
using AvaloniaTestTables.ViewModels;

namespace AvaloniaTestTables.Views;

public partial class AuthWindow : Window
{
    public AuthWindow()
    {
        InitializeComponent();
        if (DataContext is AuthWindowViewModel viewModel)
        {
            viewModel.CloseCurrentWindow = Close;
        }
    }
}