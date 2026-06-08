using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TodoApp.Models;
using TodoApp.ViewModels;

namespace TodoApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadWindowIcon();
    }

    private void LoadWindowIcon()
    {
        try
        {
            Icon = BitmapFrame.Create(
                new Uri("pack://application:,,,/Resources/app.ico", UriKind.Absolute));
        }
        catch
        {
            // 图标加载失败时不影响应用启动
        }
    }

    private void TaskList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox || DataContext is not MainViewModel vm)
            return;

        if (listBox.SelectedItem is TaskItemViewModel task)
            vm.SelectTaskCommand.Execute(task);
    }

    private void ActionButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        ExecuteButtonCommand(sender);
    }

    private void DeleteListButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        ExecuteButtonCommand(sender);
    }

    private void RenameListTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not TextBox textBox || !textBox.IsVisible)
            return;

        textBox.Focus();
        textBox.SelectAll();
    }

    private void RenameListTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox { DataContext: ListItemViewModel list }
            && DataContext is MainViewModel vm)
        {
            vm.CommitRenameListCommand.Execute(list);
        }
    }

    private void RenameListTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox || DataContext is not MainViewModel vm)
            return;

        if (e.Key == Key.Enter)
        {
            vm.CommitRenameListCommand.Execute(textBox.DataContext);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && textBox.DataContext is ListItemViewModel list)
        {
            list.CancelRenameMode();
            e.Handled = true;
        }
    }

    private void MainContent_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainViewModel vm || !vm.IsDetailPanelOpen)
            return;

        if (e.OriginalSource is not DependencyObject source)
            return;

        if (IsDescendantOf<ListBoxItem>(source)
            || IsDescendantOf<TextBox>(source)
            || IsDescendantOf<Button>(source)
            || IsDescendantOf<DatePicker>(source))
            return;

        CloseDetailPanel();
    }

    private void StepTitleTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox { DataContext: TodoStep step } || DataContext is not MainViewModel vm)
            return;

        vm.CommitStepEditCommand.Execute(step);
    }

    private void CloseDetailButton_Click(object sender, RoutedEventArgs e)
    {
        CloseDetailPanel();
    }

    private void CloseDetailPanel()
    {
        ActiveTaskListBox.SelectedItem = null;

        if (DataContext is MainViewModel vm)
            vm.CloseDetailCommand.Execute(null);
    }

    private static void ExecuteButtonCommand(object sender)
    {
        if (sender is not Button { Command: { } command } button)
            return;

        var parameter = button.CommandParameter;
        if (command.CanExecute(parameter))
            command.Execute(parameter);
    }

    private static bool IsDescendantOf<T>(DependencyObject? source) where T : DependencyObject
    {
        while (source is not null)
        {
            if (source is T)
                return true;

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }
}
