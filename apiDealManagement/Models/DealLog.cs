using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apiDealManagement.Models
{
    public partial class DealLog
    {
        [Key]
        public int id { get; set; }
        public int action { get; set; }
        public int type { get; set; }
        public int user { get; set; }
        public int deal_id { get; set; }
        public int? deal_request_code { get; set; }
        public string deal_reference { get; set; }
        public int qty_code { get; set; }
        public int? bill_id { get; set; }
        public string remark { get; set; }
        public string link_file { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? requested_at { get; set; }
    }
}
