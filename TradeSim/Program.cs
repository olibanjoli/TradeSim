using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using TradeSim.BotEngine;

namespace TradeSim;

public class Program
{
    private DiscordSocketClient? client;
    private CommandService? commands;
    private InteractionService? interactionService;

    public static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

    public async Task RunBotAsync()
    {
        client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents =  GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.MessageContent
        });
        
        commands = new CommandService();
        
        client.Log += LogAsync;
        client.Ready += OnReadyAsync;

        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("BOT_TOKEN"));
        await client.StartAsync();

        await Task.Delay(-1);
    }

    public async Task RegisterCommandsAsync()
    {
        client!.MessageReceived += HandleCommandAsync;
        client.ReactionAdded += HandleReactionAdded;

        await commands!.AddModulesAsync(Assembly.GetEntryAssembly(), null);
    }

    private static async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> userMessage, Cacheable<IMessageChannel, ulong> messageChannel, SocketReaction reaction)
    {
        if (reaction.User.Value.IsBot) return;

        var engine = EngineManager.Get(reaction.Channel.Id);
        var channel = await messageChannel.GetOrDownloadAsync();

        if (Equals(reaction.Emote, Settings.Long))
        {
            await engine.Long(reaction.UserId, reaction.User.Value, channel);
        }
        else if (Equals(reaction.Emote, Settings.Short))
        {
            await engine.Short(reaction.UserId, reaction.User.Value, channel);
        }
        else if (Equals(reaction.Emote, Settings.Close))
        {
            await engine.Close(reaction.UserId, reaction.User.Value, channel);
        }
        else if (Equals(reaction.Emote, Settings.Reverse))
        {
            await engine.Reverse(reaction.UserId, reaction.User.Value, channel);
        }
        else if (Equals(reaction.Emote, Settings.DoubleDown))
        {
            await engine.DoubleDown(reaction.UserId, reaction.User.Value, channel);
        }
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        
        return Task.CompletedTask;
    }

    private async Task OnReadyAsync()
    {
        Console.WriteLine($"Logged in as {client?.CurrentUser.Username}");

        interactionService = new InteractionService(client);
        
        await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        //await interactionService.RegisterCommandsGloballyAsync();
        await interactionService.RegisterCommandsToGuildAsync(1081995164173738144);

        client!.InteractionCreated += async interaction =>
        {
            var ctx = new SocketInteractionContext(client, interaction);
            await interactionService.ExecuteCommandAsync(ctx, null);
        };
        
        client.ReactionAdded += HandleReactionAdded;
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        var message = arg as SocketUserMessage;
        var context = new SocketCommandContext(client, message);
        
        if (message?.Author.IsBot ?? false) return;

        int argPos = 0;
        
        if (message.HasStringPrefix(".", ref argPos))
        {
            var result = await commands!.ExecuteAsync(context, argPos, null);
            
            if (!result.IsSuccess)
            {
                Console.WriteLine(result.ErrorReason);
            
                await message.ReplyAsync(result.ErrorReason);
            }
        }
    }
}