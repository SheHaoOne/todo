using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TodoApp.Helpers;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IDataService _dataService;
    private AppData _appData = new();
    private ListItemViewModel? _selectedList;
    private TaskItemViewModel? _selectedTask;
    private string _newTaskTitle = string.Empty;
    private string _newListName = string.Empty;
    private bool _showCompleted;
    private bool _isDetailPanelOpen;
    private bool _isRefreshingTasks;
    private string _newStepTitle = string.Empty;

    public MainViewModel() : this(new JsonDataService()) { }

    public MainViewModel(IDataService dataService)
    {
        _dataService = dataService;

        SmartLists = new ObservableCollection<ListItemViewModel>();
        CustomLists = new ObservableCollection<ListItemViewModel>();
        ActiveTasks = new ObservableCollection<TaskItemViewModel>();
        CompletedTasks = new ObservableCollection<TaskItemViewModel>();

        SelectListCommand = new RelayCommand(p => SelectList(p as ListItemViewModel));
        SelectTaskCommand = new RelayCommand(p => SelectTask(p as TaskItemViewModel));
        AddTaskCommand = new RelayCommand(AddTask, () => !string.IsNullOrWhiteSpace(NewTaskTitle));
        ToggleCompleteCommand = new RelayCommand(p => ToggleComplete(p as TaskItemViewModel));
        ToggleImportantCommand = new RelayCommand(p => ToggleImportant(p as TaskItemViewModel));
        ToggleMyDayCommand = new RelayCommand(p => ToggleMyDay(p as TaskItemViewModel));
        DeleteTaskCommand = new RelayCommand(p => DeleteTask(p as TaskItemViewModel));
        AddListCommand = new RelayCommand(AddList, () => !string.IsNullOrWhiteSpace(NewListName));
        DeleteListCommand = new RelayCommand(p => DeleteList(p as ListItemViewModel), p => p is ListItemViewModel { IsSmartList: false });
        BeginRenameListCommand = new RelayCommand(p => BeginRenameList(p as ListItemViewModel), p => p is ListItemViewModel { IsSmartList: false });
        CommitRenameListCommand = new RelayCommand(p => CommitRenameList(p as ListItemViewModel));
        SetDueDateCommand = new RelayCommand(p => SetDueDate(p));
        ClearDueDateCommand = new RelayCommand(_ => ClearDueDate(), _ => SelectedTask?.DueDate != null);
        CloseDetailCommand = new RelayCommand(_ => CloseDetail());
        AddStepCommand = new RelayCommand(AddStep, () => !string.IsNullOrWhiteSpace(NewStepTitle) && SelectedTask != null);
        ToggleStepCommand = new RelayCommand(p => ToggleStep(p as TodoStep));
        DeleteStepCommand = new RelayCommand(p => DeleteStep(p as TodoStep));
        CommitStepEditCommand = new RelayCommand(p => CommitStepEdit(p as TodoStep));
        ReorderTasksCommand = new RelayCommand(p => ReorderTasks(p), _ => CanReorderTasks);
        ReorderStepsCommand = new RelayCommand(p => ReorderSteps(p), _ => SelectedTask != null);
        ToggleShowCompletedCommand = new RelayCommand(_ => ShowCompleted = !ShowCompleted);

        LoadData();
    }

    public ObservableCollection<ListItemViewModel> SmartLists { get; }
    public ObservableCollection<ListItemViewModel> CustomLists { get; }
    public ObservableCollection<TaskItemViewModel> ActiveTasks { get; }
    public ObservableCollection<TaskItemViewModel> CompletedTasks { get; }

    public ListItemViewModel? SelectedList
    {
        get => _selectedList;
        set
        {
            if (!SetProperty(ref _selectedList, value)) return;

            foreach (var item in SmartLists.Concat(CustomLists))
                item.IsSelected = item == value;

            OnPropertyChanged(nameof(HeaderTitle));
            OnPropertyChanged(nameof(HeaderSubtitle));
            OnPropertyChanged(nameof(CanRenameSelectedList));
            OnPropertyChanged(nameof(SelectedListName));
            OnPropertyChanged(nameof(CanReorderTasks));
            CloseDetail();
            RefreshTasks();
        }
    }

    public TaskItemViewModel? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (ReferenceEquals(_selectedTask, value)) return;
            if (_isRefreshingTasks && value is null) return;

            if (_selectedTask != null)
                _selectedTask.IsSelected = false;

            _selectedTask = value;
            OnPropertyChanged();

            if (_selectedTask != null)
                _selectedTask.IsSelected = true;

            IsDetailPanelOpen = _selectedTask != null;
            OnPropertyChanged(nameof(HasSelectedTask));
        }
    }

    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set
        {
            if (SetProperty(ref _newTaskTitle, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public string NewListName
    {
        get => _newListName;
        set
        {
            if (SetProperty(ref _newListName, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public string NewStepTitle
    {
        get => _newStepTitle;
        set
        {
            if (SetProperty(ref _newStepTitle, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool ShowCompleted
    {
        get => _showCompleted;
        set
        {
            if (!SetProperty(ref _showCompleted, value)) return;
            OnPropertyChanged(nameof(CompletedToggleText));
        }
    }

    public bool IsDetailPanelOpen
    {
        get => _isDetailPanelOpen;
        set => SetProperty(ref _isDetailPanelOpen, value);
    }

    public bool HasSelectedTask => SelectedTask != null;
    public bool HasCompletedTasks => CompletedTasks.Count > 0;
    public string CompletedToggleText => ShowCompleted ? "隐藏已完成" : $"已完成 ({CompletedTasks.Count})";
    public string DataFilePath => _dataService.DataFilePath;

    public bool CanRenameSelectedList => SelectedList is { IsSmartList: false };

    public string HeaderTitle => SelectedList?.Name ?? "任务";

    public string SelectedListName
    {
        get => SelectedList?.Name ?? string.Empty;
        set => RenameList(SelectedList, value);
    }
    public string HeaderSubtitle => SelectedList?.Type switch
    {
        ListType.MyDay => DateTime.Now.ToString("M月d日 dddd"),
        ListType.Important => "已标记为重要的任务",
        ListType.Planned => "有截止日期的任务",
        ListType.All => "所有未完成的任务",
        ListType.Completed => "已完成的任务",
        _ => $"{ActiveTasks.Count} 个任务"
    };

    public ICommand SelectListCommand { get; }
    public ICommand SelectTaskCommand { get; }
    public ICommand AddTaskCommand { get; }
    public ICommand ToggleCompleteCommand { get; }
    public ICommand ToggleImportantCommand { get; }
    public ICommand ToggleMyDayCommand { get; }
    public ICommand DeleteTaskCommand { get; }
    public ICommand AddListCommand { get; }
    public ICommand DeleteListCommand { get; }
    public ICommand BeginRenameListCommand { get; }
    public ICommand CommitRenameListCommand { get; }
    public ICommand SetDueDateCommand { get; }
    public ICommand ClearDueDateCommand { get; }
    public ICommand CloseDetailCommand { get; }
    public ICommand AddStepCommand { get; }
    public ICommand ToggleStepCommand { get; }
    public ICommand DeleteStepCommand { get; }
    public ICommand CommitStepEditCommand { get; }
    public ICommand ReorderTasksCommand { get; }
    public ICommand ReorderStepsCommand { get; }
    public ICommand ToggleShowCompletedCommand { get; }

    public bool CanReorderTasks => SelectedList?.Type is ListType.Custom
        or ListType.MyDay
        or ListType.Important
        or ListType.All;

    private void LoadData()
    {
        _appData = _dataService.Load();
        RefreshLists();

        var defaultList = SmartLists.FirstOrDefault(l => l.Type == ListType.MyDay)
            ?? SmartLists.FirstOrDefault()
            ?? CustomLists.FirstOrDefault();

        if (defaultList != null)
            SelectList(defaultList);
    }

    private void RefreshLists()
    {
        SmartLists.Clear();
        CustomLists.Clear();

        foreach (var list in _appData.Lists.OrderBy(l => l.SortOrder))
        {
            var vm = new ListItemViewModel(list, OnListNameChanged)
            {
                TaskCount = CountActiveTasks(list)
            };

            if (list.IsSmartList)
                SmartLists.Add(vm);
            else
                CustomLists.Add(vm);
        }
    }

    private int CountActiveTasks(TodoList list)
    {
        return TaskFilterService.FilterTasks(_appData.Tasks, list).Count();
    }

    private void SelectList(ListItemViewModel? list)
    {
        if (list == null) return;
        SelectedList = list;
    }

    private void SelectTask(TaskItemViewModel? task)
    {
        if (task != null)
            EnsureStepsSorted(task.Model);

        SelectedTask = task;
    }

    private void RefreshTasks()
    {
        var selectedId = SelectedTask?.Id;
        var preservedTask = SelectedTask;

        _isRefreshingTasks = true;
        try
        {
            ActiveTasks.Clear();
            CompletedTasks.Clear();

            if (SelectedList != null)
            {
                var active = TaskFilterService.FilterTasks(_appData.Tasks, SelectedList.Model)
                    .OrderBy(t => t.SortOrder)
                    .ThenByDescending(t => t.CreatedAt);

                foreach (var task in active)
                {
                    EnsureStepsSorted(task);
                    ActiveTasks.Add(new TaskItemViewModel(task, OnTaskSaveRequested, OnTaskMetadataChanged));
                }

                var completed = TaskFilterService.FilterCompletedTasks(_appData.Tasks, SelectedList.Model)
                    .OrderByDescending(t => t.CompletedAt);

                foreach (var task in completed)
                    CompletedTasks.Add(new TaskItemViewModel(task, OnTaskSaveRequested, OnTaskMetadataChanged));
            }

            if (selectedId.HasValue)
            {
                var model = _appData.Tasks.FirstOrDefault(t => t.Id == selectedId.Value);
                if (model is null)
                {
                    CloseDetail();
                }
                else
                {
                    var inList = ActiveTasks.FirstOrDefault(t => t.Id == selectedId.Value)
                        ?? CompletedTasks.FirstOrDefault(t => t.Id == selectedId.Value);

                    if (inList is not null)
                        SelectedTask = inList;
                    else if (preservedTask?.Id == selectedId.Value)
                        SelectedTask = preservedTask;
                    else
                        SelectedTask = new TaskItemViewModel(model, OnTaskSaveRequested, OnTaskMetadataChanged);
                }
            }
        }
        finally
        {
            _isRefreshingTasks = false;
        }

        OnPropertyChanged(nameof(HasCompletedTasks));
        OnPropertyChanged(nameof(CompletedToggleText));
        OnPropertyChanged(nameof(HeaderSubtitle));
        RefreshListCounts();
    }

    private void RefreshListCounts()
    {
        foreach (var list in SmartLists.Concat(CustomLists))
            list.TaskCount = CountActiveTasks(list.Model);
    }

    private void OnTaskSaveRequested(TaskItemViewModel task) => SaveData();

    private void OnTaskMetadataChanged(TaskItemViewModel task)
    {
        SaveData();
        RefreshListCounts();
        RefreshTasksIfSmartList(ListType.Important, ListType.MyDay, ListType.Planned, ListType.All);
    }

    private void AddTask()
    {
        if (string.IsNullOrWhiteSpace(NewTaskTitle) || SelectedList == null) return;
        if (SelectedList.Type == ListType.Completed) return;

        var listId = SelectedList.Type == ListType.Custom
            ? SelectedList.Id
            : GetOrCreateDefaultCustomListId();

        var task = new TodoTask
        {
            Title = NewTaskTitle.Trim(),
            ListId = listId,
            IsMyDay = SelectedList.Type == ListType.MyDay,
            SortOrder = GetNextTaskSortOrder(listId),
            CreatedAt = DateTime.Now
        };

        if (SelectedList.Type == ListType.Important)
            task.IsImportant = true;

        if (SelectedList.Type == ListType.Planned)
            task.DueDate = DateTime.Today;

        _appData.Tasks.Add(task);
        NewTaskTitle = string.Empty;
        SaveData();
        RefreshListCounts();
        RefreshTasks();

        var newVm = ActiveTasks.FirstOrDefault(t => t.Id == task.Id);
        if (newVm != null)
            SelectTask(newVm);
    }

    private void ToggleComplete(TaskItemViewModel? task)
    {
        if (task == null) return;
        task.IsCompleted = !task.IsCompleted;
        SaveData();
        RefreshTasks();
    }

    private void ToggleImportant(TaskItemViewModel? task)
    {
        if (task == null) return;
        task.IsImportant = !task.IsImportant;
        SaveData();
        RefreshListCounts();
        RefreshTasksIfSmartList(ListType.Important);
    }

    private void ToggleMyDay(TaskItemViewModel? task)
    {
        if (task == null) return;
        task.IsMyDay = !task.IsMyDay;
        SaveData();
        RefreshListCounts();
        RefreshTasksIfSmartList(ListType.MyDay);
    }

    private void DeleteTask(TaskItemViewModel? task)
    {
        if (task == null) return;

        var result = MessageBox.Show(
            $"确定要删除任务「{task.Title}」吗？",
            "删除任务",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        _appData.Tasks.RemoveAll(t => t.Id == task.Id);
        if (SelectedTask?.Id == task.Id)
            CloseDetail();

        SaveData();
        RefreshTasks();
    }

    private void AddList()
    {
        if (string.IsNullOrWhiteSpace(NewListName)) return;

        var customListCount = _appData.Lists.Count(l => l.Type == ListType.Custom);

        var list = new TodoList
        {
            Name = NewListName.Trim(),
            Type = ListType.Custom,
            SortOrder = _appData.Lists.Count,
            IconGlyph = "\uE8FD",
            Color = ListColorPalette.GetColor(customListCount)
        };

        _appData.Lists.Add(list);
        NewListName = string.Empty;
        SaveData();
        RefreshLists();

        var newVm = CustomLists.FirstOrDefault(l => l.Id == list.Id);
        if (newVm != null)
            SelectList(newVm);
    }

    private void OnListNameChanged(ListItemViewModel list)
    {
        SaveData();
        if (SelectedList?.Id == list.Id)
        {
            OnPropertyChanged(nameof(HeaderTitle));
            OnPropertyChanged(nameof(SelectedListName));
        }
    }

    private void RenameList(ListItemViewModel? list, string newName)
    {
        if (list is not { IsSmartList: false }) return;

        var trimmed = newName.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            OnPropertyChanged(nameof(SelectedListName));
            return;
        }

        if (list.Name == trimmed) return;

        list.Name = trimmed;
        OnPropertyChanged(nameof(HeaderTitle));
        OnPropertyChanged(nameof(SelectedListName));
    }

    private void BeginRenameList(ListItemViewModel? list)
    {
        if (list is not { IsSmartList: false }) return;

        foreach (var item in CustomLists)
            item.IsRenaming = false;

        SelectList(list);
        list.EnterRenameMode();
    }

    private void CommitRenameList(ListItemViewModel? list)
    {
        if (list is null || list.IsSmartList) return;
        list.ExitRenameMode();
    }

    private Guid GetOrCreateDefaultCustomListId()
    {
        var custom = _appData.Lists.FirstOrDefault(l => l.Type == ListType.Custom);
        if (custom != null)
            return custom.Id;

        var list = new TodoList
        {
            Name = "任务",
            Type = ListType.Custom,
            SortOrder = _appData.Lists.Count,
            IconGlyph = "\uE8FD",
            Color = ListColorPalette.GetColor(0)
        };

        _appData.Lists.Add(list);
        RefreshLists();
        return list.Id;
    }

    private void DeleteList(ListItemViewModel? list)
    {
        if (list == null || list.IsSmartList) return;

        if (_appData.Lists.Count(l => l.Type == ListType.Custom) <= 1)
        {
            MessageBox.Show(
                "至少需要保留一个任务列表。",
                "无法删除",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"确定要删除列表「{list.Name}」及其所有任务吗？",
            "删除列表",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        _appData.Tasks.RemoveAll(t => t.ListId == list.Id);
        _appData.Lists.RemoveAll(l => l.Id == list.Id);

        if (SelectedList?.Id == list.Id)
            SelectedList = null;

        SaveData();
        RefreshLists();

        if (SelectedList == null)
            SelectList(SmartLists.FirstOrDefault() ?? CustomLists.FirstOrDefault());
    }

    private void SetDueDate(object? parameter)
    {
        if (SelectedTask == null) return;

        SelectedTask.DueDate = parameter switch
        {
            DateTime dt => dt.Date,
            string option => option switch
            {
                "today" => DateTime.Today,
                "tomorrow" => DateTime.Today.AddDays(1),
                "nextweek" => DateTime.Today.AddDays(7),
                _ => SelectedTask.DueDate
            },
            _ => SelectedTask.DueDate
        };

        RefreshTasksIfSmartList(ListType.Planned);
    }

    private void ClearDueDate()
    {
        if (SelectedTask == null) return;
        SelectedTask.DueDate = null;
        RefreshTasksIfSmartList(ListType.Planned);
    }

    private void CloseDetail()
    {
        SelectedTask = null;
        IsDetailPanelOpen = false;
    }

    private void AddStep()
    {
        if (SelectedTask == null || string.IsNullOrWhiteSpace(NewStepTitle)) return;

        SelectedTask.Steps.Add(new TodoStep
        {
            Title = NewStepTitle.Trim(),
            SortOrder = SelectedTask.Steps.Count
        });

        NewStepTitle = string.Empty;
        RefreshTaskStepDisplay(SelectedTask.Id);
        SaveData();
    }

    private void ToggleStep(TodoStep? step)
    {
        if (step == null || SelectedTask == null) return;
        step.IsCompleted = !step.IsCompleted;
        RefreshTaskStepDisplay(SelectedTask.Id);
        SaveData();
    }

    private void DeleteStep(TodoStep? step)
    {
        if (step == null || SelectedTask == null) return;
        SelectedTask.Steps.Remove(step);
        CollectionReorderHelper.ReindexSortOrder(SelectedTask.Steps, (s, i) => s.SortOrder = i);
        RefreshTaskStepDisplay(SelectedTask.Id);
        SaveData();
    }

    private void CommitStepEdit(TodoStep? step)
    {
        if (step == null || SelectedTask == null) return;

        step.Title = step.Title.Trim();
        RefreshTaskStepDisplay(SelectedTask.Id);
        SaveData();
    }

    private void ReorderTasks(object? parameter)
    {
        if (parameter is not ReorderInfo info || SelectedList == null || !CanReorderTasks) return;

        CollectionReorderHelper.MoveItem(ActiveTasks, info.OldIndex, info.NewIndex);
        CollectionReorderHelper.ReindexSortOrder(ActiveTasks, (task, index) => task.Model.SortOrder = index);
        SaveData();
    }

    private void ReorderSteps(object? parameter)
    {
        if (parameter is not ReorderInfo info || SelectedTask == null) return;

        var steps = SelectedTask.Steps;
        CollectionReorderHelper.MoveItem(steps, info.OldIndex, info.NewIndex);
        CollectionReorderHelper.ReindexSortOrder(steps, (step, index) => step.SortOrder = index);
        RefreshTaskStepDisplay(SelectedTask.Id);
        SaveData();
    }

    private static void EnsureStepsSorted(TodoTask task)
    {
        var sorted = task.Steps.OrderBy(s => s.SortOrder).ToList();
        if (sorted.SequenceEqual(task.Steps)) return;

        task.Steps.Clear();
        foreach (var step in sorted)
            task.Steps.Add(step);
    }

    private int GetNextTaskSortOrder(Guid listId)
    {
        var maxOrder = _appData.Tasks
            .Where(t => t.ListId == listId && !t.IsCompleted)
            .Select(t => t.SortOrder)
            .DefaultIfEmpty(-1)
            .Max();

        return maxOrder + 1;
    }

    private void RefreshTaskStepDisplay(Guid taskId)
    {
        if (SelectedTask?.Id == taskId)
            SelectedTask.RefreshSteps();

        foreach (var task in ActiveTasks.Concat(CompletedTasks))
        {
            if (task.Id == taskId)
                task.RefreshSteps();
        }
    }

    private void RefreshTasksIfSmartList(params ListType[] types)
    {
        if (SelectedList != null && types.Contains(SelectedList.Type))
            RefreshTasks();
    }

    private void SaveData() => _dataService.Save(_appData);
}
