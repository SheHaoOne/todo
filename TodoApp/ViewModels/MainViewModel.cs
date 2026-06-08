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
        SetDueDateCommand = new RelayCommand(p => SetDueDate(p));
        ClearDueDateCommand = new RelayCommand(_ => ClearDueDate(), _ => SelectedTask?.DueDate != null);
        CloseDetailCommand = new RelayCommand(_ => CloseDetail());
        AddStepCommand = new RelayCommand(AddStep, () => !string.IsNullOrWhiteSpace(NewStepTitle) && SelectedTask != null);
        ToggleStepCommand = new RelayCommand(p => ToggleStep(p as TodoStep));
        DeleteStepCommand = new RelayCommand(p => DeleteStep(p as TodoStep));
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
            RefreshTasks();
            CloseDetail();
        }
    }

    public TaskItemViewModel? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (_selectedTask != null)
                _selectedTask.IsSelected = false;

            if (!SetProperty(ref _selectedTask, value)) return;

            if (_selectedTask != null)
                _selectedTask.IsSelected = true;

            IsDetailPanelOpen = _selectedTask != null;
            OnPropertyChanged(nameof(HasSelectedTask));
        }
    }

    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set => SetProperty(ref _newTaskTitle, value);
    }

    public string NewListName
    {
        get => _newListName;
        set => SetProperty(ref _newListName, value);
    }

    public string NewStepTitle
    {
        get => _newStepTitle;
        set => SetProperty(ref _newStepTitle, value);
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

    public string HeaderTitle => SelectedList?.Name ?? "任务";
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
    public ICommand SetDueDateCommand { get; }
    public ICommand ClearDueDateCommand { get; }
    public ICommand CloseDetailCommand { get; }
    public ICommand AddStepCommand { get; }
    public ICommand ToggleStepCommand { get; }
    public ICommand DeleteStepCommand { get; }
    public ICommand ToggleShowCompletedCommand { get; }

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
            var vm = new ListItemViewModel(list)
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

        foreach (var item in SmartLists.Concat(CustomLists))
            item.IsSelected = false;

        list.IsSelected = true;
        SelectedList = list;
        OnPropertyChanged(nameof(HeaderTitle));
        OnPropertyChanged(nameof(HeaderSubtitle));
    }

    private void SelectTask(TaskItemViewModel? task)
    {
        SelectedTask = task;
    }

    private void RefreshTasks()
    {
        ActiveTasks.Clear();
        CompletedTasks.Clear();

        if (SelectedList == null) return;

        var active = TaskFilterService.FilterTasks(_appData.Tasks, SelectedList.Model)
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.CreatedAt);

        foreach (var task in active)
            ActiveTasks.Add(new TaskItemViewModel(task, OnTaskChanged));

        var completed = TaskFilterService.FilterCompletedTasks(_appData.Tasks, SelectedList.Model)
            .OrderByDescending(t => t.CompletedAt);

        foreach (var task in completed)
            CompletedTasks.Add(new TaskItemViewModel(task, OnTaskChanged));

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

    private void OnTaskChanged(TaskItemViewModel task)
    {
        SaveData();
        RefreshTasks();

        if (SelectedTask?.Id == task.Id)
            SelectedTask = ActiveTasks.FirstOrDefault(t => t.Id == task.Id)
                ?? CompletedTasks.FirstOrDefault(t => t.Id == task.Id);
    }

    private void AddTask()
    {
        if (string.IsNullOrWhiteSpace(NewTaskTitle) || SelectedList == null) return;

        var listId = SelectedList.Type == ListType.Custom
            ? SelectedList.Id
            : CustomLists.FirstOrDefault()?.Id ?? _appData.Lists.First(l => l.Type == ListType.Custom).Id;

        var task = new TodoTask
        {
            Title = NewTaskTitle.Trim(),
            ListId = listId,
            IsMyDay = SelectedList.Type == ListType.MyDay,
            SortOrder = _appData.Tasks.Count,
            CreatedAt = DateTime.Now
        };

        if (SelectedList.Type == ListType.Important)
            task.IsImportant = true;

        if (SelectedList.Type == ListType.Planned)
            task.DueDate = DateTime.Today;

        _appData.Tasks.Add(task);
        NewTaskTitle = string.Empty;
        SaveData();
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
    }

    private void ToggleMyDay(TaskItemViewModel? task)
    {
        if (task == null) return;
        task.IsMyDay = !task.IsMyDay;
        SaveData();
        RefreshListCounts();
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

        var list = new TodoList
        {
            Name = NewListName.Trim(),
            Type = ListType.Custom,
            SortOrder = _appData.Lists.Count,
            IconGlyph = "\uE8FD",
            Color = "#0078D4"
        };

        _appData.Lists.Add(list);
        NewListName = string.Empty;
        SaveData();
        RefreshLists();

        var newVm = CustomLists.FirstOrDefault(l => l.Id == list.Id);
        if (newVm != null)
            SelectList(newVm);
    }

    private void DeleteList(ListItemViewModel? list)
    {
        if (list == null || list.IsSmartList) return;

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
        if (SelectedTask == null || parameter is not string option) return;

        SelectedTask.DueDate = option switch
        {
            "today" => DateTime.Today,
            "tomorrow" => DateTime.Today.AddDays(1),
            "nextweek" => DateTime.Today.AddDays(7),
            _ => SelectedTask.DueDate
        };

        SaveData();
        RefreshListCounts();
    }

    private void ClearDueDate()
    {
        if (SelectedTask == null) return;
        SelectedTask.DueDate = null;
        SaveData();
        RefreshListCounts();
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
        SelectedTask.RefreshSteps();
        SaveData();
    }

    private void ToggleStep(TodoStep? step)
    {
        if (step == null || SelectedTask == null) return;
        step.IsCompleted = !step.IsCompleted;
        SelectedTask.RefreshSteps();
        SaveData();
    }

    private void DeleteStep(TodoStep? step)
    {
        if (step == null || SelectedTask == null) return;
        SelectedTask.Steps.Remove(step);
        SelectedTask.RefreshSteps();
        SaveData();
    }

    private void SaveData() => _dataService.Save(_appData);
}
