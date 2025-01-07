using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace apiDealManagement.Models
{
    public partial class Invoice
    {
        [Key]
        public int no { get; set; }
        public string shop_id { get; set; }
        public string attention { get; set; }
        public string name_company { get; set; }
        public string address { get; set; }
        public string tax_id { get; set; }
        public string project { get; set; }
        public DateTime? date { get; set; }
        public DateTime? price_validity { get; set; }
        public string package { get; set; }
        public string payment_term { get; set; }
        public string revise { get; set; }
        public string remark { get; set; }
        public int? qty { get; set; }
        public double? unit_price { get; set; }
        public double? total { get; set; }
        public double? total_vat7 { get; set; }
        public string created_by { get; set; }
        public DateTime? created_at { get; set; }
        public string updated_by { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}