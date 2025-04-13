namespace AITakeOverEscape;

internal static class Map
{
    internal static string[] DefaultMap =>
    [
        "OOOOOOOOO",
        "O@      O",
        "O OOOOO O",
        "OOOO OOOO",
        "O O   O O",
        "O      #O",
        "OOOOOOOOO"
    ];
    
    internal static int Height => DefaultMap.Length;
    internal static int Width => DefaultMap[0].Length;
}