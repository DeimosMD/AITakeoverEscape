namespace AITakeOverEscape;

internal class GameplayScene : IScene
{
    private Entity?[,] EntityMap { get; }

    internal GameplayScene()
    {
        EntityMap = new Entity[Map.Width, Map.Height]; 
        var rowNum = 0; 
        foreach (var row in Map.DefaultMap) 
        { 
            var colNum = 0; 
            foreach (var character in row) 
            { 
                if (character == ' ') 
                    EntityMap[colNum, rowNum] = null;
                else 
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
        
        return result;
    }
}