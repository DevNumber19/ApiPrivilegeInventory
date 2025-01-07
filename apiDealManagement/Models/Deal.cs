using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apiDealManagement.Models
{
    public partial class Deal
    {
        [Key]
        public int id { get; set; }
        public int? deal_supplier_id { get; set; }
        public int? deal_customer_id { get; set; }
        public string deal_name { get; set; }
        public string remark { get; set; }
        public int? status { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_at { get; set; }
        public int? deal_coupon_type { get; set; }
        public decimal? deal_coupon_value { get; set; }
        public int? deal_permission { get; set; }
        public string deal_quatation { get; set; }
        public string deal_quatation_link { get; set; }
        public int? deal_merchant_id { get; set; }
        public string deal_merchant_name { get; set; }
        public string deal_invoice { get; set; }
        public string deal_receipt { get; set; }
        public DateTime? deal_start_date { get; set; }
        public DateTime? deal_end_date { get; set; }
        public string deal_coupon_name { get; set; }
        public decimal? deal_cost { get; set; }
        public string deal_condition { get; set; }
        public string deal_coupon_image { get; set; }
        public string deal_major { get; set; }
        public decimal? deal_discount_value { get; set; }  
        public int? deal_summary_value { get; set; }  
        public string deal_add_item { get; set; }
        public int? deal_add_amount { get; set; }  
        public decimal? deal_add_value { get; set; }
        public string deal_pr_number { get; set; }
        public string deal_pr_file { get; set; }
        public int? minimum_requested { get; set; }
        public int? maximum_requested { get; set; }
        public int? minimum_order { get; set; }
        public int? maximum_order { get; set; }
        public int? deal_quatation_id { get; set; }
        public int? deal_receipt_id { get; set; }
        public int? deal_coupon_image_id { get; set; }
        public int? deal_pr_file_id { get; set; }
        public int? deal_invoice_file_id { get; set; }
    }
}
