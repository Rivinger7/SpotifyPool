using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer.DBContext
{
    public class MongoDBSetting
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
