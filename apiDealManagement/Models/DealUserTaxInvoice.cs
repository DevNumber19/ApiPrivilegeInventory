using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace apiDealManagement.Models
{
    public partial class DealUserTaxInvoice
    {
        [Key]
        public int id { get; set; }
        public int? file_id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
        public string postcode { get; set; }
        public int? status { get; set; }
        public string inv_no { get; set; }
        public decimal? total_price { get; set; }
        public decimal? vat7percent { get; set; }
        public decimal? total_price_without_tax { get; set; }
        public string remark { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public int? updated_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string tracking_no { get; set; }
        public DateTime? tracking_date { get; set; }
        public byte[] blob_image_invoice { get; set; }
    }
}
