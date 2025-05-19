namespace AITakeOverEscape;

// stores info about map as well as deals with different char usages
internal static class Map
{
    internal const char PlayerChar = '@';
    internal const char RobotChar = '&';

    internal static char[] SpecialCharArray { get; } = [
        FlightTerminalChar, TrashDisposalChar, CaptainsLogChar, CaptainsBodyChar, SinkChar, EscapePodInitiatorChar
    ];
    internal const char FlightTerminalChar = 'K'; 
    internal const char TrashDisposalChar = '%';
    internal const char CaptainsLogChar = 'E';
    internal const char CaptainsBodyChar = 't';
    internal const char SinkChar = 'W';
    internal const char EscapePodInitiatorChar = 'Y';
    
    internal static char[] ItemCharArray { get; } = [CrowBarChar, ElectricalKitChar, FuelRodsChar, BucketChar];
    internal const char CrowBarChar = 'l';
    internal const char ElectricalKitChar = 'e';
    internal const char FuelRodsChar = '=';
    internal const char BucketChar = 'U';
    
    internal static (int row, int colOne, int colTwo) TrashDisposalOuterDoorPosition { get; } = (0, 43, 55);
    internal static (int row, int colOne, int colTwo) TrashDisposalInnerDoorPosition { get; } = (5, 45, 53);
    
    internal static int[] FirstAccessibleDoors { get; } = [0, 1, 2, 7];
    internal const int TotalDoorCount = 8;
    internal static char[] TransparentCharArray { get; } = 
        ['F', 'E', 'L', 'H', '[', ']', '<', 'Y', 'B', 'W', 'U', 'G', 'K'];
    internal static (int col, int row) TrashDisposalPos { get; } = (43, 5);

    // all rows of map must be same length
    // player position must be set or will cause runtime exception
    // numbers represent doors
    // all else that is interactable is either an item or a special interactable
    internal static string[] DefaultMap =>
    [
        "             OOOOOOOOOOOOOOOOOOOOOOOOOOOOOO             O",
        "            OOOOOOOO&                    UO             O",
        "           O    GO O   [H]  [#]     O B   O             O",
        "          O< Y   6 O    H           O B  GO             O",
        "         OOO    GO O   [H]  [#]          WO             O",
        "     OOOOOOOOOOOOO OO              OOOOOOOO%O         OOO",
        "   OOOOOOOOOOOOOOO                                      O",
        "  OOOO        MRDO OOOOO7OOOOOOOOOOOOO OOOOOO       # OOO",
        " OOOO  <]  <]    O Ol      OF     [ EO O&           #   O",
        "OOOOK    &       5 OOOOOOOOO &      LO O # ##  ###      O",
        " OOOO  <]  <]    O OH]    LO         O O ####  ###  # # O", 
        "  OOOO        MRDO O  @    O    t    O O  #     ##  # # O", 
        "   OOOOOOOOOOOOOOO OF      OD        O O    ##    ##  ##O", 
        "     OOOOOOOOOOOOO OOOO0OOOOOOOOOO3OOO     ###   ###  ##O",
        "         Oe     FO 1                 2     ###   ### ## O",
        "          O##    4 OO   OOOOOOOOO   OO         #        O",
        "           O#  ##O O       FO        O  #### ###   ##  &O",
        "            OOOOOOOOF H]      [H]  [HO #####   #  ####&=O",
        "             OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO"
    ];
    
    internal static int Height => DefaultMap.Length;
    internal static int Width => DefaultMap[0].Length;

    internal static bool IsDoorVertical(int id)
    {
        switch (id)
        {
            case 0: return true;
            case 1: return false;
            case 2: return false;
            case 3: return true;
            case 4: return false;
            case 5: return false;
            case 6: return false;
            case 7: return true;
            default: throw new ArgumentException($"{id} is not the ID of an actual door. ");
        }
    }

    internal static string GetPromptForItem(char ch)
    {
        switch (ch)
        {
            case CrowBarChar:
                return "There's a crowbar on a shelf.";
            case ElectricalKitChar:
                return "There's an unopened electrical kit here.";
            case FuelRodsChar:
                return "There are some fuel rods stored over here.";
            case BucketChar:
                return "There's an empty bucket on the floor.";
            default: throw new ArgumentException($"'{ch}' is not the char of an actual item. ");
        }
    }
    
    internal static bool IsOnMap((int col, int row) pos)
        => pos is { col: >= 0, row: >= 0 } 
           && pos.col < Width 
           && pos.row < Height; 
    
}