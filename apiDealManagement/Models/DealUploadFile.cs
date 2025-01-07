using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apiDealManagement.Models
{
    public partial class DealUploadFile
    {
        [Key]
        public int id { get; set; }
        public string deal_file_description { get; set; }
        public string deal_file_path { get; set; }
        public string deal_file_link { get; set; }
        public int? deal_type_of_file { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
    }
}
