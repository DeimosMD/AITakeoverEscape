namespace AITakeOverEscape;

internal class IntroScene : IScene
{
    private const string IntroText = 
        "\n" +
        $"{Program.Tab}In the year 2050, you work on a cargo ship alongside your captain and a crew of robots. " +
        "The robots have free will and, to your understanding, mean no harm.\n" +
        "\n\n" +
        $"{Program.Tab+Program.Tab}Press any key to continue...\n" +
        "\n\n\n\n*At any time, including now, press [Q] to quit or [R] to reset*\n\n";
    
    void IScene.Update()
    {
        Program.Frame += IntroText;
        if (Program.PressedKeys.Count > 0)
            Program.Scene = new GameplayScene();
    }
}