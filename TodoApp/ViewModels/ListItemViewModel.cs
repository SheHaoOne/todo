using TodoApp.Models;

namespace TodoApp.ViewModels;

public class ListItemViewModel : ViewModelBase
{
    private bool _isSelected;
    private string _name;

    public ListItemViewModel(TodoList list)
    {
        Model = list;
        _name = list.Name;
    }

    public TodoList Model { get; }

    public Guid Id => Model.Id;
    public ListType Type => Model.Type;
    public string IconGlyph => Model.IconGlyph;
    public string Color => Model.Color;
    public bool IsSmartList => Model.IsSmartList;

    public string Name
    {
        get => _name;
        set
        {
            if (!SetProperty(ref _name, value)) return;
            Model.Name = value;
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private int _taskCount;

    public int TaskCount
    {
        get => _taskCount;
        set
        {
            if (SetProperty(ref _taskCount, value)) return;
            OnPropertyChanged(nameof(TaskCount));
        }
    }
}
