namespace ToDoList.Core.Models
{
    public class Mission
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
