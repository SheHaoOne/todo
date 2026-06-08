using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TodoApp.Helpers;

public static class ListBoxDragReorder
{
    private static readonly Dictionary<ListBox, DragState> States = new();

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ListBoxDragReorder),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(ListBoxDragReorder),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DragHandleProperty =
        DependencyProperty.RegisterAttached(
            "DragHandle",
            typeof(bool),
            typeof(ListBoxDragReorder),
            new PropertyMetadata(false));

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) =>
        (bool)element.GetValue(IsEnabledProperty);

    public static void SetCommand(DependencyObject element, ICommand? value) =>
        element.SetValue(CommandProperty, value);

    public static ICommand? GetCommand(DependencyObject element) =>
        (ICommand?)element.GetValue(CommandProperty);

    public static void SetDragHandle(DependencyObject element, bool value) =>
        element.SetValue(DragHandleProperty, value);

    public static bool GetDragHandle(DependencyObject element) =>
        (bool)element.GetValue(DragHandleProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox) return;

        if ((bool)e.NewValue)
        {
            listBox.AllowDrop = true;
            listBox.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            listBox.PreviewMouseMove += OnPreviewMouseMove;
            listBox.DragOver += OnDragOver;
            listBox.Drop += OnDrop;
            States[listBox] = new DragState();
        }
        else
        {
            listBox.AllowDrop = false;
            listBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            listBox.PreviewMouseMove -= OnPreviewMouseMove;
            listBox.DragOver -= OnDragOver;
            listBox.Drop -= OnDrop;
            States.Remove(listBox);
        }
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox listBox || !States.TryGetValue(listBox, out var state)) return;
        if (e.OriginalSource is not DependencyObject source) return;
        if (IsInteractiveControl(source)) return;
        if (!IsDragHandleClick(source)) return;

        state.StartPoint = e.GetPosition(null);
        state.DraggedIndex = GetItemIndex(listBox, source);
    }

    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not ListBox listBox || !States.TryGetValue(listBox, out var state)) return;
        if (e.LeftButton != MouseButtonState.Pressed || state.DraggedIndex < 0) return;

        var current = e.GetPosition(null);
        if (Math.Abs(current.X - state.StartPoint.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(current.Y - state.StartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        if (listBox.Items.Count == 0) return;

        var draggedItem = listBox.Items[state.DraggedIndex];
        DragDrop.DoDragDrop(listBox, draggedItem, DragDropEffects.Move);
        state.DraggedIndex = -1;
    }

    private static void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not ListBox listBox || !States.TryGetValue(listBox, out var state)) return;

        var droppedItem = FindDroppedItem(listBox, e);
        if (droppedItem == null) return;

        var oldIndex = listBox.Items.IndexOf(droppedItem);
        if (oldIndex < 0) return;

        var targetIndex = GetTargetIndex(listBox, e.GetPosition(listBox));
        if (oldIndex == targetIndex) return;

        var command = GetCommand(listBox);
        if (command?.CanExecute(new ReorderInfo(oldIndex, targetIndex)) == true)
            command.Execute(new ReorderInfo(oldIndex, targetIndex));

        state.DraggedIndex = -1;
        e.Handled = true;
    }

    private static object? FindDroppedItem(ListBox listBox, DragEventArgs e)
    {
        foreach (var item in listBox.Items)
        {
            if (e.Data.GetDataPresent(item.GetType()) && ReferenceEquals(e.Data.GetData(item.GetType()), item))
                return item;
        }

        foreach (var format in e.Data.GetFormats())
        {
            var data = e.Data.GetData(format);
            if (data != null && listBox.Items.Contains(data))
                return data;
        }

        return null;
    }

    private static int GetTargetIndex(ListBox listBox, Point position)
    {
        for (var i = 0; i < listBox.Items.Count; i++)
        {
            if (listBox.ItemContainerGenerator.ContainerFromIndex(i) is not ListBoxItem container)
                continue;

            var top = container.TranslatePoint(new Point(0, 0), listBox).Y;
            var mid = top + container.ActualHeight / 2;
            if (position.Y < mid)
                return i;
        }

        return listBox.Items.Count - 1;
    }

    private static int GetItemIndex(ListBox listBox, DependencyObject source)
    {
        while (source is not null)
        {
            if (source is ListBoxItem item)
                return listBox.ItemContainerGenerator.IndexFromContainer(item);

            source = VisualTreeHelper.GetParent(source);
        }

        return -1;
    }

    private static bool IsDragHandleClick(DependencyObject source)
    {
        while (source is not null)
        {
            if (source is FrameworkElement element && GetDragHandle(element))
                return true;

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private static bool IsInteractiveControl(DependencyObject source)
    {
        while (source is not null)
        {
            if (source is TextBoxBase or ComboBox or DatePicker)
                return true;

            if (source is Button button)
            {
                if (GetDragHandle(button))
                    return false;

                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private sealed class DragState
    {
        public Point StartPoint;
        public int DraggedIndex = -1;
    }
}

public readonly record struct ReorderInfo(int OldIndex, int NewIndex);
