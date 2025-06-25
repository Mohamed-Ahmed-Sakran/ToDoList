using System.Text.Json.Serialization;

namespace ToDoList.Core.Dtos
{
    public class LoginResponseDto
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; } = false;
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }

        [JsonIgnore]
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
