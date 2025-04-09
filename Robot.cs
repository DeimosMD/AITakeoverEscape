namespace AITakeOverEscape;

internal class Robot(
    int col, int row
    )
{
    internal (int col, int row) Position { get; set; } = (col, row);
    internal (int col, int row) Velocity { get; set; }
}