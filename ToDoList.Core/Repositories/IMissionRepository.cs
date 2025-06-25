using System;
using ToDoList.Core.Dtos;
using ToDoList.Core.Models;

namespace ToDoList.Core.Repositories
{
    public interface IMissionRepository : IBaseRepository<Mission>
    {
        Task<IEnumerable<Mission>> GetMissionsByUserAsync(string userId);
        Task<IEnumerable<MissionDto>> GetMissionsByUserAndDueDateAsync(string userId, DateTime dueDate);
        Task<MissionDto> AddAsync(string userId, MissionDto missionDto);
        Task<MissionDto> UpdateAsync(int missionId, MissionDto missionDto);
        Task<bool> CheckAccessToMissionAsync(string userId, int missionId);
        Task<Mission> CompleteAsync(int missionId);
    }
}
