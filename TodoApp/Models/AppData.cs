namespace TodoApp.Models;

public class AppData
{
    public List<TodoList> Lists { get; set; } = [];
    public List<TodoTask> Tasks { get; set; } = [];
}
