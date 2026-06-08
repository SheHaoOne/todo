using TodoApp.Models;

namespace TodoApp.Services;

public static class TaskFilterService
{
    public static IEnumerable<TodoTask> FilterTasks(IEnumerable<TodoTask> tasks, TodoList list)
    {
        return list.Type switch
        {
            ListType.MyDay => tasks.Where(t => t.IsMyDay && !t.IsCompleted),
            ListType.Important => tasks.Where(t => t.IsImportant && !t.IsCompleted),
            ListType.Planned => tasks.Where(t => t.DueDate.HasValue && !t.IsCompleted)
                .OrderBy(t => t.DueDate),
            ListType.All => tasks.Where(t => !t.IsCompleted),
            ListType.Completed => tasks.Where(t => t.IsCompleted)
                .OrderByDescending(t => t.CompletedAt),
            ListType.Custom => tasks.Where(t => t.ListId == list.Id && !t.IsCompleted),
            _ => tasks
        };
    }

    public static IEnumerable<TodoTask> FilterCompletedTasks(IEnumerable<TodoTask> tasks, TodoList list)
    {
        if (list.Type == ListType.Completed)
            return [];

        return list.Type switch
        {
            ListType.MyDay => tasks.Where(t => t.IsMyDay && t.IsCompleted),
            ListType.Important => tasks.Where(t => t.IsImportant && t.IsCompleted),
            ListType.Planned => tasks.Where(t => t.DueDate.HasValue && t.IsCompleted),
            ListType.All => tasks.Where(t => t.IsCompleted),
            ListType.Custom => tasks.Where(t => t.ListId == list.Id && t.IsCompleted),
            _ => []
        };
    }
}
