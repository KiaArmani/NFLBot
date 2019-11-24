using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XurBlitzer.Services;

namespace XurBlitzer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlitzController : ControllerBase
    {
        private readonly BlitzMissionService _blitzMissionService;
        private readonly MongoService _mongoService;

        public BlitzController(MongoService mongoService, BlitzMissionService blitzMissionService)
        {
            _mongoService = mongoService;
            _blitzMissionService = blitzMissionService;
        }

        // GET api/Blitz/completed/5
        [HttpGet("completed/{id}")]
        public ActionResult<string> GetCompletionStatus(long id)
        {
            var resultDictionary = new Dictionary<string, bool>();

            var hasCompletedCurrentBlitzMission =
                _mongoService.HasCompletedBlitzMission(id, _blitzMissionService.CurrentActiveMissionStart,
                    _blitzMissionService.CurrentActiveMission);
            resultDictionary.Add("currentBlitzMissionDone", hasCompletedCurrentBlitzMission);

            return JsonConvert.SerializeObject(resultDictionary);
        }

        // GET api/Blitz/currentmission
        [HttpGet("currentmission")]
        public ActionResult<string> GetCurrentMission()
        {
            return JsonConvert.SerializeObject(_blitzMissionService.CurrentActiveMission);
        }

        // GET api/Blitz/currentmissionend
        [HttpGet("currentmissionend")]
        public ActionResult<string> GetCurrentMissionEnd()
        {
            return JsonConvert.SerializeObject(_blitzMissionService.CurrentActiveMissionEnd);
        }
    }
}