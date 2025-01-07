using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace apiDealManagement.Models
{
    public partial class InvoiceOrderItem
    {
        [Key]
        public int id { get; set; }
        public int? product_id { get; set; }
        public int? inv_order { get; set; }
        public string created_by { get; set; }
        public DateTime? created_at { get; set; }
        public string updated_by { get; set; }
        public DateTime? updated_at { get; set; }
    }
}