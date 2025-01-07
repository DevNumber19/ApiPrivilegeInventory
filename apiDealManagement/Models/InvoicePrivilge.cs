using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace apiDealManagement.Models
{
    public partial class InvoicePrivilge
    {
        [Key]
        public int id { get; set; }
        public DateTime? receive_date { get; set; }
        public DateTime? redemption_date { get; set; }
        public string order_id { get; set; }
        public double? order_total { get; set; }
        public double? unit_price { get; set; }
        public int? quantity { get; set; }
        public int? inv_order_id { get; set; }
        public string created_by { get; set; }
        public DateTime? created_at { get; set; }
        public string updated_by { get; set; }
        public DateTime? updated_at { get; set; }
    }
}