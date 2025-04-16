namespace AITakeOverEscape;

internal class GameplayMenuPrompt 
{
    private string CompletePrompt { get; set; } = String.Empty;
    private Action<int> OnOptionSelected { get; }
    private (ConsoleKey? key, string? text) AlternateFirstOptionKey { get; }
    private int OptionCount { get; }
    
    internal GameplayMenuPrompt (
        string promptStart, string[] optionArray, Action<int> onOptionSelection
    ) {
        OnOptionSelected = onOptionSelection;
        SetCompletePrompt(promptStart, optionArray);
        OptionCount = optionArray.Length;
    }

    internal GameplayMenuPrompt (
        string prompt, string[] optionArray, Action<int> onOptionSelection, 
        (ConsoleKey key, string text) alternateFirstOptionKey
    ) {
        OnOptionSelected = onOptionSelection;
        AlternateFirstOptionKey = alternateFirstOptionKey;
        SetCompletePrompt(prompt, optionArray);
        OptionCount = optionArray.Length;
    }
    
    internal void Update()
    {
        Program.Frame += CompletePrompt;
        foreach (ConsoleKey key in Program.PressedKeys)
        {
            int num = GetKeyAsNumerical(key);
            if ((num > 0 && num <= OptionCount && AlternateFirstOptionKey.key == null) 
                || (num >= 0 && num < OptionCount && AlternateFirstOptionKey.key != null))
                OnOptionSelected.Invoke(num);
        }
    }

    private int GetKeyAsNumerical(ConsoleKey key)
    {
        if (key == (AlternateFirstOptionKey.key ?? ConsoleKey.D0)) return 0;
        if (key == ConsoleKey.D1) return 1;
        if (key == ConsoleKey.D2) return 2;
        if (key == ConsoleKey.D3) return 3;
        if (key == ConsoleKey.D4) return 4;
        if (key == ConsoleKey.D5) return 5;
        if (key == ConsoleKey.D6) return 6;
        if (key == ConsoleKey.D7) return 7;
        if (key == ConsoleKey.D8) return 8;
        if (key == ConsoleKey.D9) return 9;
        return -1;
    }

    private void SetCompletePrompt(string promptStart, string[] optionArray)
    {
        CompletePrompt += "\n\n   " + promptStart + "\n\n\n";
        for (int i = 0; i < optionArray.Length; i++)
        {
            CompletePrompt += "     ";
            if (AlternateFirstOptionKey.key != null)
            {
                if (i == 0)
                    CompletePrompt += AlternateFirstOptionKey.text;
                else
                    CompletePrompt += i;
            }
            else
                CompletePrompt += i + 1;
            CompletePrompt += "     --     " + optionArray[i] + "\n";
        }
        CompletePrompt += "\n";
    }
}