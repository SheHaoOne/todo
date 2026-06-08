using System.Windows;
using System.Windows.Controls;
using TodoApp.ViewModels;

namespace TodoApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SmartList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox { SelectedItem: ListItemViewModel list }
            && DataContext is MainViewModel vm)
        {
            vm.SelectListCommand.Execute(list);
        }
    }

    private void TaskList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox { SelectedItem: TaskItemViewModel task }
            && DataContext is MainViewModel vm)
        {
            vm.SelectTaskCommand.Execute(task);
        }
    }
}
