namespace TodoApp.Models;

public enum ListType
{
    Custom,
    MyDay,
    Important,
    Planned,
    All,
    Completed
}

public class TodoList
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ListType Type { get; set; } = ListType.Custom;
    public string IconGlyph { get; set; } = "\uE8FD";
    public string Color { get; set; } = "#0078D4";
    public int SortOrder { get; set; }
    public bool IsSmartList => Type != ListType.Custom;
}
