namespace TodoApp.Helpers;

public static class CollectionReorderHelper
{
    public static void MoveItem<T>(IList<T> list, int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || oldIndex >= list.Count) return;
        if (newIndex < 0 || newIndex >= list.Count) return;
        if (oldIndex == newIndex) return;

        var item = list[oldIndex];
        list.RemoveAt(oldIndex);
        list.Insert(newIndex, item);
    }

    public static void ReindexSortOrder<T>(IList<T> items, Action<T, int> setSortOrder)
    {
        for (var i = 0; i < items.Count; i++)
            setSortOrder(items[i], i);
    }
}
