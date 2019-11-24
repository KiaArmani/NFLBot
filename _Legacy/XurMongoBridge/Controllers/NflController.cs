using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using XurClassLibrary.Models;
using XurMongoBridge.Services;

namespace XurMongoBridge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NflController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public NflController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        [HttpGet("scores/position/{nightfallid}")]
        public ActionResult<long> GetPositionOfScore(long instanceId)
        {
            return _mongoService.GetPositionOfScore(instanceId);
        }

        [HttpGet("scores/top/{topX}")]
        public ActionResult<List<ScoreEntry>> GetTopOrdealScores(int topX)
        {
            return _mongoService.GetTopOrdealScores(topX);
        }
    }
}