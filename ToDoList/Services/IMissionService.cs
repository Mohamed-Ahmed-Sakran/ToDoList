using ToDoList.Dtos;
using ToDoList.Models;

namespace ToDoList.Services
{
    public interface IMissionService
    {
        Task<IEnumerable<MissionDto>> GetAsync(string userId);
        Task<IEnumerable<MissionDto>> GetAsync(string userId , DateTime dueDate);
        Task<MissionDto> AddAsync(string userId , MissionDto missionDto);
        Task<MissionDto> UpdateAsync(int missionId , MissionDto missionDto);
        Task<string> DeleteAsync(int missionId);
    }
}

