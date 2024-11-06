using AutoMapper;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response
{
    public class UserResponseModel
    {
        public required string UserId { get; set; }
        public string? Role { get; set; }
        public string? DisplayName { get; set; }
        public string? Gender { get; set; }
        public string? Birthdate { get; set; }
        public string? Image { get; set; }
        public bool? IsLinkedWithGoogle { get; set; }
        public string? Status { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastUpdatedTime { get; set; }
    }
}
