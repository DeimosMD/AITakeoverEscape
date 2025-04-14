namespace AITakeOverEscape;

internal class Robot(
    int startCol, int startRow
) {
    private const double MovesPerSecond = 5;
    private const int MaxPathFindingDistance = 7;
    private (int col, int row) MostRecentPosition { get; set; } = (startCol, startRow);
    private double TimeSinceLastMove { get; set; }
    internal (int col, int row) Position { get; private set; } = (startCol, startRow);
    
    internal void UpdateMovement(char?[,] charMap, (int col, int row) playerPosition)
    {
        TimeSinceLastMove += Program.DeltaTime;
        if (TimeSinceLastMove > 1 / MovesPerSecond)
        {
            TimeSinceLastMove = 0;
            if (!AttemptPathFindToPlayer(charMap, playerPosition))
                MoveAimlessly(charMap);
        }
    }

     // attempts to path-find to the player in a distance less than or equal to the max pathfinding distance
     // and with two or less direction changes
     // returns true if pathfinding was possible and move was made
    private bool AttemptPathFindToPlayer(char?[,] charMap, (int col, int row) playerPosition)
    {
        if (GetDistanceTo(playerPosition) > MaxPathFindingDistance || Position == playerPosition)
            return false;
        ((int col, int row) position, int distance)[] potentialMovesWithDistances =
        [
            (Position with { row = Position.row + 1}, CheckForPathUp(charMap, playerPosition)),
            (Position with { row = Position.row - 1}, CheckForPathDown(charMap, playerPosition)),
            (Position with { col = Position.col + 1}, CheckForPathRight(charMap, playerPosition)),
            (Position with { col = Position.col - 1}, CheckForPathLeft(charMap, playerPosition))
        ];
        int min = int.MaxValue;
        for (int i = 0; i < potentialMovesWithDistances.Length; i++)
        {
            if (!IsOnMap(potentialMovesWithDistances[i].position)) 
                potentialMovesWithDistances[i].distance = int.MaxValue;
            else if (potentialMovesWithDistances[i].distance < min)
                min = potentialMovesWithDistances[i].distance;
        }

        if (min == int.MaxValue)
            return false;
        List<(int col, int row)> bestMoves = new();
        for (int i = 0; i < potentialMovesWithDistances.Length; i++)
        {
            if (potentialMovesWithDistances[i].distance == min)
                bestMoves.Add(potentialMovesWithDistances[i].position);
        }
        Position = bestMoves[Random.Shared.Next(bestMoves.Count)];
        return true;
    }

    // returns shortest allowed distance to player from going in this direction
    // (with two or less direction changes for simplicity)
    private int CheckForPathUp(char?[,] charMap, (int col, int row) playerPosition)
    {
        if (
            playerPosition.col == Position.col && playerPosition.row > Position.row &&
            IsPathClear(charMap, playerPosition, Position)
        ) return GetDistanceTo(playerPosition); // assumed to be below max pathfinding distance
        int bestDistance = int.MaxValue;
        for (int row = Position.row+1; row < Position.row + MaxPathFindingDistance; row++)
        {
            int pathDistance = GetDistanceTo(playerPosition);
            if (row > playerPosition.row)
                pathDistance += 2 * (row - Position.row);
            if (
                pathDistance <= MaxPathFindingDistance
                && pathDistance < bestDistance
                && IsPathClear(charMap, Position, Position with { row = row })
                && IsPathClear(charMap, Position with { row = row }, playerPosition with { row = row })
                && IsPathClear(charMap, playerPosition with { row = row }, playerPosition)
            ) bestDistance = pathDistance;
        }
        
        return bestDistance;
    }
    
     private int CheckForPathDown(char?[,] charMap, (int col, int row) playerPosition)
        {
            if (
                playerPosition.col == Position.col && playerPosition.row < Position.row &&
                IsPathClear(charMap, playerPosition, Position)
            ) return GetDistanceTo(playerPosition);
            int bestDistance = Int32.MaxValue;
            for (int row = Position.row-1; row > Position.row - MaxPathFindingDistance; row--)
            {
                int pathDistance = GetDistanceTo(playerPosition);
                if (row < playerPosition.row)
                    pathDistance += 2 * (Position.row - row);
                if (
                    pathDistance <= MaxPathFindingDistance
                    && pathDistance < bestDistance
                    && IsPathClear(charMap, Position, Position with { row = row })
                    && IsPathClear(charMap, Position with { row = row }, playerPosition with { row = row })
                    && IsPathClear(charMap, playerPosition with { row = row }, playerPosition)
                ) bestDistance = pathDistance;
            }
            
            return bestDistance;
        } 
    
      private int CheckForPathRight(char?[,] charMap, (int col, int row) playerPosition)
         {
             if (
                 playerPosition.row == Position.row && playerPosition.col > Position.col &&
                 IsPathClear(charMap, playerPosition, Position)
             ) return GetDistanceTo(playerPosition);
             int bestDistance = Int32.MaxValue;
             for (int col = Position.col+1; col < Position.col + MaxPathFindingDistance; col++)
             {
                 int pathDistance = GetDistanceTo(playerPosition);
                 if (col > playerPosition.col)
                     pathDistance += 2 * (col - Position.col);
                 if (
                     pathDistance <= MaxPathFindingDistance
                     && pathDistance < bestDistance
                     && IsPathClear(charMap, Position, Position with { col = col })
                     && IsPathClear(charMap, Position with { col = col }, playerPosition with { col = col })
                     && IsPathClear(charMap, playerPosition with { col = col }, playerPosition)
                 ) bestDistance = pathDistance;
             }
             
             return bestDistance;
         }
      
     private int CheckForPathLeft(char?[,] charMap, (int col, int row) playerPosition)
        {
            if (
                playerPosition.row == Position.row && playerPosition.col < Position.col &&
                IsPathClear(charMap, playerPosition, Position)
            ) return GetDistanceTo(playerPosition);
            int bestDistance = Int32.MaxValue;
            for (int col = Position.col-1; col > Position.col - MaxPathFindingDistance; col--)
            {
                int pathDistance = GetDistanceTo(playerPosition);
                if (col < playerPosition.col)
                    pathDistance += 2 * (Position.col - col);
                if (
                    pathDistance <= MaxPathFindingDistance
                    && pathDistance < bestDistance
                    && IsPathClear(charMap, Position, Position with { col = col })
                    && IsPathClear(charMap, Position with { col = col}, playerPosition with { col = col })
                    && IsPathClear(charMap, playerPosition with { col = col }, playerPosition)
                ) bestDistance = pathDistance;
            }
            
            return bestDistance;
        } 
     
    private int GetDistanceTo((int col, int row) pos)
        => Math.Abs(pos.col - Position.col) + Math.Abs(pos.row - Position.row);
    
    // checks if all space between and including two points are empty on the entity map
    // must be directly vertical or directly horizontal
    private bool IsPathClear(char?[,] charMap, (int col, int row) posOne, (int col, int row) posTwo)
    {
        if (!IsOnMap(posOne) || !IsOnMap(posTwo))
            return false;
        if (posOne.col == posTwo.col)
        {
            for (var row = Math.Min(posOne.row, posTwo.row); row <= Math.Max(posOne.row, posTwo.row); row++)
            {
                if (charMap[posOne.col, row] != null)
                    return false;
            }
            return true;
        }
        if (posOne.row == posTwo.row)
        {
            for (var col = Math.Min(posOne.col, posTwo.col); col <= Math.Max(posOne.col, posTwo.col); col++) 
            { 
                if (charMap[col, posOne.row] != null) 
                    return false;
            }
            return true;
        }
        throw new ArgumentException();
    }

    private void MoveAimlessly(char?[,] charMap)
    {
        List<(int col, int row)> potentialMoves = [
             (Position.col + 1, Position.row), 
             (Position.col - 1, Position.row), 
             (Position.col, Position.row + 1), 
             (Position.col, Position.row - 1)
         ];
        potentialMoves = RemoveIllegalMoves(potentialMoves, MostRecentPosition, charMap);
        MostRecentPosition = Position;
        if (potentialMoves.Count == 0)
            return;
        Position = potentialMoves[Random.Shared.Next(potentialMoves.Count)];
    }
    
    // any move onto an entity on the entity map or onto the space occupied before the most recent move is illegal.
    // note that if in the most recent move call there was no possible moves and the robot stayed still, then
    // there is no way that any of the potential moves in the current move call are equal to the robot's recent position
    private List<(int col, int row)> RemoveIllegalMoves(
        List<(int col, int row)> potentialMoves,
        (int col, int row) robotLastPos,
        char?[,] charMap
        )
    {
        List<(int col, int row)> remainingMoves = new();
        foreach (var move in potentialMoves)
        {
            if (IsOnMap(move) && charMap[move.col, move.row] == null && robotLastPos != move)
                remainingMoves.Add(move);
        }
        return remainingMoves;
    }
    
    private bool IsOnMap((int col, int row) pos)
        => pos is { col: >= 0, row: >= 0 } 
           && pos.col < Map.Width 
           && pos.row < Map.Height;
}