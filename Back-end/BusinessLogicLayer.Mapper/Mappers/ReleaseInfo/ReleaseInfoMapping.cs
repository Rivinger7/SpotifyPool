using AutoMapper;
using BusinessLogicLayer.ModelView.Service_Model_Views.ReleaseInfo.Response;

namespace BusinessLogicLayer.Mapper.Mappers.ReleaseInfo;

public class ReleaseInfoMapping : Profile
{
    public ReleaseInfoMapping()
    {
        CreateMap<DataAccessLayer.Repository.Entities.ReleaseInfo, ReleaseInfoResponseModel>().ReverseMap();
    }
}