using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apiDealManagement.Models
{
    public partial class DealUser
    {
        [Key]
        public int id { get; set; }
        public int? user_id { get; set; }
        public int? is_admin { get; set; }
        public string token { get; set; }
        public int? status { get; set; }
        public string created_by { get; set; }
        public DateTime? created_at { get; set; }
        public string updated_by { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
