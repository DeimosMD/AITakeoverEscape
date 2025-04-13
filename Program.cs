using System.Diagnostics;

namespace AITakeOverEscape;

public static class Program
{
    private const ConsoleKey ExitKey = ConsoleKey.Q; // when pressed, exits program
    private const double TargetFramesPerSecond = 30;
    
    internal static string Frame { get; set; } = string.Empty; // text to be outputted for current frame
    internal static IScene Scene { get; set; } = new IntroScene();
    internal static List<ConsoleKey> PressedKeys { get; private set; } = new(); // all keys pressed during most recent frame
    internal static double DeltaTime { get; private set; } = 1;
    
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
            DeltaTime = sleepTimeSeconds + stopWatch.Elapsed.TotalSeconds;
            Frame = string.Empty;
            Frame += $"FPS: {Math.Round(1 / DeltaTime)}\n\n";
        }
    }

    private static void RunUpdates()
    {
        UpdatePressedKeys();
        Scene.Update();
        Console.Clear();
        Console.Write(Frame);
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