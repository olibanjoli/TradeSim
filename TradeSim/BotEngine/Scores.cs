using Discord.WebSocket;

namespace TradeSim.BotEngine;

public class Scores
{
    private static readonly object Locker = new object();

    public List<ScoreEntry> ScoreEntries { get; set; } = new();

    public void UpdateScore(ulong discordId, string name, double points)
    {
        lock (Locker)
        {
            var entry = ScoreEntries.FirstOrDefault(p => p.DiscordId == discordId);

            if (entry == null)
            {
                entry = new ScoreEntry
                {
                    DiscordId = discordId,
                    Name = name,
                };

                ScoreEntries.Add(entry);
            }

            entry.Points += points;

            if (points == 0) return;
            
            entry.TradeCounter += 1;
                
            if (points > 0)
            {
                entry.Wins += 1;
            }
            else
            {
                entry.Losses += 1;
            }
        }
    }

    public void Reset()
    {
        lock (Locker)
        {
            ScoreEntries.Clear();
        }
    }

    public void SetScore(SocketGuildUser user, double points, bool countTrade = true)
    {
        lock (Locker)
        {
            var entry = ScoreEntries.FirstOrDefault(p => p.DiscordId == user.Id);

            if (entry == null)
            {
                entry = new ScoreEntry
                {
                    DiscordId = user.Id,
                    Name = user.Username,
                };

                ScoreEntries.Add(entry);
            }

            entry.Points = points;

            if (countTrade)
            {
                entry.TradeCounter += 1;
            }
        }
    }
}