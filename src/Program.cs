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
                await builder.ConnectAsync();

                await Task.Delay(-1);

                builder.ConfigureEventHandlers
                (
                    b => b.HandleMessageCreated( async ( s, e ) => 
                    {
                        if( e.Message.Content.ToLower().StartsWith( "ping" ) )
                        {
                            await e.Message.RespondAsync( "pong!" );
                        }
                    })
                );

                builder.ConfigureEventHandlers
                (
                    b => b.HandleMessageCreated(MessageCreatedHandler)
                );

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
