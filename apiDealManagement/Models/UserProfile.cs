using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace apiDealManagement.Models
{
    public partial class UserProfile
    {
        //[Key]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int id { get; set; }
        public int emp_id { get; set; }
        public int? finger_print_id { get; set; }
        public string username { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string nick_name { get; set; }
        public string email { get; set; }
        public string phone_number { get; set; }
        public string department { get; set; }
        public string position { get; set; }
        public string user_office { get; set; }
        public byte[] blob_image { get; set; }
        public int? is_admin { get; set; }
        public string token { get; set; }
        public DateTime? created_at { get; set; }
        public string created_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string updated_by { get; set; }
        public int? enabled { get; set; }
        public string initial_name { get; set; }
        public string account { get; set; }

    }
}