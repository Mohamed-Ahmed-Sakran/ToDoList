﻿using System.ComponentModel.DataAnnotations;

namespace ToDoList.Core.Dtos
{
    public class UpdateUserDto
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        public string LastName { get; set; }
    }
}
