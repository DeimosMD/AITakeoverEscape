using System.Diagnostics;

namespace AITakeOverEscape;

public static class Program
{
    private const ConsoleKey ExitKey = ConsoleKey.Q; // when pressed, exits program
    private const double TargetFramesPerSecond = 30;
    
    private static List<ConsoleKey> PressedKeys { get; set; } = new(); // all keys pressed during most recent frame
    private static string Frame { get; set; } = string.Empty; // text to be outputted for current frame
    
    public static void Main(string[] args)
    {
        if (args.Length != 0)
            Console.WriteLine($"All given {args.Length} argument(s) are unnecessary.");

        while (!PressedKeys.Contains(ExitKey))
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            RunUpdates();
            stopWatch.Stop();
            var sleepTimeSeconds = Sleep(stopWatch.Elapsed.TotalSeconds);
            Frame = string.Empty;
            Frame += $"FPS: {Math.Round(1 / (sleepTimeSeconds + stopWatch.Elapsed.TotalSeconds))}\n\n";
        }
    }

    private static void RunUpdates()
    {
        UpdatePressedKeys();
        Console.Clear();
        Console.WriteLine(Frame);
    }

    private static void UpdatePressedKeys()
    { 
        PressedKeys = new(); 
        while (Console.KeyAvailable) 
            PressedKeys.Add(Console.ReadKey(true).Key);
    }

    // sleeps required time to maintain target FPS then returns time slept in seconds
    private static double Sleep(double updateTimeSeconds)
    {
       double sleepTimeSeconds = 1 / TargetFramesPerSecond - updateTimeSeconds;
       if (sleepTimeSeconds > 0)
       {
           Thread.Sleep((int) (sleepTimeSeconds * 1000));
           return sleepTimeSeconds;
       }

       return 0;
    }
}