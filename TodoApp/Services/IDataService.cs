using TodoApp.Models;

namespace TodoApp.Services;

public interface IDataService
{
    AppData Load();
    void Save(AppData data);
    string DataFilePath { get; }
}
