namespace Data_Access_Layer.Entities
{
    public class Artist
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public DateOnly Birthdate { get; set; }
        public string Description { get; set; }
    }
}
