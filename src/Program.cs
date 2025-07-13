using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main( string[] args )
    {
        string? token = args.Length > 0 ? args[0] : null;

        while( true ) // Wait for a valid bot TOKEN
        {
            if( string.IsNullOrWhiteSpace( token ) )
            {
                Console.Write( "Input a valid bot TOKEN" );
                token = Console.ReadLine();
                continue;
            }

            DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(
                token,
                DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            );

            try
            {
                builder.ConfigureEventHandlers
                (
                    b => b.HandleMessageCreated(MessageCreatedHandler)
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

    private static async Task MessageCreatedHandler( DiscordClient s, MessageCreatedEventArgs e )
    {
        if( e.Message.Content.ToLower().StartsWith( "hello" ) )
        {
            await e.Message.RespondAsync( "world!" );
        }
    }
}
