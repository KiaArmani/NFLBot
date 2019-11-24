using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using XurClassLibrary.Models;
using XurNightfaller.Services;

namespace XurNightfaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NightfallsController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public NightfallsController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // GET /api/Nightfalls/scores/position/{nightfallid}
        [HttpGet("scores/position/{nightfallid}")]
        public ActionResult<long> GetPositionOfScore(long instanceId)
        {
            return _mongoService.GetPositionOfScore(instanceId);
        }

        // GET /api/Nightfalls/scores/top/{topX}
        [HttpGet("scores/top/{topX}")]
        public ActionResult<List<ScoreEntry>> GetTopOrdealScores(int topX)
        {
            return _mongoService.GetTopOrdealScores(topX);
        }
    }
}