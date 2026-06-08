using TodoApp.Models;

namespace TodoApp.ViewModels;

public class ListItemViewModel : ViewModelBase
{
    private readonly Action<ListItemViewModel>? _onNameChanged;
    private bool _isSelected;
    private bool _isRenaming;
    private string _name;
    private string _nameBeforeRename = string.Empty;

    public ListItemViewModel(TodoList list, Action<ListItemViewModel>? onNameChanged = null)
    {
        Model = list;
        _onNameChanged = onNameChanged;
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
            _onNameChanged?.Invoke(this);
        }
    }

    public bool IsRenaming
    {
        get => _isRenaming;
        set => SetProperty(ref _isRenaming, value);
    }

    public void EnterRenameMode()
    {
        _nameBeforeRename = Name;
        IsRenaming = true;
    }

    public void ExitRenameMode()
    {
        IsRenaming = false;
        var trimmed = Name.Trim();
        Name = string.IsNullOrWhiteSpace(trimmed) ? _nameBeforeRename : trimmed;
    }

    public void CancelRenameMode()
    {
        IsRenaming = false;
        Name = _nameBeforeRename;
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
