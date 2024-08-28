using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.ModelView.Models
{
    public class AuthenticatedResponseModel
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
