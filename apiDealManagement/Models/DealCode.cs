﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apiDealManagement.Models
{
    public partial class DealCode
    {
        [Key]
        public int id { get; set; }
        public int? deal_supplier_id { get; set; }
        public int? deal_customer_id { get; set; }
        public int? deal_id { get; set; }
        public string deal_quatation { get; set; }
        public string deal_quatation_file { get; set; }
        public string deal_quatation_link { get; set; }
        public int? deal_quatation_id { get; set; }
        public string deal_coupon_code { get; set; }
        public string deal_reference { get; set; }
        public string remark { get; set; }
        public int? requested_by { get; set; }
        public DateTime? deal_start_date { get; set; }
        public DateTime? deal_end_date { get; set; }
        public DateTime? requested_at { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
