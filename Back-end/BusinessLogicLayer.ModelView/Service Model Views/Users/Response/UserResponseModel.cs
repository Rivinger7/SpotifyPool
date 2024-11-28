namespace BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response
{
    public class UserResponseModel
    {
        public string? UserId { get; set; }
        public string? Role { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? Gender { get; set; }
        public string? Birthdate { get; set; }
        public string? Image { get; set; }
        public bool? IsLinkedWithGoogle { get; set; }
        public string? Status { get; set; }

        public string? CreatedTime { get; set; }
        public string? LastLoginTime { get; set; }
        public string? LastUpdatedTime { get; set; }
    }
}
