namespace AITakeOverEscape;

internal class GameplayScene : IScene
{
    private const char PlayerRenderChar = '@';
    private const char RobotRenderChar = '#';
    internal const char ClosedVerticalDoorRenderChar = '-'; // vertical as in a door where one travels vertically
    internal const char OpenVerticalDoorRenderChar = '/';
    internal const char ClosedHorizontalDoorRenderChar = '|';
    internal const char OpenHorizontalDoorRenderChar = '_';
    private const double PlayerMovesPerSecond = 5;
    private const int FlashlightFloodFillRange = 6;
    private const double FlashlightAbsoluteDistanceRange = 4;
    
    private char?[,] CharMap { get; }
    private (int col, int row) PlayerPosition { get; set; }
    private double PlayerTimeSinceLastMove { get; set; }
    private List<Robot> RobotList { get; }
    private uint FrameNum { get; set; }
    private GameplayMenuPrompt? MenuPrompt { get; set; }
    private bool IsFlashlightActive { get; set; }
    private List<(char character, (int col, int row) position, bool isPickedUp)> ItemList { get; }
    private List<(int ID, (int col, int row) positoin, bool isOpen)> DoorList { get; }
    private List<(char character, (int col, int row) position)> SpecialInteractableList { get; }

    internal GameplayScene()
    {
        CharMap = new char?[Map.Width, Map.Height]; 
        RobotList = new List<Robot>();
        ItemList = new();
        DoorList = new();
        SpecialInteractableList = new();
        PullFromMap();
    }

    private void PullFromMap()
    {
          var rowNum = 0; 
          foreach (var row in Map.DefaultMap) 
          { 
              var colNum = 0; 
              foreach (var character in row) 
              { 
                  UseMapPointBasedOnChar((colNum, rowNum), character);
                  colNum++;
              } 
              rowNum++;
          }
    }

    private void UseMapPointBasedOnChar((int col, int row) pos, char c)
    {
          if (c == Map.PlayerChar) 
          { 
              PlayerPosition = (pos.col, pos.row);
          }
          else if (c == Map.RobotChar) 
          { 
              RobotList.Add(new Robot(pos.col, pos.row));
          }
          else if (CharToInt(c) != -1)
          {
              DoorList.Add((CharToInt(c), pos, false));
          }
          else if (Map.ItemCharArray.Contains(c)) 
          { 
              ItemList.Add((c, pos, false));
          }
          else if (Map.SpecialCharArray.Contains(c)) 
          { 
              SpecialInteractableList.Add((c, pos));
          }
          else if (c != ' ') 
              CharMap[pos.col, pos.row] = c;
    }

    void IScene.Update()
    {
        FrameNum++;
        char[,] completeCharMap = GetCompleteCharMap();
        UpdateRobots();
        UpdatePlayerMovement(completeCharMap);
        AddVisibleCharMapToFrame(completeCharMap);
        UpdateMenuPrompts();
    }

    private void UpdateMenuPrompts()
    {
        if (Math.Abs(FrameNum - Program.TargetFramesPerSecond * 2) <= 0.1)
            MenuPrompt = new GameplayMenuPrompt(
                "It seems the lights have gone out.",
                ["Activate flashlight"],
                _ =>
                {
                    IsFlashlightActive = true;
                    MenuPrompt = null;
                },
                (ConsoleKey.Spacebar, "Spacebar")
            );
        MenuPrompt?.Update();
    }
    
    private void UpdateRobots()
    {
        foreach (var robot in RobotList)
        {
           robot.UpdateMovement(CharMap, PlayerPosition); 
        }
    }

    private void AddVisibleCharMapToFrame(char[,] completeCharMap)
    {
        bool isAllVisible = IsAllVisible();
        List<(int col, int row)> allInFlashlightRange = 
            GetAllInFloodFillRange(FlashlightFloodFillRange, completeCharMap);
        allInFlashlightRange = FilterForInFlashLightAbsoluteRange(allInFlashlightRange);
        for (int row = 0; row < Map.Height; row++)
        {
            for (int col = 0; col < Map.Width; col++)
            {
                if (isAllVisible || completeCharMap[col, row] == '@' || 
                    (allInFlashlightRange.Contains((col, row)) && IsFlashlightActive))
                    Program.Frame += completeCharMap[col, row];
                else
                    Program.Frame += ' ';
            }

            Program.Frame += '\n';
        }
    }

    private bool IsAllVisible()
        => (FrameNum < Program.TargetFramesPerSecond * 1.5 && FrameNum % 2 == 0)
           || FrameNum < Program.TargetFramesPerSecond;


    private List<(int col, int row)> GetAllInFloodFillRange(int reiterations, char[,] charMap)
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
                    if (IsEmptyOrPermeable(charMap[surroundingPoint.col, surroundingPoint.row])
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

    private List<(int col, int row)> FilterForInFlashLightAbsoluteRange(List<(int col, int row)> positionList)
    {
        List<(int col, int row)> results = new();
        foreach (var position in positionList)
        {
            if (FlashlightAbsoluteDistanceRange >=
                Math.Sqrt(Square(PlayerPosition.row - position.row) + Square(PlayerPosition.col - position.col)))
            {
                results.Add(position);
            }
        }
        return results;
    }

    private int Square(int x) => x * x;

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
        
        foreach (var item in ItemList)
            if (!item.isPickedUp)
                result[item.position.col, item.position.row] = item.character;
        foreach (var specialInteractable in SpecialInteractableList)
            result[specialInteractable.position.col, specialInteractable.position.row] 
                = specialInteractable.character;
        foreach (var door in DoorList) 
            if (door.isOpen) result[door.positoin.col, door.positoin.row] 
                = Map.IsDoorVertical(door.ID) ? OpenVerticalDoorRenderChar : OpenHorizontalDoorRenderChar;
            else result[door.positoin.col, door.positoin.row] 
                = Map.IsDoorVertical(door.ID) ? ClosedVerticalDoorRenderChar : ClosedHorizontalDoorRenderChar;
        result[PlayerPosition.col, PlayerPosition.row] = PlayerRenderChar; 
        foreach (var robot in RobotList) 
            result[robot.Position.col, robot.Position.row] = RobotRenderChar;
        return result;
    }

    private void UpdatePlayerMovement(char[,] completeCharMap)
    {
        PlayerTimeSinceLastMove += Program.DeltaTime;
        if (PlayerTimeSinceLastMove > 1 / PlayerMovesPerSecond)
        {
            if (
                (Program.PressedKeys.Contains(ConsoleKey.UpArrow) || Program.PressedKeys.Contains(ConsoleKey.W))
                && IsEmptyOrPermeable(completeCharMap[PlayerPosition.col, PlayerPosition.row-1])
                )
            {
                PlayerPosition = (PlayerPosition.col, PlayerPosition.row-1);
                PlayerTimeSinceLastMove = 0;
                return;
            }

            if (
                (Program.PressedKeys.Contains(ConsoleKey.DownArrow) || Program.PressedKeys.Contains(ConsoleKey.S))
                 && IsEmptyOrPermeable(completeCharMap[PlayerPosition.col, PlayerPosition.row+1])
                )
            {
                 PlayerPosition = (PlayerPosition.col, PlayerPosition.row+1);
                 PlayerTimeSinceLastMove = 0;
                 return;
            }
            
            if (
                (Program.PressedKeys.Contains(ConsoleKey.LeftArrow) || Program.PressedKeys.Contains(ConsoleKey.A))
                && IsEmptyOrPermeable(completeCharMap[PlayerPosition.col-1, PlayerPosition.row])
                )
            {
                 PlayerPosition = (PlayerPosition.col-1, PlayerPosition.row);
                 PlayerTimeSinceLastMove = 0;
                 return;
            }
                        
            if (
                (Program.PressedKeys.Contains(ConsoleKey.RightArrow) || Program.PressedKeys.Contains(ConsoleKey.D))
                && IsEmptyOrPermeable(completeCharMap[PlayerPosition.col+1, PlayerPosition.row])
                ) 
            {
                 PlayerPosition = (PlayerPosition.col+1, PlayerPosition.row);
                 PlayerTimeSinceLastMove = 0;
            }
        }
    }

    private int CharToInt(char c)
    {
        switch (c)
        {
            case '0': return 0;
            case '1': return 1; 
            case '2': return 2; 
            case '3': return 3; 
            case '4': return 4;
            case '5': return 5;
            case '6': return 6;
            case '7': return 7; 
            case '8': return 8;
            case '9': return 9;
            default: return -1;
        }
    }

    private bool IsEmptyOrPermeable(char c)
        => c == ' ' || c == RobotRenderChar
                    || c == OpenHorizontalDoorRenderChar || c == OpenVerticalDoorRenderChar;
}
