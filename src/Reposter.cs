// This schedule is only used on my local machine.
using DSharpPlus;
using DSharpPlus.Entities;

public class Reposter
{
    private readonly DiscordClient _client;

    private readonly string[] FolderChannels = [
        "cosplay",
        "funny",
        "furry",
        "general",
        "lesbian",
        "media",
        "shemale",
        "strange-bizarre",
        "strong-girls",
        "trap",
        "welcome",
        "yuri",
        "2dmedia",
        "ai-generated",
        "alternative",
        "asians",
        "big_files",
        "bots"
    ];

    public Reposter( DiscordClient c )
    {
        _client = c;
    }

    public void Start()
    {
        _ = Task.Run( async () =>
        {
            int iSleepTime = 0;

            while( true )
            {
                try
                {
                    iSleepTime = await CheckAndUpload();

                    if( iSleepTime == 0 )
                    {
                        Program.ReposterScheduler = null;
                        return;
                    }
                }
                catch( Exception e )
                {
                    Console.WriteLine( $"Reposter Exception: {e}" );
                }

                await Task.Delay( TimeSpan.FromMinutes( iSleepTime ) );
            }
        } );
    }

    private async Task<int> CheckAndUpload()
    {
        DiscordGuild? guild = null;

        try
        {
#if DEBUG
            guild = await _client.GetGuildAsync( 1145236064596918304 );
#else
            guild = await _client.GetGuildAsync( 1216162825307820042 );
#endif
        }
        catch( DSharpPlus.Exceptions.NotFoundException ) {
            return 0;
        }
        catch( DSharpPlus.Exceptions.ServerErrorException ) {
            return 0;
        }

        if( guild is null )
            return 0;

        string RepostDirectory = Path.Combine( Program.Workspace(), "furnace" );

        if( !Directory.Exists( RepostDirectory ) )
            return 0;

//        string SaveDirectory = Path.Combine( RepostDirectory, "big_files" );

        Random rnd = new Random();
        string[] RandomChannels = [ .. FolderChannels.OrderBy( _ => rnd.Next() ) ];

        foreach( string FolderName in RandomChannels )
        {
            string FolderPath = Path.Combine( RepostDirectory, FolderName );

            if( !Directory.Exists( FolderPath ) )
            {
                Directory.CreateDirectory( FolderPath );                
                continue;
            }

            // For now ignore webp. i may create a utils to automatically convert them x[
            string? file = Directory.EnumerateFiles( FolderPath ).Where( f => !f.EndsWith( ".webp" ) ).FirstOrDefault();

            if( file == null )
                continue;

            string FileFullPath = Path.Combine( FolderPath, file );

            FileInfo fileInfo = new( file );

            if( fileInfo.Length > 8388608 )
            {
                // Somehow this isn't working. not relevant for now
//                fileInfo.MoveTo( Path.Combine( SaveDirectory, file ) );
//                File.Move( FileFullPath, Path.Combine( SaveDirectory, file ) );
                continue;
            }

            IReadOnlyList<DiscordChannel> GuildChannels;

            try {
                GuildChannels = await guild.GetChannelsAsync();
            }
            catch( DSharpPlus.Exceptions.ServerErrorException ) {
                continue;
            }
            catch( DSharpPlus.Exceptions.NotFoundException ) {
                continue;
            }

            DiscordChannel? channel = GuildChannels.FirstOrDefault( c => c.Name.Equals( FolderName ) );

            if( channel is null )
                continue;

            using( FileStream stream = File.OpenRead( file ) )
            {
                try
                {
                    await channel.SendMessageAsync( new DiscordMessageBuilder()
                        .AddFile( Path.GetFileName( file ), stream )
                    );
                }
                catch( DSharpPlus.Exceptions.UnauthorizedException ) {}
                catch( DSharpPlus.Exceptions.ServerErrorException ) {}
            }

            File.Delete( FileFullPath );

            return 10;
        }

        Console.WriteLine( "Got no more files to repost. Destroying Reposter..." );

        return 0;
    }
}
