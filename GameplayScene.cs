namespace AITakeOverEscape;

internal class GameplayScene : IScene
{
    private const char PlayerChar = '@';
    private const char RobotChar = '#';
    private const double PlayerMovesPerSecond = 4;
    private char?[,] CharMap { get; }
    private (int col, int row) PlayerPosition { get; set; }
    private double PlayerTimeSinceLastMove { get; set; }
    private List<Robot> RobotList { get; }

    internal GameplayScene()
    {
        CharMap = new char?[Map.Width, Map.Height]; 
        RobotList = new List<Robot>();
        var rowNum = 0; 
        foreach (var row in Map.DefaultMap) 
        { 
            var colNum = 0; 
            foreach (var character in row) 
            { 
                if (character == PlayerChar)
                {
                    PlayerPosition = (colNum, rowNum);
                }
                else if (character == RobotChar)
                {
                    RobotList.Add(new Robot(colNum, rowNum));
                }
                else if (character != ' ')
                    CharMap[colNum, rowNum] = character;
                colNum++;
            }
            rowNum++;
        }
    }

    void IScene.Update()
    {
        UpdateRobots();
        UpdatePlayerMovement();
        AddCharMapToFrame();
    }

    private void UpdateRobots()
    {
        foreach (var robot in RobotList)
        {
           robot.UpdateMovement(CharMap, PlayerPosition); 
        }
    }

    private void AddCharMapToFrame()
    {
        char[,] charMap = GetDrawableCharMap();
        for (int row = 0; row < Map.Height; row++)
        {
            for (int col = 0; col < Map.Width; col++)
            {
                Program.Frame += charMap[col, row];
            }

            Program.Frame += '\n';
        }
    }
    
    private char[,] GetDrawableCharMap()
    {
        char[,] result = new char[Map.Width, Map.Height];
        for (int row = 0; row < Map.Height; row++)
        {
            for (int col = 0; col < Map.Width; col++)
            {
                var ch = CharMap[col, row];
                result[col, row] = ch ?? ' ';
            }
        }
        
        result[PlayerPosition.col, PlayerPosition.row] = PlayerChar;
        foreach (var robot in RobotList)
            result[robot.Position.col, robot.Position.row] = RobotChar;
        return result;
    }

    private void UpdatePlayerMovement()
    {
        PlayerTimeSinceLastMove += Program.DeltaTime;
        if (PlayerTimeSinceLastMove > 1 / PlayerMovesPerSecond)
        {
            if (
                (Program.PressedKeys.Contains(ConsoleKey.UpArrow) || Program.PressedKeys.Contains(ConsoleKey.W))
                && CharMap[PlayerPosition.col, PlayerPosition.row-1] == null
                )
            {
                PlayerPosition = (PlayerPosition.col, PlayerPosition.row-1);
                PlayerTimeSinceLastMove = 0;
                return;
            }

            if (
                (Program.PressedKeys.Contains(ConsoleKey.DownArrow) || Program.PressedKeys.Contains(ConsoleKey.S))
                 && CharMap[PlayerPosition.col, PlayerPosition.row+1] == null
                )
            {
                 PlayerPosition = (PlayerPosition.col, PlayerPosition.row+1);
                 PlayerTimeSinceLastMove = 0;
                 return;
            }
            
            if (
                (Program.PressedKeys.Contains(ConsoleKey.LeftArrow) || Program.PressedKeys.Contains(ConsoleKey.A))
                     && CharMap[PlayerPosition.col-1, PlayerPosition.row] == null
                )
            {
                 PlayerPosition = (PlayerPosition.col-1, PlayerPosition.row);
                 PlayerTimeSinceLastMove = 0;
                 return;
            }
                        
            if (
                (Program.PressedKeys.Contains(ConsoleKey.RightArrow) || Program.PressedKeys.Contains(ConsoleKey.D))
                     && CharMap[PlayerPosition.col+1, PlayerPosition.row] == null
                ) 
            {
                 PlayerPosition = (PlayerPosition.col+1, PlayerPosition.row); 
                 PlayerTimeSinceLastMove = 0;
            }
        }
    }
}
