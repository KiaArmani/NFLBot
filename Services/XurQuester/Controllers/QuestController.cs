using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XurClassLibrary.Models;
using XurQuester.Services;

namespace XurQuester.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public QuestController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // GET api/Quest/challenges/{id}
        [HttpGet("challenges/{id}")]
        public ActionResult<string> Get(long id)
        {
            var resultDictionary = new Dictionary<string, bool>();

            foreach (var challenge in ChallengeGlobals.WeeklyChallengeList)
            {
                var hasCompletedChallenge =
                    _mongoService.HasCompletedChallenge(id, challenge.Week, challenge.Tier, challenge.Difficulty);
                resultDictionary.Add($"tier{challenge.Tier}{challenge.Difficulty}", hasCompletedChallenge);
            }

            return JsonConvert.SerializeObject(resultDictionary);
        }

        // GET api/Quest/scores/5
        [HttpGet("score/{id}")]
        public ActionResult<long> GetPlayerScore(long id)
        {
            return _mongoService.GetPlayerScore(id);
        }


        // GET api/Quest/score/clan
        [HttpGet("score/clan")]
        public ActionResult<long> GetClanScore(long id)
        {
            return _mongoService.GetClanScore();
        }

        // GET api/Quest/challenges
        [HttpGet("challenges")]
        public ActionResult<string> GetChallengeInformation()
        {
            return JsonConvert.SerializeObject(ChallengeGlobals.WeeklyChallenges);
        }

        // GET api/Quest/stats/kills
        [HttpGet("stats/kills")]
        public ActionResult<long> GetClanKills()
        {
            return _mongoService.GetClanKills();
        }
    }
}