using ABCD.Application;
using ABCD.Domain;
using ABCD.Lib;
using ABCD.Server.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCD.Server.Controllers
{
    [ApiController]
    [Route("api/public")]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ISeriesService _seriesService;
        private readonly ITypeMapper _typeMapper;

        public PublicController(IPostService postService, ISeriesService seriesService, ITypeMapper typeMapper)
        {
            _postService = postService;
            _seriesService = seriesService;
            _typeMapper = typeMapper;
        }

        [HttpGet("posts")]
        public async Task<IActionResult> GetPublishedPosts([FromQuery] int limit = 10, [FromQuery] int skip = 0)
        {
            var posts = await _postService.GetPublishedAsync(limit, skip);
            var response = _typeMapper.Map<IEnumerable<Post>, IEnumerable<PublicPostSummaryResponse>>(posts);
            return Ok(response);
        }

        [HttpGet("posts/{pathSegment}")]
        public async Task<IActionResult> GetPublishedPost([FromRoute] string pathSegment)
        {
            var post = await _postService.GetPublishedByPathSegmentAsync(pathSegment);
            if (post == null)
                return NotFound();
            var response = _typeMapper.Map<Post, PublicPostDetailResponse>(post);
            return Ok(response);
        }

        [HttpGet("series")]
        public async Task<IActionResult> GetPublishedSeries([FromQuery] int limit = 10, [FromQuery] int skip = 0)
        {
            var seriesList = await _seriesService.GetPublishedAsync(limit, skip);
            var response = _typeMapper.Map<IEnumerable<Series>, IEnumerable<PublicSeriesSummaryResponse>>(seriesList);
            return Ok(response);
        }

        [HttpGet("series/{pathSegment}")]
        public async Task<IActionResult> GetPublishedSeriesByPath([FromRoute] string pathSegment)
        {
            var series = await _seriesService.GetPublishedByPathSegmentAsync(pathSegment);
            if (series == null)
                return NotFound();
            var response = _typeMapper.Map<Series, PublicSeriesDetailResponse>(series);
            return Ok(response);
        }
    }
}
