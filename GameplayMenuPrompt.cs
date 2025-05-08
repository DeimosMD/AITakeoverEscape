namespace AITakeOverEscape;

internal class GameplayMenuPrompt 
{
    private string CompletePrompt { get; } 
    private Action<int> OnOptionSelected { get; }
    private (ConsoleKey key, string text) SingleOptionSelectKey => (ConsoleKey.Spacebar, "Spacebar");
    
    internal GameplayMenuPrompt (
        string prompt, Action<int> onOptionSelection
    ) {
        OnOptionSelected = onOptionSelection;
        CompletePrompt = $"\n\n{Program.Tab}{prompt}";
    }

    // presence of an option prompt has no technical effect on the invoking of the passed function
    internal GameplayMenuPrompt (
        string prompt, Action<int> onOptionSelection, string singleOptionText 
    ) {
        OnOptionSelected = onOptionSelection;
        CompletePrompt = $"\n\n{Program.Tab}{prompt}\n\n\n" +
                         $"{Program.Tab+Program.Tab}{SingleOptionSelectKey.text}" +
                         $"{Program.Tab}--{Program.Tab}{singleOptionText}";
    }
    
    internal void Update()
    {
        Program.Frame += CompletePrompt;
        foreach (ConsoleKey key in Program.PressedKeys)
        {
            if (GetKeyAsNumerical(key) != -1 || key == SingleOptionSelectKey.key)
                OnOptionSelected.Invoke(GetKeyAsNumerical(key)); // passes -1 if is single-option key
        }
    }

    private int GetKeyAsNumerical(ConsoleKey key)
    {
        if (key == ConsoleKey.D0) return 0;
        if (key == ConsoleKey.D1) return 1;
        if (key == ConsoleKey.D2) return 2;
        if (key == ConsoleKey.D3) return 3;
        if (key == ConsoleKey.D4) return 4;
        if (key == ConsoleKey.D5) return 5;
        if (key == ConsoleKey.D6) return 6;
        if (key == ConsoleKey.D7) return 7;
        if (key == ConsoleKey.D8) return 8;
        if (key == ConsoleKey.D9) return 9;
        if (key == ConsoleKey.Backspace) return 10;
        return -1;
    }
}