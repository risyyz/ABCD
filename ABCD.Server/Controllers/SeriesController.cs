using ABCD.Application;
using ABCD.Domain;
using ABCD.Lib;
using ABCD.Server.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCD.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SeriesController : ControllerBase
    {
        private readonly ISeriesService _seriesService;
        private readonly ITypeMapper _typeMapper;

        public SeriesController(ISeriesService seriesService, ITypeMapper typeMapper)
        {
            _seriesService = seriesService;
            _typeMapper = typeMapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSeriesRequest payload)
        {
            var command = new CreateSeriesCommand(payload.Title, payload.Path, payload.Description);
            var series = await _seriesService.CreateSeriesAsync(command);
            var response = _typeMapper.Map<Series, SeriesDetailResponse>(series);
            return CreatedAtAction(nameof(Create), new { seriesId = series.SeriesId?.Value }, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var seriesList = await _seriesService.GetAllAsync();
            var response = _typeMapper.Map<IEnumerable<Series>, IEnumerable<SeriesSummaryResponse>>(seriesList);
            return Ok(response);
        }

        [HttpGet("{seriesId:int}")]
        public async Task<IActionResult> GetById([FromRoute] int seriesId)
        {
            var series = await _seriesService.GetByIdAsync(seriesId);
            if (series == null)
                return NotFound();

            var response = _typeMapper.Map<Series, SeriesDetailResponse>(series);
            return Ok(response);
        }

        [HttpPut("{seriesId:int}")]
        public async Task<IActionResult> Update([FromRoute] int seriesId, [FromBody] SeriesUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Version))
                return BadRequest("Invalid request or missing version.");

            var command = new UpdateSeriesCommand(seriesId, request.Title, request.Description, request.PathSegment, request.Version);
            var result = await _seriesService.UpdateSeriesAsync(command);
            var response = _typeMapper.Map<Series, SeriesDetailResponse>(result);
            return Ok(response);
        }

        [HttpPost("{seriesId:int}/status")]
        public async Task<IActionResult> ToggleStatus([FromRoute] int seriesId, [FromBody] ToggleSeriesStatusRequest request)
        {
            var command = new ToggleSeriesStatusCommand(seriesId, request.Version);
            var result = await _seriesService.ToggleSeriesStatusAsync(command);
            var response = _typeMapper.Map<Series, SeriesDetailResponse>(result);
            return Ok(response);
        }

        [HttpPost("{seriesId:int}/posts")]
        public async Task<IActionResult> AddPost([FromRoute] int seriesId, [FromBody] AddPostToSeriesRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Version))
                return BadRequest("Invalid request or missing version.");

            var command = new AddPostToSeriesCommand(seriesId, request.PostId, request.Position, request.Version);
            var result = await _seriesService.AddPostToSeriesAsync(command);
            var response = _typeMapper.Map<Series, SeriesDetailResponse>(result);
            return Ok(response);
        }

        [HttpDelete("{seriesId:int}/posts")]
        public async Task<IActionResult> RemovePost([FromRoute] int seriesId, [FromBody] RemovePostFromSeriesRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Version))
                return BadRequest("Invalid request or missing version.");

            var command = new RemovePostFromSeriesCommand(seriesId, request.PostId, request.Version);
            var result = await _seriesService.RemovePostFromSeriesAsync(command);
            var response = _typeMapper.Map<Series, SeriesDetailResponse>(result);
            return Ok(response);
        }
    }
}
