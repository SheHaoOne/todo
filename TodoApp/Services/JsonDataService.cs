using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using TodoApp.Models;

namespace TodoApp.Services;

public class JsonDataService : IDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public string DataFilePath { get; }

    public JsonDataService()
    {
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TodoApp");
        Directory.CreateDirectory(appDataFolder);
        DataFilePath = Path.Combine(appDataFolder, "data.json");
    }

    public AppData Load()
    {
        if (!File.Exists(DataFilePath))
            return CreateDefaultData();

        try
        {
            var json = File.ReadAllText(DataFilePath);
            return JsonSerializer.Deserialize<AppData>(json, JsonOptions) ?? CreateDefaultData();
        }
        catch
        {
            return CreateDefaultData();
        }
    }

    public void Save(AppData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(DataFilePath, json);
    }

    private static AppData CreateDefaultData()
    {
        return new AppData
        {
            Lists =
            [
                new TodoList { Id = SmartListIds.MyDay, Name = "我的一天", Type = ListType.MyDay, IconGlyph = "\uE706", SortOrder = 0 },
                new TodoList { Id = SmartListIds.Important, Name = "重要", Type = ListType.Important, IconGlyph = "\uE735", SortOrder = 1 },
                new TodoList { Id = SmartListIds.Planned, Name = "计划内", Type = ListType.Planned, IconGlyph = "\uE787", SortOrder = 2 },
                new TodoList { Id = SmartListIds.All, Name = "全部", Type = ListType.All, IconGlyph = "\uE8FD", SortOrder = 3 },
                new TodoList { Id = SmartListIds.Completed, Name = "已完成", Type = ListType.Completed, IconGlyph = "\uE73E", SortOrder = 4 },
                new TodoList { Id = Guid.NewGuid(), Name = "任务", Type = ListType.Custom, IconGlyph = "\uE8FD", Color = "#0078D4", SortOrder = 5 }
            ]
        };
    }
}
