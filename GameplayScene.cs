namespace AITakeOverEscape;

internal class GameplayScene : IScene
{
    private const char PlayerChar = '@';
    private const char RobotChar = '#';
    private const double PlayerMovesPerSecond = 4;
    private const int FlashlightRange = 5;
    private char?[,] CharMap { get; }
    private (int col, int row) PlayerPosition { get; set; }
    private double PlayerTimeSinceLastMove { get; set; }
    private List<Robot> RobotList { get; }
    private uint FrameNum { get; set; }

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
        FrameNum++;
        UpdateRobots();
        UpdatePlayerMovement();
        AddVisibleCharMapToFrame();
    }

    private void UpdateRobots()
    {
        foreach (var robot in RobotList)
        {
           robot.UpdateMovement(CharMap, PlayerPosition); 
        }
    }

    private void AddVisibleCharMapToFrame()
    {
        char[,] charMap = GetCompleteCharMap();
        bool isAllVisible = IsAllVisible();
        List<(int col, int row)> allInFlashlightRange = GetAllInFlashlightRange(FlashlightRange, charMap);
        for (int row = 0; row < Map.Height; row++)
        {
            for (int col = 0; col < Map.Width; col++)
            {
                if (isAllVisible || charMap[col, row] == '@' || allInFlashlightRange.Contains((col, row)))
                    Program.Frame += charMap[col, row];
                else
                    Program.Frame += ' ';
            }

            Program.Frame += '\n';
        }
    }

    private bool IsAllVisible()
        => (FrameNum < Program.TargetFramesPerSecond * 1.5 && FrameNum % 2 == 0)
           || FrameNum < Program.TargetFramesPerSecond;


    private List<(int col, int row)> GetAllInFlashlightRange(int reiterations, char[,] charMap)
    {
        List<(int col, int row)> results = []; // all non-empty (and non-robot) points visible as result of flashlight
        List<(int col, int row)> allEmptyFoundList = []; 
        List<(int col, int row)> recentEmptyFoundList = [PlayerPosition]; // uses player position as starting point
        for (int i = 0; i < reiterations; i++)
        {
            List<(int col, int row)> currentEmptyFoundList = [];
            foreach (var emptyFoundPoint in recentEmptyFoundList)
            {
                foreach (var surroundingPoint in GetSurroundingPoints(emptyFoundPoint))
                {
                    if ((charMap[surroundingPoint.col, surroundingPoint.row] == ' ' || 
                        charMap[surroundingPoint.col, surroundingPoint.row] == '#')
                        && !allEmptyFoundList.Contains(surroundingPoint))
                    {
                        currentEmptyFoundList.Add(surroundingPoint);
                        allEmptyFoundList.Add(emptyFoundPoint);
                    }
                    else if (!results.Contains(surroundingPoint))
                        results.Add(surroundingPoint);
                }
            }
            recentEmptyFoundList = currentEmptyFoundList;
        }

        return results;
    }

    private List<(int col, int row)> GetSurroundingPoints((int col, int row) position)
        =>
        [
            position with { row = position.row + 1},
            position with { row = position.row - 1},
            position with { col = position.col + 1},
            position with { col = position.col - 1}
        ]; 

    private char[,] GetCompleteCharMap()
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
