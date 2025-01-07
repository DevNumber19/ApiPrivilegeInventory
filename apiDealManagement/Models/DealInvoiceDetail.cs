using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apiDealManagement.Models
{
    public partial class DealInvoiceDetail
    {
        [Key]
        public int id { get; set; }
        public string inv_no { get; set; }
        public int? deal_id { get; set; }
        public string item_detail { get; set; }
        public int? quantity { get; set; }
        public decimal? price { get; set; }
        public string option_selected { get; set; }
        public string remark { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
