using BusinessLogicLayer.ModelView.Service_Model_Views.ReleaseInfo.Response;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Episode.Request;

public class UpdateEpisodeRequestModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public IFormFile AudioFile { get; set; }
    public IFormFile ImageFile { get; set; }
    public ReleaseInfoResponseModel ReleaseInfo { get; set; }
}