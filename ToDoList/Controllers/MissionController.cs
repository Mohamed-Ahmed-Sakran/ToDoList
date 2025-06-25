using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using ToDoList.Core.Dtos;
using ToDoList.Core.Repositories;

namespace ToDoList.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class MissionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;
        private readonly ILogger<MissionController> logger;

        public MissionController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MissionController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            this.logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAllMission(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("There is an error in data");

            try
            {
                await IsValidUserAsync(userId);

                logger.LogInformation($"Now finished check user with id = {userId} and start getting allMission for him");

                var Missions = await _unitOfWork.missions.FindAllAsync(m => m.UserId == userId);

                var missionsDto = _mapper.Map<IEnumerable<MissionDto>>(Missions);
                return Ok(missionsDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{userId}/{missionDueDate}")]
        public async Task<IActionResult> GetAllMission(string userId, DateTime missionDueDate)
        {
            if (string.IsNullOrEmpty(userId) || missionDueDate == DateTime.MinValue)
                return BadRequest("There is an error in data");

            try
            {
                await IsValidUserAsync(userId);

                var Missions = await _unitOfWork.missions.GetMissionsByUserAndDueDateAsync(userId, missionDueDate);
                return Ok(Missions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> AddMission(string userId, [FromBody] MissionDto missionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(userId) || missionDto is null)
                return BadRequest("There is an error in data");

            try
            {
                await IsValidUserAsync(userId);

                var Mission = await _unitOfWork.missions.AddAsync(userId, missionDto);
                await _unitOfWork.CompleteAsync();
                return Ok(Mission);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
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
                await IsHaveAccessToMissionAsync(missionId);

                var Missions = await _unitOfWork.missions.UpdateAsync(missionId, missionDto);
                await _unitOfWork.CompleteAsync();

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
                await IsHaveAccessToMissionAsync(missionId);

                var Mission = await _unitOfWork.missions.DeleteAsync(m => m.Id == missionId);
                await _unitOfWork.CompleteAsync();
                return Ok(Mission);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("updateStatue/{missionId}")]
        public async Task<IActionResult> CompleteMission(int missionId)
        {
            Console.WriteLine("where is an error ?");

            if (missionId <= 0)
                return BadRequest("There is an error in data");

            try
            {
                await IsHaveAccessToMissionAsync(missionId);

                var Mission = await _unitOfWork.missions.CompleteAsync(missionId);
                await _unitOfWork.CompleteAsync();
                return Ok(Mission);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Completed/{userId}")]
        public async Task<IActionResult> GetCompletedMission(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("There is an error in data");

            try
            {
                await IsValidUserAsync(userId);

                var Missions = await _unitOfWork.missions.FindAllAsync(m => m.UserId == userId & m.IsCompleted == true);

                var missionsDto = _mapper.Map<IEnumerable<MissionDto>>(Missions);
                return Ok(missionsDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Non-Completed/{userId}")]
        public async Task<IActionResult> GetNonCompletedMission(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("There is an error in data");

            try
            {
                await IsValidUserAsync(userId);

                var Missions = await _unitOfWork.missions.FindAllAsync(m => m.UserId == userId & m.IsCompleted == false);

                var missionsDto = _mapper.Map<IEnumerable<MissionDto>>(Missions);
                return Ok(missionsDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        private async Task IsValidUserAsync(string userId)
        {
            await _unitOfWork.users.IsUserFoundAsync(userId);

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || currentUserId != userId)
            {
                logger.LogWarning("This user with id: {currentUser} is want to access another user with id: {requestUserId}",currentUserId,userId);
                throw new UnauthorizedAccessException("You can only manage your own missions");
            }
        }

        private async Task IsHaveAccessToMissionAsync(int missionId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            bool checkAccessMission = await _unitOfWork.missions.CheckAccessToMissionAsync(currentUserId, missionId);

            if (!checkAccessMission)
            {
                logger.LogWarning("This user with id: {currentUser} is want to access mission with id: {missionId} for another user",currentUserId,missionId);
                throw new Exception("Sorry! you don't have an access to this mission");
            }
        }

    }
}