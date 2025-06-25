﻿using Microsoft.EntityFrameworkCore;

namespace ToDoList.Core.Models
{
    [Owned]
    public class RefreshToken
    {
        public string Token {  get; set; }
        public DateTime ExpireOn { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpireOn;
        public DateTime CreateOn { get; set; }
        public DateTime? RevokedOn { get; set; }
        public bool IsActive => RevokedOn is null && !IsExpired;
    }
}
