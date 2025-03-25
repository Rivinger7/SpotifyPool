using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Podcast.Request;

public class PodcastRequestModel
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Publisher { get; set; }
    public string? Host { get; set; }
    public List<string> Categories { get; set; } = new List<string>();
    public IFormFile ImageFile { get; set; }
}