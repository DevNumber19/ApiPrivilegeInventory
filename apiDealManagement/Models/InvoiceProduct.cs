using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace apiDealManagement.Models
{
    public partial class InvoiceProduct
    {
        [Key]
        public int id { get; set; }
        public string product_id { get; set; }
        public string product_name { get; set; }
        public int? status { get; set; }
        public string remark { get; set; }
        public string created_by { get; set; }
        public DateTime? created_at { get; set; }
        public string updated_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string deleted_by { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}