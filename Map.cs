namespace AITakeOverEscape;

// stores info about map as well as deals with different char usages
internal static class Map
{
    internal const char PlayerChar = '@';
    internal const char RobotChar = '#';

    internal static char[] SpecialCharArray { get; } = [
        FlightTerminalChar, TrashDisposalChar, CaptainsLogChar, CaptainsBodyChar, SinkChar, EscapePodInitiatorChar
    ];
    internal const char FlightTerminalChar = 'K'; 
    internal const char TrashDisposalChar = '%';
    internal const char CaptainsLogChar = 'E';
    internal const char CaptainsBodyChar = 't';
    internal const char SinkChar = 'W';
    internal const char EscapePodInitiatorChar = 'Y';
    
    internal static char[] ItemCharArray { get; } = [CrowBar, ElectricalKitChar, FuelRodsChar, BucketChar];
    internal const char CrowBar = 'l';
    internal const char ElectricalKitChar = 'e';
    internal const char FuelRodsChar = '=';
    internal const char BucketChar = 'U';
    
    // all rows of map must be same length
    // player position must be set or will cause runtime exception
    // numbers represent doors
    // all else that is interactable is either an item or a special interactable
    internal static string[] DefaultMap =>
    [
        "             OOOOOOOOOOOOOOOOOOOOOOOOOOOOOO_____________O",
        "            O    O O                     UO             O",
        "           O     O O                O     O             O",
        "          O      7 O                O     O             O",
        "         OY      O O                     WO             O",
        "     OOOOOOOOOOOOO OO              OOOOOOOO%O         OOO",
        "   OO            O                                      O",
        "  O              O OOOOO OOOOOOOOOOOOO OOOOOO         OOO",
        " O               O Ol      O        EO O                O",
        "OK               6 OOOOOOOOO         O O                O",
        " O               O O       O         O O                O", 
        "  O              O O  @    O    t    O O                O", 
        "   OO            O O       O         O O                O", 
        "     OOOOOOOOOOOOO OOOO1OOOOOOOOOO4OOO                  O",
        "         Oe      O 2                 3                  O",
        "          O      5 OO   OOOOOOOOO   OO                  O",
        "           O     O O        O        O                  O",
        "            O    O O                 O                 =O",
        "             OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO"
    ];
    
    internal static int Height => DefaultMap.Length;
    internal static int Width => DefaultMap[0].Length;

    internal static bool IsDoorVertical(int id)
    {
        switch (id)
        {
            case 1: return true;
            case 2: return false;
            case 3: return false;
            case 4: return true;
            case 5: return false;
            case 6: return false;
            case 7: return false;
            default: throw new ArgumentException($"{id} is not the ID of an actual door");
        }
    }
}