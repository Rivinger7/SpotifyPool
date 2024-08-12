using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Models
{
    public class CustomerModel
    {
        //public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phonenumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Birthdate { get; set; }
        public string? Image { get; set; }
        public string? Gender { get; set; }
        public string Status { get; set; }

    }
}
