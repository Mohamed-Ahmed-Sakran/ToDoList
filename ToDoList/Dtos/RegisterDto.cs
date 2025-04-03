using System.ComponentModel.DataAnnotations;

namespace ToDoList.Dtos
{
    public class RegisterDto
    {
        [Required,StringLength(50)]
        public string FirstName { get; set; }

        [Required,StringLength(50)]
        public string LastName { get; set; }

        [Required,StringLength(50)]
        public string UserName { get; set; }

        [Required,StringLength(256)]
        public string Password { get; set; }

        [Required, StringLength(128)]
        public string Email { get; set; }
    }
}
