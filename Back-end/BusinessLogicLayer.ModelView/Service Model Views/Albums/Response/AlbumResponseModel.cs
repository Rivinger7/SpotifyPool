using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Response
{
    public class AlbumResponseModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public IEnumerable<ImageResponseModel> Images { get; set; } = [];
    }
}
