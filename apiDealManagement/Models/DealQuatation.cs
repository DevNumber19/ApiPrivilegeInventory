using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apiDealManagement.Models
{
    public partial class DealQuatation
    {
        [Key]
        public int id { get; set; }
        public string deal_quatation { get; set; }
        public string deal_quatation_file { get; set; }
        public string deal_quatation_link { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
    }
}
