namespace AITakeOverEscape;

internal static class Map
{
    internal static string[] DefaultMap =>
    [
        "OOOOOOOOOOOOOOOO",
        "O              O",
        "O OOOOO OOOOOO O",
        "O      @O      O",
        "O OOOOO O O    O",
        "O         O    O",
        "OOOOOOOOOOOOOOOO"
    ];
    
    internal static int Height => DefaultMap.Length;
    internal static int Width => DefaultMap[0].Length;
}