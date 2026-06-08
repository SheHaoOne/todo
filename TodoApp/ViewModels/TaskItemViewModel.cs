using TodoApp.Models;

namespace TodoApp.ViewModels;

public class TaskItemViewModel : ViewModelBase
{
    private readonly Action<TaskItemViewModel>? _onSaveRequested;
    private readonly Action<TaskItemViewModel>? _onMetadataChanged;
    private string _title = string.Empty;
    private bool _isCompleted;
    private bool _isImportant;
    private bool _isMyDay;
    private DateTime? _dueDate;
    private DateTime? _reminderDate;
    private string _notes = string.Empty;
    private bool _isSelected;

    public TaskItemViewModel(TodoTask task,
        Action<TaskItemViewModel>? onSaveRequested = null,
        Action<TaskItemViewModel>? onMetadataChanged = null)
    {
        Model = task;
        _onSaveRequested = onSaveRequested;
        _onMetadataChanged = onMetadataChanged;
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
            NotifyMetadataChanged();
        }
    }

    public bool IsImportant
    {
        get => _isImportant;
        set
        {
            if (!SetProperty(ref _isImportant, value)) return;
            Model.IsImportant = value;
            NotifyMetadataChanged();
        }
    }

    public bool IsMyDay
    {
        get => _isMyDay;
        set
        {
            if (!SetProperty(ref _isMyDay, value)) return;
            Model.IsMyDay = value;
            NotifyMetadataChanged();
        }
    }

    public DateTime? DueDate
    {
        get => _dueDate;
        set
        {
            var normalized = value?.Date;
            if (!SetProperty(ref _dueDate, normalized)) return;
            Model.DueDate = normalized;
            OnPropertyChanged(nameof(DueDateDisplay));
            OnPropertyChanged(nameof(HasDueDate));
            OnPropertyChanged(nameof(IsOverdue));
            OnPropertyChanged(nameof(IsDueToday));
            NotifyMetadataChanged();
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

    public string DueDateDisplay
    {
        get
        {
            if (DueDate is not DateTime date)
                return string.Empty;

            return date.Date switch
            {
                var d when d == DateTime.Today => "今天",
                var d when d == DateTime.Today.AddDays(1) => "明天",
                var d when d == DateTime.Today.AddDays(-1) => "昨天",
                var d when d < DateTime.Today => date.ToString("M月d日"),
                _ => date.ToString("M月d日")
            };
        }
    }

    public IList<TodoStep> Steps => Model.Steps;

    public int CompletedStepCount => Model.CompletedStepCount;
    public int TotalStepCount => Model.TotalStepCount;
    public bool HasSteps => Model.HasSteps;

    public string StepProgress => HasSteps ? $"{CompletedStepCount}/{TotalStepCount}" : string.Empty;

    public void RefreshSteps()
    {
        OnPropertyChanged(nameof(Steps));
        OnPropertyChanged(nameof(CompletedStepCount));
        OnPropertyChanged(nameof(TotalStepCount));
        OnPropertyChanged(nameof(HasSteps));
        OnPropertyChanged(nameof(StepProgress));
    }

    private void NotifyChanged() => _onSaveRequested?.Invoke(this);

    private void NotifyMetadataChanged() => _onMetadataChanged?.Invoke(this);
}
