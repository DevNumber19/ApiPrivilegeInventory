using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apiDealManagement.Models
{
    public partial class DealSupplier
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string link { get; set; }
        public int? major { get; set; }
        public string doc { get; set; }
        public int? file_vat_20 { get; set; }
        public int? file_certificate { get; set; }
        public int? file_bookbank { get; set; }
        public int? file_accept { get; set; }
        public string remark { get; set; }
        public int status { get; set; }
        public string contact_name { get; set; }
        public string contact_phone { get; set; }
        public string contact_email { get; set; }
        public int? type { get; set; }
        public int? amount_major { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}
