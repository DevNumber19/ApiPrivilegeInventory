using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apiDealManagement.Models
{
    public partial class DealReconcile
    {
        [Key]
        public int id { get; set; }
        public int? deal_id { get; set; }
        public string invoice { get; set; }
        public string deal_reference { get; set; }
        public int? qty_code { get; set; }
        public string remark { get; set; }
        public int? status { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
