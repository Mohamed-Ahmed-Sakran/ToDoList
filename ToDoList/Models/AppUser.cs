using Microsoft.AspNetCore.Identity;

namespace ToDoList.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<Mission>? Missions { get; set; }

        public DateTime? BlockUntil { get; set; }
    }
}
