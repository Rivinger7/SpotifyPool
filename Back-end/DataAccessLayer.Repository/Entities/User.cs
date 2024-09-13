﻿using MongoDB.Bson;
using AspNetCore.Identity.MongoDbCore.Models;

namespace DataAccessLayer.Repository.Entities
{
    public class User : MongoIdentityUser<ObjectId>
    {
        public string? FullName { get; set; }
        public string Role { get; set; } = null!;
        public DateOnly? Birthdate { get; set; }
        public string? Gender { get; set; }
        public string Status { get; set; } = null!;
        public string? TokenEmailConfirm { get; set; }
        public bool? IsLinkedWithGoogle { get; set; } = null;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Image Object
        public string? Image { get; set; }
        public string? ImageType { get; set; }
    }
}