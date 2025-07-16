// This schedule is only used on my local machine.
using DSharpPlus;
using DSharpPlus.Entities;

public class Reposter
{
    private readonly DiscordClient _client;

    private readonly string[] FolderChannels = [
        "trusted",
        "users",
        "welcome"
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
#if DEBUG
        DiscordGuild? guild = await _client.GetGuildAsync( 1145236064596918304 );
#else
        DiscordGuild? guild = await _client.GetGuildAsync( 1216162825307820042 );
#endif

        if( guild is null )
            return 0;

        string RepostDirectory = Path.Combine( Program.Workspace(), "furnace" );

        if( !Directory.Exists( RepostDirectory ) )
            return 0;

//        string SaveDirectory = Path.Combine( RepostDirectory, "big_files" );

        foreach( string FolderName in FolderChannels )
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

            IReadOnlyList<DiscordChannel> GuildChannels = await guild.GetChannelsAsync();

            DiscordChannel? channel = GuildChannels.FirstOrDefault( c => c.Name.Equals( FolderName ) );

            if( channel is null )
                continue;

            using( FileStream stream = File.OpenRead( file ) )
            {
                await channel.SendMessageAsync( new DiscordMessageBuilder()
                    .AddFile( Path.GetFileName( file ), stream )
                );
            }

            File.Delete( FileFullPath );

            return 10;
        }

        Console.WriteLine( "Got no more files to repost. Destroying Reposter..." );

        return 0;
    }
}
