using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;

using System;
using System.Threading.Tasks;

using R34Sharp;

class Program
{
    private static readonly R34ApiClient R34Client = new();

    private static readonly R34Sharp.Models.R34FormattedTag[] ExcludedTags = [
        new( "video" ),
        new( "animated" ),
    ];

    static async Task Main(string[] args)
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
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static async Task MessageCreatedHandler( DiscordClient s, MessageCreatedEventArgs e )
    {
        if( !e.Message.Content.ToLower().StartsWith( "r34" ) )
            return;

        R34Sharp.Models.R34FormattedTag[] Tags = [.. e.Message.Content
            .Split(' ')
            .Skip(1)
            .Select( tag => new R34Sharp.Models.R34FormattedTag( tag ) )
        ];

        if( Tags.Length <= 0 )
        {
            await e.Message.RespondAsync("You have to speficy some tags separated by spaces");
            return;
        }

        Task<R34Sharp.Entities.Posts.R34Posts> getPostTask = R34Client.Posts.GetPostsAsync(
            new R34Sharp.Search.R34PostsSearchBuilder{
                Limit = 1000,
                Tags = Tags
            }
        );

        R34Sharp.Entities.Posts.R34Posts? postsResponse = await getPostTask;

        if( postsResponse.Count <= 0 )
        {
            await e.Message.RespondAsync("Couldn't find a post with these tags x[");
            return;
        }

        R34Sharp.Entities.Posts.R34Post[] Posts = [.. postsResponse.Data.Where( post => !post.HasTags( ExcludedTags ) ) ];

        if( Posts.Length <= 0 ) // -TODO See how to display videos and gifs
        {
            await e.Message.RespondAsync("Couldn't find a post with these tags x[");
            return;
        }

        Random rnd = new();

        R34Sharp.Entities.Posts.R34Post RandomPost = Posts[ rnd.Next(0, Posts.Length - 1 ) ];

        await e.Message.RespondAsync( new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder{
                Title = "r34",
                Url = RandomPost.FileUrl,
                ImageUrl = RandomPost.FileUrl/*,
                Description = RandomPost.TagsString*/
            }.Build() )
        );
    }
}
