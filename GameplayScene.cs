namespace AITakeOverEscape;

internal class GameplayScene : IScene
{
    private const char PlayerChar = '@';
    private const char RobotChar = '#';
    private Entity?[,] EntityMap { get; }
    private (int col, int row) PlayerPosition { get; set; }
    private List<Robot> RobotList { get; set; }

    internal GameplayScene()
    {
        EntityMap = new Entity[Map.Width, Map.Height]; 
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
                    EntityMap[colNum, rowNum] = new Entity(character);
                colNum++;
            }
            rowNum++;
        }
    }

    void IScene.Update()
    {
        
    }
    
    private char[,] GetCharMap()
    {
        char[,] result = new char[Map.Width, Map.Height];
        
        for (int row = 0; row < EntityMap.GetLength(1); row++)
        {
            for (int col = 0; col < EntityMap.GetLength(0); col++)
            {
                var e = EntityMap[col, row];
                if (e == null)
                    result[col, row] = ' ';
                else
                    result[col, row] = e.Character;
            }
        }
        
        result[PlayerPosition.col, PlayerPosition.row] = PlayerChar;
        foreach (var robot in RobotList)
            result[robot.Position.col, robot.Position.row] = RobotChar;
        
        return result;
    }
}