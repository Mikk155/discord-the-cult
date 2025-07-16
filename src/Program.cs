using DSharpPlus;
using DSharpPlus.EventArgs;

delegate Task<bool> MessageHandlerDelegate( DiscordClient s, MessageCreatedEventArgs e );

class Program
{
#if DEBUG
    public static readonly bool isDebug = true;
#else
    public static readonly bool isDebug = false;
#endif

    static async Task Main( string[] args )
    {
        string? token = args.Length > 0 ? args[0] : null;

        while( true ) // Wait for a valid bot TOKEN
        {
            if( string.IsNullOrWhiteSpace( token ) )
            {
                Console.Write( "Input a valid bot TOKEN\n" );
                token = Console.ReadLine();
                continue;
            }

            DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(
                token,
                (
                    DiscordIntents.AllUnprivileged |
                    DiscordIntents.MessageContents |
                    DiscordIntents.GuildMembers |
                    DiscordIntents.GuildMessageReactions |
                    DiscordIntents.Guilds
                )
            );

            try
            {
                builder.ConfigureEventHandlers
                (
                    b => b.HandleMessageCreated( OnMessage )
                );

                await builder.ConnectAsync();

                await Task.Delay(-1);

                break;
            }
            catch( Exception ex )
            {
                token = null;
                Console.WriteLine( $"Error: {ex.Message}" );
            }
        }
    }

    private static readonly List<MessageHandlerDelegate> fnOnMessage =[
        CommandRule34.OnMessage
    ];

    private static async Task OnMessage( DiscordClient s, MessageCreatedEventArgs e )
    {
        foreach( MessageHandlerDelegate fn in fnOnMessage )
        {
            if( await fn(s, e) )
                break;
        }
    }
}
