﻿namespace ToDoList.Core.Dtos
{
    public class AuthDto
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; } = false;
        public bool IsRegister { get; set; } = false;
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
