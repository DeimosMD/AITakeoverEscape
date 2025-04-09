namespace AITakeOverEscape;

internal static class Map
{
    internal static string[] DefaultMap =>
    [
        "OOOOOOO",
        "O      ",
        "O OOOOO",
        "O@OOOOO",
        "OOOOOOO"
    ];
    
    internal static int Height => DefaultMap.Length;
    internal static int Width => DefaultMap[0].Length;
}