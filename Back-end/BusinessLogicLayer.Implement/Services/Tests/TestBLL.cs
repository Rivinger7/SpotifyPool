using AutoMapper;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Implement.Services.Tests
{
    public class TestBLL(IUnitOfWork unitOfWork, IMapper mapper)
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<(string addedAtString, DateTime addedAtTime)> AddDayOnly()
        {
            string addedAtString = DateTime.UtcNow.AddHours(7).ToString();
            string addedAtString2 = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            DateTime addedAtTime = DateTime.UtcNow.AddHours(7);

            string dateOnly = "2024-10-07";
            

            Test test = new()
            {
                DateTimeValue = addedAtTime,
                DateOnly = DateTime.Parse(dateOnly).Date.ToUniversalTime(),
                DateTimeString = addedAtString,
                DateTimeString2 = addedAtString2
            };

            await _unitOfWork.GetCollection<Test>().InsertOneAsync(test);

            return (addedAtString, addedAtTime);
        }
    }
}
