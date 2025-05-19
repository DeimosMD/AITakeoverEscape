namespace AITakeOverEscape;

internal class GameCompletionScene : IScene
{
    private const string WinningText = "You have completed the game!";
    private const int AnimationHeight = 3;
    private const double EscapePodMovesPerSecond = 10.0;

    private (int min, int max) PlayerColOffsetBounds => (0, 2); // inclusive bounds of PlayerColOffset
    private int WinningTextColumn => (Console.WindowWidth - WinningText.Length) / 2;
    private double SceneTime { get; set; }
    private (int col, int row) EscapePodPosition => ((int) (SceneTime * EscapePodMovesPerSecond) - 10, 1);
    private int PlayerColOffset { get; set; } = 1;
    
    void IScene.Update()
    {
        SceneTime += Program.DeltaTime;
        UpdatePlayerMovement();
        AddSpacesToFrameForWinningTextColumn();
        Program.Frame += WinningText + "\n\n\n\n\n\n";
        AddAnimationToFrame();
    }

    private void AddAnimationToFrame()
    {
        char[,] animationChars = GetAnimationChars();
        for (int row = 0; row < animationChars.GetLength(1); row++)
        {
            for (int col = 0; col < animationChars.GetLength(0); col++)
            {
                Program.Frame += animationChars[col, row];
            }

            Program.Frame += "\n";
        }
    }

    private char[,] GetAnimationChars()
    {
        char[,] result = new char[Console.WindowWidth, AnimationHeight];
        for (int i = 0; i < result.GetLength(0); i++)
            for (int j = 0; j < result.GetLength(1); j++)
                result[i, j] = ' ';
        foreach (var p in GetEscapePodPoints())
        {
           (int col, int row) absolutePosition = (EscapePodPosition.col + p.colOffset, 
               EscapePodPosition.row + p.rowOffset);
           if (absolutePosition is { col: >= 0, row: >= 0 } && absolutePosition.col < result.GetLength(0)
               && absolutePosition.row < result.GetLength(1))
               result[absolutePosition.col, absolutePosition.row] = p.character;
        }
        return result;
    }

    private (int colOffset, int rowOffset, char character)[] GetEscapePodPoints()
        => [
            (PlayerColOffset, 0, '@'),
            (-1, 0, '<'),
            (-2, 0, '>'),
            (-3, 0, '~'),
            (3, 0,'>'),
            (0, 1, '='),
            (0, -1, '='),
            (-1, -1, '/'),
            (-1, 1, '\\'),
            (1, -1, '='),
            (1, 1, '='),
            (2, -1, '\\'),
            (2, 1, '/')
        ];

    private void AddSpacesToFrameForWinningTextColumn()
    {
        for (int i = 0; i < WinningTextColumn; i++)
        {
            Program.Frame += ' ';
        }
    }

    private void UpdatePlayerMovement()
    {
        if ((Program.PressedKeys.Contains(ConsoleKey.LeftArrow) || Program.PressedKeys.Contains(ConsoleKey.A))
            && PlayerColOffset > PlayerColOffsetBounds.min)
            PlayerColOffset--;
        if ((Program.PressedKeys.Contains(ConsoleKey.RightArrow) || Program.PressedKeys.Contains(ConsoleKey.D))
            && PlayerColOffset < PlayerColOffsetBounds.max)
            PlayerColOffset++;
    }
}