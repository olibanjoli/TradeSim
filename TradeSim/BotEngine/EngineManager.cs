namespace TradeSim.BotEngine;

public static class EngineManager
{
    private static readonly Dictionary<ulong, Engine> Engines = new();

    public static Engine Get(ulong channel)
    {
        if (Engines.TryGetValue(channel, out var engine1))
        {
            return engine1;
        }

        var engine = new Engine();
        
        Engines.Add(channel, engine);

        return engine;
    }
}