using System;
using System.Collections.Generic;
using ToDoList.Core.Models;

namespace ToDoList.Core.Dtos
{
    public class UserDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public List<MissionDto>? Missions { get; set; }

        public DateTime? BlockUntil { get; set; }
    }
}
