using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;

using R34Sharp;

public static class CommandRule34
{
    private static readonly R34ApiClient R34Client = new();

    private static readonly R34Sharp.Models.R34FormattedTag[] ExcludedTags = [
        new( "video" ),
        new( "animated" ),
    ];

    public static async Task<bool> OnMessage( DiscordClient s, MessageCreatedEventArgs e )
    {
        if( !e.Message.Content.ToLower().StartsWith( "r34" ) )
            return false;

        R34Sharp.Models.R34FormattedTag[] Tags = [.. e.Message.Content
            .Split(' ')
            .Skip(1)
            .Select( tag => new R34Sharp.Models.R34FormattedTag( tag ) )
        ];

        if( Tags.Length <= 0 )
        {
            await e.Message.RespondAsync("You have to speficy some tags separated by spaces");
            return true;
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
            return true;
        }

        R34Sharp.Entities.Posts.R34Post[] Posts = [.. postsResponse.Data.Where( post => !post.HasTags( ExcludedTags ) ) ];

        if( Posts.Length <= 0 ) // -TODO See how to display videos and gifs
        {
            await e.Message.RespondAsync("Couldn't find a post with these tags x[");
            return true;
        }

        Random rnd = new();

        R34Sharp.Entities.Posts.R34Post RandomPost = Posts[ rnd.Next(0, Posts.Length - 1 ) ];

        await e.Message.RespondAsync( new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder{
                Title = $"Score: {RandomPost.Score}",
                Url = RandomPost.FileUrl,
                ImageUrl = RandomPost.FileUrl,
                Color = new ( 170, 229, 164 ),
                Timestamp = RandomPost.CreatedAt
            }.Build() )
        );

        return true;
    }
}
