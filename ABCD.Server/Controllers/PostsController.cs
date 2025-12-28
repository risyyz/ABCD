using ABCD.Application;
using Microsoft.AspNetCore.Mvc;

namespace ABCD.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        public PostsController(IPostService postService)
        {
            _postService = postService;
        }
        // Methods will be added here later
    }
}
