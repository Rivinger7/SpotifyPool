using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Podcast.Response;

public class PodcastResponseModel
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Publisher { get; set; }
    public string? Host { get; set; }
    public List<string> Categories { get; set; } = new List<string>();
    public List<ImageResponseModel> Images { get; set; } = new List<ImageResponseModel>();
    public int Popularity { get; set; }
    public int FollowersCount { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime LastUpdatedTime { get; set; }
    public List<string> Episodes { get; set; } = new List<string>();
}