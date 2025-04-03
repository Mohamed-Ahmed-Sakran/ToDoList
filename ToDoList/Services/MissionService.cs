using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToDoList.Data;
using ToDoList.Dtos;
using ToDoList.Models;

namespace ToDoList.Services
{
    public class MissionService : IMissionService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MissionService(UserManager<AppUser> userManager, AppDbContext context, IMapper mapper)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MissionDto>> GetAsync(string userId)
        {
            if (userId is null)
                throw new ArgumentException("There is an error in data"); ;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("There is no user with this userId");

            var missions = await _context.Missions.Where(x => x.UserId == userId).ToListAsync();

            var missionsDto = _mapper.Map<IEnumerable<MissionDto>>(missions);

            return missionsDto;
        }

        public async Task<IEnumerable<MissionDto>> GetAsync(string userId, DateTime dueDate)
        {
            if (userId is null || dueDate == DateTime.MinValue)
                throw new ArgumentException("There is an error in data"); ;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("There is no user with this userId");

            var missions = _context.Missions.Where(x => x.UserId == userId && x.DueDate.Date == dueDate.Date).ToList();

            var missionsDto = _mapper.Map<IEnumerable<MissionDto>>(missions);

            return missionsDto;
        }

        public async Task<MissionDto> AddAsync(string userId, MissionDto missionDto)
        {
            if (missionDto is null || userId is null)
                throw new ArgumentException("There is an error in data"); ;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("There is no user with this userId");

            var mission = new Mission()
            {
                Title = missionDto.Title,
                Description = missionDto.Description,
                DueDate = missionDto.DueDate,
                UserId = userId,
            };

            await _context.Missions.AddAsync(mission);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                
                return missionDto;
            }

            throw new Exception("There is an error when add");
        }

        public async Task<MissionDto> UpdateAsync(int missionId , MissionDto missionDto)
        {
            if (missionDto is null || missionId == 0)
                throw new ArgumentException("There is an error in data"); ;

            var mission = await _context.Missions.FindAsync(missionId);
            if (mission is null)
                throw new ArgumentException("There is no mission with this missionId");

            mission.Title = missionDto.Title;
            mission.Description = missionDto.Description;
            mission.DueDate = missionDto.DueDate;
            mission.IsCompleted = missionDto.IsCompleted;

            _context.Missions.Update(mission);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return missionDto;

            throw new Exception("There is an error when updating");
        }

        public async Task<string> DeleteAsync(int missionId)
        {
            if (missionId == 0)
                throw new ArgumentException("There is an error in data"); ;

            var mission = await _context.Missions.FindAsync(missionId);
            if (mission == null)
                throw new ArgumentException("There is no mission with this missionId");

            _context.Missions.Remove(mission);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return "Deleted successful";

            throw new Exception("There is an error when updating");
        }
    }
}
