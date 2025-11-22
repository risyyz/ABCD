using ABCD.Server.Requests;
using ABCD.Application.Models;

public static class BlogMappingExtensions
{
    public static BlogModel ToBlogModel(this BlogUpdateRequest request)
    {
        return new BlogModel (        
            request.BlogId,
            request.Name,
            request.Description,
            request.Domains ?? new List<string>()
        );
    }
}
