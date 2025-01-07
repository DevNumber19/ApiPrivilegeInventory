using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace apiDealManagement.Models
{
    public partial class InvoiceOrder
    {
        [Key]
        public int id { get; set; }
        public int? inv_order_id { get; set; }
        public string recipient_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string postcode { get; set; }
        public string invoice { get; set; }
        public string status { get; set; }
        public string tracking { get; set; }
        public string created_by { get; set; }
        public DateTime? created_at { get; set; }
        public string updated_by { get; set; }
        public DateTime? updated_at { get; set; }
    }
}