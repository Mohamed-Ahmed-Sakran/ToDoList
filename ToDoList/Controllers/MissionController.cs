using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToDoList.Dtos;
using ToDoList.Services;

namespace ToDoList.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class MissionController : ControllerBase
    {
        private readonly IMissionService _missionService;

        public MissionController(IMissionService missionService)
        {
            _missionService = missionService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAllMission(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("There is an error in data");

            try
            {
                var Missions = await _missionService.GetAsync(userId);
                return Ok(Missions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{userId}/{missionDueDate}")]
        public async Task<IActionResult> GetAllMission(string userId , DateTime missionDueDate)
        {
            if (string.IsNullOrEmpty(userId) || missionDueDate == DateTime.MinValue)
                return BadRequest("There is an error in data");

            try
            {
                var Missions = await _missionService.GetAsync(userId,missionDueDate);
                return Ok(Missions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> AddMission(string userId , [FromBody] MissionDto missionDto)
        {
            if (string.IsNullOrEmpty(userId) || missionDto is null)
                return BadRequest("There is an error in data");

            try
            {
                var Missions = await _missionService.AddAsync(userId,missionDto);
                return Ok(Missions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{missionId}")]
        public async Task<IActionResult> UpdateMission(int missionId, [FromBody] MissionDto missionDto)
        {
            if (missionId <= 0 || missionDto is null)
                return BadRequest("There is an error in data");

            try
            {
                var Missions = await _missionService.UpdateAsync(missionId, missionDto);
                return Ok(Missions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{missionId}")]
        public async Task<IActionResult> DeleteMission(int missionId)
        {
            if (missionId <= 0)
                return BadRequest("There is an error in data");

            try
            {
                var Missions = await _missionService.DeleteAsync(missionId);
                return Ok(Missions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
