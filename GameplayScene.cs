namespace AITakeOverEscape;

internal class GameplayScene : IScene
{
    private const char PlayerRenderChar = '@';
    private const char RobotRenderChar = '&';
    private const char DeadRobotRenderChar = 'X';
    private const char ClosedVerticalDoorRenderChar = '-'; // vertical as in a door where one travels vertically
    private const char OpenVerticalDoorRenderChar = '/';
    private const char ClosedHorizontalDoorRenderChar = '|';
    private const char OpenHorizontalDoorRenderChar = '_';
    private const double PlayerMovesPerSecond = 5;
    private const int FlashlightFloodFillRange = 6;
    private const double FlashlightAbsoluteDistanceRange = 4;
    private const int ShipLightsFloodFillRange = 12;
    private const double ShipLightsAbsoluteDistanceRange = 8;
    private const int CaptainMinAge = 25;
    private const int CaptainMaxAge = 45;
    private const double BucketAttackRange = 2.5;
    private const double CrowBarAttackRange = 1;
    private const double SmashedRobotInteractRange = 1.5;
    
    private char?[,] CharMap { get; }
    private (int col, int row) PlayerPosition { get; set; }
    private double PlayerTimeSinceLastMove { get; set; }
    private List<Robot> RobotList { get; }
    private uint FrameNum { get; set; }
    private GameplayMenuPrompt? MenuPrompt { get; set; }
    private bool IsFlashlightActive { get; set; }
    private bool IsCaptainsLogOpen { get; set; }
    private Dictionary<char, ((int col, int row) position, bool isPickedUp)> ItemDictionary { get; }
    private ((int col, int row) position, bool isOpen)[] Doors { get; }
    private List<(char character, (int col, int row) position)> SpecialInteractableList { get; }
    private int CaptainAge { get; } = CaptainMinAge + Random.Shared.Next(CaptainMaxAge - CaptainMinAge + 1);
    private static int?[] CurrentPasscodeEntered { get; set; } = new int?[4];
    private bool IsBucketFilled { get; set; }
    private bool HasBucketBeenUsed { get; set; }
    private (int col, int row)? SplashedRobotPosition { get; set; }
    private (int col, int row)? SmashedRobotPosition { get; set; }

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
                x =>
                {
                    if (x == -1)
                        IsFlashlightActive = true;
                },
                "Activate flashlight"
            );

        if (IsBucketFilled && GetClosestRobotToPlayer()?.DistanceTo(PlayerPosition) <= BucketAttackRange)
        {
            MenuPrompt = new GameplayMenuPrompt(
                "You can splash water on the robot.",
                x =>
                {
                    if (x == -1)
                    {
                        SplashedRobotPosition = GetClosestRobotToPlayer()!.Position;
                        RobotList.Remove(GetClosestRobotToPlayer()!);
                        IsBucketFilled = false;
                        HasBucketBeenUsed = true;
                    }
                },
                "Throw your bucket"
            );
        }

        if (ItemDictionary[Map.CrowBarChar].isPickedUp
            && GetClosestRobotToPlayer()?.DistanceTo(PlayerPosition) <= CrowBarAttackRange)
        {
            MenuPrompt = new GameplayMenuPrompt(
                "You can attack the robot with your crowbar.",
                x =>
                {
                    if (x == -1)
                    {
                        SmashedRobotPosition = GetClosestRobotToPlayer()!.Position;
                        RobotList.Remove(GetClosestRobotToPlayer()!);
                        ItemDictionary[Map.CrowBarChar] = ItemDictionary[Map.CrowBarChar] with { isPickedUp = false };
                    }
                },
                "Throw your crowbar"
            );
        }

        if (
            SmashedRobotPosition != null
            && Math.Sqrt(Square(PlayerPosition.col - SmashedRobotPosition.Value.col)
                         + Square(PlayerPosition.row - SmashedRobotPosition.Value.row)) <= SmashedRobotInteractRange
        ) {
            MenuPrompt = new GameplayMenuPrompt(
                "You may attempt to pull out the crowbar for future use.", 
                x =>
                {
                    if (x == -1)
                        Program.Scene = new DeathScene(
                            "You just got electrocuted from touching the crowbar while it's in the robot.");
                },
                "Take it out"
            );
        }
        
        MenuPrompt?.Update();
    }

    private void UpdateDoorPrompt(int ci)
    {
         if (Map.FirstAccessibleDoors.Contains(ci))
         {
         
             MenuPrompt = new GameplayMenuPrompt(
                 "You have the key to this door.", 
                 x =>
                 {
                     if (x == -1)
                         ToggleDoor(ci);
                 }, 
                 Doors[ci].isOpen ? "Close door" : "Open door"
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
                             x =>
                             { 
                                 if (x == -1)
                                    Doors[ci].isOpen = true;
                             }, 
                             "Pry open door"
                         );
                     else 
                         MenuPrompt = new GameplayMenuPrompt(
                             "This door is locked, but the lock seems fairly weak. " + 
                             "You can't kick it down because the door opens towards you.", 
                             _ => { }
                             );
                 } 
                 break;
             }
             case 4:
             {
                 if (Doors[ci].isOpen)
                 {
                     MenuPrompt = new GameplayMenuPrompt(
                         "This door can be closed, but to open it again the code would be required.",
                         x =>
                         {
                             if (x == -1)
                                 Doors[ci].isOpen = false;
                         },
                         "Close door"
                     );
                 }
                 else
                 {
                     MenuPrompt = new GameplayMenuPrompt(
                         "This door is locked electronically; the code was set by the captain himself." +
                         "\n\n\n" +
                         $"{Program.Tab+Program.Tab}ENTER PASSCODE: {GetEnteredPasscodeAsString()}",
                         x =>
                         {
                             if (x == -1)
                             {
                                 if (IsEnteredPasscodeCorrect())
                                     Doors[ci].isOpen = true;
                                 CurrentPasscodeEntered = new int?[CurrentPasscodeEntered.Length];
                             }
                             else if (x == 10)
                             {
                                 RemoveFromEnteredPasscode();
                             }
                             else
                             {
                                 AddNumberToEnteredPasscode(x);
                             }
                         },
                         "Authenticate entered passcode"
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
            x =>
            { 
                if (x == -1)
                    ItemDictionary[ch] = ItemDictionary[ch] with { isPickedUp = true };
            }, 
            "Pick it up"
        );
    }

    private void UpdateSpecialInteractablePrompt(char ch)
    {
        switch (ch)
        {
            case Map.CaptainsBodyChar:
            {
                MenuPrompt = new GameplayMenuPrompt(
                    "That's the captain's body! Yeah... he's dead.",
                    _ => { }
                );
                break;
            }
            case Map.CaptainsLogChar:
            {
                if (IsCaptainsLogOpen)
                {
                    MenuPrompt = new GameplayMenuPrompt(
                        "           The following is the most recent entry from the captain's log." +
                        "\n\n\n" +
                        "   26 April, 2050" +
                        "\n\n" +
                        "       The robots have been acting strange ever since I gave them that software update,\n" +
                        "   but that won't distract me from the fact that it's my birthday. " +
                        $"I'm {CaptainAge} years old now!",
                        _ => { }
                    );
                }
                else
                {
                    MenuPrompt = new GameplayMenuPrompt(
                        "This appears to be the captain's log.",
                        x =>
                        {
                            if (x == -1)
                                IsCaptainsLogOpen = true;
                        },
                        "Open it"
                    );
                }

                break;
            }
            case Map.SinkChar:
            {
                if (ItemDictionary[Map.BucketChar].isPickedUp)
                {
                    if (!IsBucketFilled && HasBucketBeenUsed)
                    {
                        MenuPrompt = new GameplayMenuPrompt(
                            "It seems the water isn't working now.",
                            _ => { }
                        );
                        break;
                    }

                    if (IsBucketFilled && !HasBucketBeenUsed)
                    {
                        MenuPrompt = new GameplayMenuPrompt(
                            "You're bucket is filled.",
                            _ => { }
                        );
                        break;
                    }
                        
                    // the bucket will never be filled while its also been used
                    // so this is only reached when the bucket isn't filled nor used
                    MenuPrompt = new GameplayMenuPrompt(
                        "It's the sink. You can fill up your bucket with water now.",
                        x =>
                        {
                            if (x == -1)
                            {
                                IsBucketFilled = true;
                            }
                        },
                        "Fill it up"
                    );
                }
                else
                {
                    MenuPrompt = new GameplayMenuPrompt(
                        "It's the sink that produces infamously salty water;" +
                        " you don't seem to have a use for it at the moment.",
                        _ => { }
                    );
                }
                break;
            }
        }
    }
    
    private void UpdateRobots(char [,] completeCharMap)
    {
        foreach (var robot in RobotList)
        {
           robot.UpdateMovement(completeCharMap, PlayerPosition);
           if (robot.Position == PlayerPosition)
               Program.Scene = new DeathScene("You just got killed by a robot!");
        }

        if (PlayerPosition == SplashedRobotPosition)
            Program.Scene = new DeathScene("You just died by electrocution!");
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

        if (SplashedRobotPosition != null)
            result[SplashedRobotPosition.Value.col, SplashedRobotPosition.Value.row] = DeadRobotRenderChar;
        if (SmashedRobotPosition != null)
            result[SmashedRobotPosition.Value.col, SmashedRobotPosition.Value.row] = DeadRobotRenderChar;
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
        => c == ' ' || c == RobotRenderChar || c == PlayerRenderChar || c == DeadRobotRenderChar
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

    private int GetCaptainBirthYear()
        => 2050 - CaptainAge;
    
    private int?[] GetCorrectPassCode()
        => [2,0,(GetCaptainBirthYear()-2000)/10,(GetCaptainBirthYear()-2000)%10];

    private string GetEnteredPasscodeAsString()
    {
        string s = string.Empty;
        foreach (var i in CurrentPasscodeEntered)
            s += i is null ? "_" : i.ToString();
        return s;
    }

    private void AddNumberToEnteredPasscode(int number)
    {
        for (int i = 0; i < CurrentPasscodeEntered.Length; i++)
        {
            if (CurrentPasscodeEntered[i] == null)
            {
                CurrentPasscodeEntered[i] = number;
                return;
            }
        }
    }

    private bool IsEnteredPasscodeCorrect()
    {
        var correct = GetCorrectPassCode();
        for (var i = 0; i < correct.Length; i++)
        {
            if (correct[i] != CurrentPasscodeEntered[i])
                return false;
        }

        return true;
    }

    private void RemoveFromEnteredPasscode()
    {
        for (int i = CurrentPasscodeEntered.Length - 1; i >= 0; i--)
            if (CurrentPasscodeEntered[i] != null)
            {
                CurrentPasscodeEntered[i] = null;
                return;
            }
    }

    private Robot? GetClosestRobotToPlayer()
    {
        if (RobotList.Count == 0)
            return null;
        
        Robot closestRobot = RobotList[0];
        double minDistance = double.MaxValue;

        foreach (var robot in RobotList)
        {
            if (robot.DistanceTo(PlayerPosition) < minDistance)
            {
                minDistance = robot.DistanceTo(PlayerPosition);
                closestRobot = robot;
            }
        }
        return closestRobot;
    }
}
