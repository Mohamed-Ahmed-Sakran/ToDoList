using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToDoList.Core.Dtos;
using ToDoList.Core.Models;
using ToDoList.Core.Repositories;
using ToDoList.EF.Data;

namespace ToDoList.EF.RepositoriesImplementation
{
    public class MissionRepository : BaseRepository<Mission>, IMissionRepository
    {
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _userManager;

        private readonly IMapper _mapper;

        public MissionRepository(AppDbContext context, UserManager<AppUser> userManager, IMapper mapper) : base(context)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }
        public async Task<IEnumerable<Mission>> GetMissionsByUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("Invalid user ID");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("No user found with this ID");

            return await FindAllAsync(m => m.UserId == userId);
        }

        public async Task<IEnumerable<MissionDto>> GetMissionsByUserAndDueDateAsync(string userId, DateTime dueDate)
        {
            if (string.IsNullOrEmpty(userId) || dueDate == DateTime.MinValue)
                throw new ArgumentException("Invalid user ID or due date");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("No user found with this ID");

            var missions = await FindAllAsync(
                m => m.UserId == userId && m.DueDate.Date == dueDate.Date
            );

            if(missions is null || !missions.Any())
                throw new ArgumentException("There is no data");

            return _mapper.Map<IEnumerable<MissionDto>>(missions);

        }

        public async Task<MissionDto> AddAsync(string userId, MissionDto missionDto)
        {
            if (missionDto is null || userId is null)
                throw new ArgumentException("There is an error in data"); ;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("There is no user with this userId");

            var mission = _mapper.Map<Mission>(missionDto);
            mission.UserId = userId;

            await AddAsync(mission);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return missionDto;
            }

            throw new Exception("There is an error when add");
        }

        public async Task<MissionDto> UpdateAsync(int missionId, MissionDto missionDto)
        {
            if (missionDto is null || missionId == 0)
                throw new ArgumentException("There is an error in data"); ;

            var mission = await _context.Missions.FindAsync(missionId);
            if (mission is null)
                throw new ArgumentException("There is no mission with this missionId");

            _mapper.Map(missionDto,mission);
            
            var result = Update(mission);

            if (result > 0)
                return missionDto;

            throw new Exception("There is an error while updating");
        }

        public async Task<bool> CheckAccessToMissionAsync(string userId,int missionId)
        {
            if (string.IsNullOrEmpty(userId) || missionId <= 0)
                throw new ArgumentException("Invalid user ID or mission Id");

            var accessCheckToMission = await _context.Missions.AsNoTracking()
                .AnyAsync(m => m.Id == missionId && m.UserId == userId);

            return accessCheckToMission;
        }

        public async Task<Mission> CompleteAsync(int missionId)
        {
            var mission = await _context.Missions
               .FindAsync(missionId);

            mission.IsCompleted = true;

            Update(mission);

            return mission;
        }
    }
}
