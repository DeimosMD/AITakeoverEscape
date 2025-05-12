namespace AITakeOverEscape;

public class DeathScene (string text) : IScene
{
    void IScene.Update()
    {
        Program.Frame += $"\n{Program.Tab}{text}\n\n\n{Program.Tab}{Program.Tab}Press [Q] to quit or [R] to reset.";
    }
}