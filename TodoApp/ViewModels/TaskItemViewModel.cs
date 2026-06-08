using TodoApp.Models;

namespace TodoApp.ViewModels;

public class TaskItemViewModel : ViewModelBase
{
    private readonly Action<TaskItemViewModel>? _onChanged;
    private string _title = string.Empty;
    private bool _isCompleted;
    private bool _isImportant;
    private bool _isMyDay;
    private DateTime? _dueDate;
    private DateTime? _reminderDate;
    private string _notes = string.Empty;
    private bool _isSelected;

    public TaskItemViewModel(TodoTask task, Action<TaskItemViewModel>? onChanged = null)
    {
        Model = task;
        _onChanged = onChanged;
        _title = task.Title;
        _isCompleted = task.IsCompleted;
        _isImportant = task.IsImportant;
        _isMyDay = task.IsMyDay;
        _dueDate = task.DueDate;
        _reminderDate = task.ReminderDate;
        _notes = task.Notes;
    }

    public TodoTask Model { get; }

    public Guid Id => Model.Id;
    public Guid ListId => Model.ListId;

    public string Title
    {
        get => _title;
        set
        {
            if (!SetProperty(ref _title, value)) return;
            Model.Title = value;
            NotifyChanged();
        }
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (!SetProperty(ref _isCompleted, value)) return;
            Model.IsCompleted = value;
            Model.CompletedAt = value ? DateTime.Now : null;
            OnPropertyChanged(nameof(DueDateDisplay));
            OnPropertyChanged(nameof(IsOverdue));
            OnPropertyChanged(nameof(IsDueToday));
            NotifyChanged();
        }
    }

    public bool IsImportant
    {
        get => _isImportant;
        set
        {
            if (!SetProperty(ref _isImportant, value)) return;
            Model.IsImportant = value;
            NotifyChanged();
        }
    }

    public bool IsMyDay
    {
        get => _isMyDay;
        set
        {
            if (!SetProperty(ref _isMyDay, value)) return;
            Model.IsMyDay = value;
            NotifyChanged();
        }
    }

    public DateTime? DueDate
    {
        get => _dueDate;
        set
        {
            if (!SetProperty(ref _dueDate, value)) return;
            Model.DueDate = value;
            OnPropertyChanged(nameof(DueDateDisplay));
            OnPropertyChanged(nameof(HasDueDate));
            OnPropertyChanged(nameof(IsOverdue));
            OnPropertyChanged(nameof(IsDueToday));
            NotifyChanged();
        }
    }

    public DateTime? ReminderDate
    {
        get => _reminderDate;
        set
        {
            if (!SetProperty(ref _reminderDate, value)) return;
            Model.ReminderDate = value;
            NotifyChanged();
        }
    }

    public string Notes
    {
        get => _notes;
        set
        {
            if (!SetProperty(ref _notes, value)) return;
            Model.Notes = value;
            NotifyChanged();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool HasDueDate => DueDate.HasValue;
    public bool IsOverdue => DueDate.HasValue && DueDate.Value.Date < DateTime.Today && !IsCompleted;
    public bool IsDueToday => DueDate.HasValue && DueDate.Value.Date == DateTime.Today;

    public string DueDateDisplay => DueDate switch
    {
        null => string.Empty,
        var d when d.Date == DateTime.Today => "今天",
        var d when d.Date == DateTime.Today.AddDays(1) => "明天",
        var d when d.Date == DateTime.Today.AddDays(-1) => "昨天",
        var d when d.Date < DateTime.Today => d.ToString("M月d日"),
        _ => DueDate!.Value.ToString("M月d日")
    };

    public IList<TodoStep> Steps => Model.Steps;

    public int CompletedStepCount => Model.CompletedStepCount;
    public int TotalStepCount => Model.TotalStepCount;
    public bool HasSteps => Model.HasSteps;

    public string StepProgress => HasSteps ? $"{CompletedStepCount}/{TotalStepCount}" : string.Empty;

    public void RefreshSteps()
    {
        OnPropertyChanged(nameof(CompletedStepCount));
        OnPropertyChanged(nameof(TotalStepCount));
        OnPropertyChanged(nameof(HasSteps));
        OnPropertyChanged(nameof(StepProgress));
        NotifyChanged();
    }

    private void NotifyChanged() => _onChanged?.Invoke(this);
}
