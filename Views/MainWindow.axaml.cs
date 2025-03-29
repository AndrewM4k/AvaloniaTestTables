using Avalonia.Controls;
using AvaloniaTestTables.Context;
using AvaloniaTestTables.ViewModels;

namespace AvaloniaTestTables.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(new AppDbContext());
    }
}