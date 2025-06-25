using System.ComponentModel.DataAnnotations;

namespace ToDoList.Core.Dtos
{
    public class MissionDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }

        public string TimeRemaining
        {
            get
            {
                if (DueDate.HasValue)
                {
                    var time = DueDate.Value - DateTime.Now;
                    return $"{time.Days} days, {time.Hours} hours, {time.Minutes} minutes";
                }
                return "No due date specified";
            }
        }
    }
}
