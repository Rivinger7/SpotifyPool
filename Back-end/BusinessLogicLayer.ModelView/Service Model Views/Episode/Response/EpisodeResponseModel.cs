using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.ReleaseInfo.Response;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Episode.Response;

public class EpisodeResponseModel
{
    public string? Id { get; set; }
    public string? PodcastId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AudioUrl { get; set; }
    public double Duration { get; set; }
    public ReleaseInfoResponseModel? ReleaseInfo { get; set; }
    public List<ImageResponseModel> Images { get; set; } = new List<ImageResponseModel>();
    public object? Restriction { get; set; }
    public int StreamCount { get; set; }
    public int DownloadCount { get; set; }
    public int FavoriteCount { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime LastUpdatedTime { get; set; }
}