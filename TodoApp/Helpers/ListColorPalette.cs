namespace TodoApp.Helpers;

public static class ListColorPalette
{
    public static readonly string[] Colors =
    [
        "#0078D4", // 蓝
        "#107C10", // 绿
        "#D13438", // 红
        "#FFB900", // 金
        "#8764B8", // 紫
        "#E3008C", // 粉
        "#00B7C3", // 青
        "#FF8C00", // 橙
        "#5C2E91", // 深紫
        "#498205", // 橄榄绿
    ];

    public static string GetColor(int index) => Colors[index % Colors.Length];
}
