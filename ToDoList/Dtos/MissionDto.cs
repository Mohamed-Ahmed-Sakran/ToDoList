using ToDoList.Models;

namespace ToDoList.Dtos
{
    public class MissionDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }

        public string TimeRemaining
        {
            get
            {
                var time = DueDate - DateTime.Now;
                return $"{time.Days} days, {time.Hours} hours, {time.Minutes} minutes";
            }
        }
    }
}
