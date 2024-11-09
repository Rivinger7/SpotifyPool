using AutoMapper;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response
{
    public class UserResponseModel
    {
        public required string UserId { get; set; }
        public required string Role { get; set; }
        public required string Email { get; set; }
        public required string DisplayName { get; set; }
        public string? Gender { get; set; }
        public string? Birthdate { get; set; }
        public string? Image { get; set; }
        public bool? IsLinkedWithGoogle { get; set; }
        public required string Status { get; set; }

        public required string CreatedTime { get; set; }
        public string? LastLoginTime { get; set; }
        public string? LastUpdatedTime { get; set; }
    }
}
