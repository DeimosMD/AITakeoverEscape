namespace AITakeOverEscape;

internal class GameplayScene : IScene
{
    private const char PlayerRenderChar = '@';
    private const char RobotRenderChar = '#';
    private const char ClosedVerticalDoorRenderChar = '-'; // vertical as in a door where one travels vertically
    private const char OpenVerticalDoorRenderChar = '/';
    private const char ClosedHorizontalDoorRenderChar = '|';
    private const char OpenHorizontalDoorRenderChar = '_';
    private const double PlayerMovesPerSecond = 5;
    private const int FlashlightFloodFillRange = 6;
    private const double FlashlightAbsoluteDistanceRange = 4;
    private const int ShipLightsFloodFillRange = 12;
    private const double ShipLightsAbsoluteDistanceRange = 8;
    
    private static (ConsoleKey, string) Spacebar { get; } = (ConsoleKey.Spacebar, "Spacebar");
    private char?[,] CharMap { get; }
    private (int col, int row) PlayerPosition { get; set; }
    private double PlayerTimeSinceLastMove { get; set; }
    private List<Robot> RobotList { get; }
    private uint FrameNum { get; set; }
    private GameplayMenuPrompt? MenuPrompt { get; set; }
    private bool IsFlashlightActive { get; set; }
    private Dictionary<char, ((int col, int row) position, bool isPickedUp)> ItemDictionary { get; }
    private ((int col, int row) position, bool isOpen)[] Doors { get; }
    private List<(char character, (int col, int row) position)> SpecialInteractableList { get; }

    internal GameplayScene()
    {
        CharMap = new char?[Map.Width, Map.Height]; 
        RobotList = new List<Robot>();
        ItemDictionary = new();
        Doors = new ((int col, int row) position, bool isOpen)[Map.TotalDoorCount];
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
              Doors[CharToInt(c)] = (pos, false);
          }
          else if (Map.ItemCharArray.Contains(c)) 
          { 
              ItemDictionary.Add(c, (pos, false));
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
        UpdateRobots(completeCharMap);
        UpdatePlayerMovement(completeCharMap);
        AddVisibleCharMapToFrame(completeCharMap);
        UpdateMenuPrompts();
    }

    private void UpdateMenuPrompts()
    {
        MenuPrompt = null;
        foreach (var ch in SortImmediateToEnd(GetDefaultMapCharsNextToPlayer()))
        {
            int ci = CharToInt(ch);
            if (ci != -1)
            {
              UpdateDoorPrompt(ci);
              continue;
            }

            if (Map.ItemCharArray.Contains(ch) && !ItemDictionary[ch].isPickedUp)
            {
                UpdateItemPrompt(ch);
                continue;
            }

            if (Map.SpecialCharArray.Contains(ch))
            {
                UpdateSpecialInteractablePrompt(ch);
            }
        }
        
        if (FrameNum >= Program.TargetFramesPerSecond * 2 && !IsFlashlightActive)
            MenuPrompt = new GameplayMenuPrompt(
                "It seems the lights have gone out.",
                ["Activate flashlight"],
                _ =>
                {
                    IsFlashlightActive = true;
                },
                Spacebar
            );
        MenuPrompt?.Update();
    }

    private void UpdateDoorPrompt(int ci)
    {
         if (Map.FirstAccessibleDoors.Contains(ci))
         {
         
             MenuPrompt = new GameplayMenuPrompt(
                 "You have the key to this door.", 
                 [Doors[ci].isOpen ? "Close door" : "Open door"],
                 _ =>
                 { 
                     ToggleDoor(ci);
                 }, 
                 Spacebar
                 );
             return;
         }
         switch (ci) 
         { 
             case 3: 
             { 
                 if (!Doors[ci].isOpen) 
                 { 
                     if (ItemDictionary[Map.CrowBarChar].isPickedUp) 
                         MenuPrompt = new GameplayMenuPrompt(
                             "You can try to pry the door open with your crowbar.", 
                             ["Pry open door"],
                             _ =>
                             { 
                                 Doors[ci].isOpen = true;
                             }, 
                             Spacebar
                             );
                     else 
                         MenuPrompt = new GameplayMenuPrompt(
                             "This door is locked, but the lock seems fairly weak. " + 
                             "You can't kick it down because the door opens towards you.", 
                             [], 
                             _ => { }
                             );
                 } 
                 break;
             }
         }
    }

    private void UpdateItemPrompt(char ch)
    {
        MenuPrompt = new GameplayMenuPrompt(
            Map.GetPromptForItem(ch), 
            ["Pick it up"],
            _ =>
            { 
                ItemDictionary[ch] = 
                    ItemDictionary[ch] with { isPickedUp = true };
            }, 
            Spacebar
        );
    }

    private void UpdateSpecialInteractablePrompt(char ch)
    {
        
    }
    
    private void UpdateRobots(char [,] completeCharMap)
    {
        foreach (var robot in RobotList)
        {
           robot.UpdateMovement(completeCharMap, PlayerPosition); 
        }
    }

    private void AddVisibleCharMapToFrame(char[,] completeCharMap)
    {
        bool areShipLightsOn = AreShipLightsOn();
        List<(int col, int row)> allLitUp = GetAllInFloodFillRange(
                areShipLightsOn ? ShipLightsFloodFillRange : FlashlightFloodFillRange, completeCharMap);
        allLitUp = FilterForInFlashLightAbsoluteRange(
            allLitUp, areShipLightsOn ? ShipLightsAbsoluteDistanceRange : FlashlightAbsoluteDistanceRange);
        for (int row = 0; row < Map.Height; row++)
        {
            for (int col = 0; col < Map.Width; col++)
            {
                if (
                    completeCharMap[col, row] == '@' || 
                    (allLitUp.Contains((col, row)) && (areShipLightsOn || IsFlashlightActive))
                )
                    Program.Frame += completeCharMap[col, row];
                else
                    Program.Frame += ' ';
            }

            Program.Frame += '\n';
        }
    }

    private bool AreShipLightsOn()
        => (FrameNum < Program.TargetFramesPerSecond * 1.5 && FrameNum % 2 == 0)
           || FrameNum < Program.TargetFramesPerSecond;


    private List<(int col, int row)> GetAllInFloodFillRange(int reiterations, char[,] charMap)
    {
        List<(int col, int row)> results = []; // all non-permeable points visible as result of flashlight
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
        => [
            position with { row = position.row + 1},
            position with { row = position.row - 1},
            position with { col = position.col + 1},
            position with { col = position.col - 1}
        ];

    private List<(int col, int row)> FilterForInFlashLightAbsoluteRange(
        List<(int col, int row)> positionList, double range
    ) {
        List<(int col, int row)> results = new();
        foreach (var position in positionList)
        {
            if (
                range >=
                Math.Sqrt(Square(PlayerPosition.row - position.row) + Square(PlayerPosition.col - position.col))
            ) {
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
        
        foreach (var item in ItemDictionary)
            if (!item.Value.isPickedUp)
                result[item.Value.position.col, item.Value.position.row] = item.Key;
        foreach (var specialInteractable in SpecialInteractableList)
            result[specialInteractable.position.col, specialInteractable.position.row] 
                = specialInteractable.character;
        for (var i = 0; i < Doors.Length; i++)
        {
            var door = Doors[i];
            if (door.isOpen)
                result[door.position.col, door.position.row]
                    = Map.IsDoorVertical(i) ? OpenVerticalDoorRenderChar : OpenHorizontalDoorRenderChar;
            else
                result[door.position.col, door.position.row]
                    = Map.IsDoorVertical(i) ? ClosedVerticalDoorRenderChar : ClosedHorizontalDoorRenderChar;
        }
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

    internal static bool IsEmptyOrPermeable(char c)
        => c == ' ' || c == RobotRenderChar || c == PlayerRenderChar
                    || c == OpenHorizontalDoorRenderChar || c == OpenVerticalDoorRenderChar;

    private (bool immediate, char ch)[] GetDefaultMapCharsNextToPlayer()
    {
        var results = new List<(bool, char)>();
        for (int col = PlayerPosition.col - 1; col <= PlayerPosition.col + 1; col++)
        {
            for (int row = PlayerPosition.row - 1; row <= PlayerPosition.row + 1; row++)
            {
                if (PlayerPosition.col == col && PlayerPosition.row == row)
                    continue;
                if (Map.DefaultMap[row][col] != ' ')
                    results.Add((row == PlayerPosition.row || col == PlayerPosition.col, Map.DefaultMap[row][col]));
            }
        }
        return results.ToArray();
    }

    private void ToggleDoor(int i)
        => Doors[i] = Doors[i] with { isOpen = !Doors[i].isOpen};

    private char[] SortImmediateToEnd((bool immediate, char ch)[] list)
    {
        var resultPartOne = new List<char>();
        var resultPartTwo = new List<char>();
        foreach (var item in list)
        {
            if (item.immediate) resultPartTwo.Add(item.ch);
            else resultPartOne.Add(item.ch);
        }
        return resultPartOne.Concat(resultPartTwo).ToArray();
    }
}
