namespace AITakeOverEscape;

internal class IntroScene : IScene
{
    private const string IntroText = 
        "\n" +
        "   In the year 2050, you work on a cargo ship alongside your captain and a crew of robots. " +
        "They have free will but, to your understanding, mean no harm.\n" +
        "\n\n" +
        "       Press any key to continue...\n";
    
    void IScene.Update()
    {
        Program.Frame += IntroText;
        if (Program.PressedKeys.Count > 0)
            Program.Scene = new GameplayScene();
    }
}