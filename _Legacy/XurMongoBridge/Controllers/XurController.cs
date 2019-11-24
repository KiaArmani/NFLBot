using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XurClassLibrary.Models;
using XurMongoBridge.Services;

namespace XurMongoBridge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class XurController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public XurController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // GET api/Xur/challenges/5
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

        // GET api/Xur/scores/5
        [HttpGet("score/{id}")]
        public ActionResult<long> GetPlayerScore(long id)
        {
            return _mongoService.GetPlayerScore(id);
        }


        // GET api/Xur/clanscore
        [HttpGet("clanscore")]
        public ActionResult<long> GetClanScore(long id)
        {
            return _mongoService.GetClanScore();
        }

        // GET api/Xur/challenges
        [HttpGet("challenges")]
        public ActionResult<string> GetChallengeInformation()
        {
            return JsonConvert.SerializeObject(ChallengeGlobals.WeeklyChallenges);
        }

        // GET api/Xur/clankills
        [HttpGet("clankills")]
        public ActionResult<long> GetClanKills()
        {
            return _mongoService.GetClanKills();
        }
    }
}