using System;
using System.Collections.Generic;

#nullable disable

namespace apiDealManagement.Models
{
    public partial class ReportTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Query { get; set; }
        public string QueryCount { get; set; }
        public string FilteredFieldList { get; set; }
        public string FilteredPlaceHolder { get; set; }
        public string DisplayColumnWidth { get; set; }
        public string DetailQuery { get; set; }
        public string DetailQueryCount { get; set; }
        public string KeyField { get; set; }
        public int? Weight { get; set; }
        public byte? IsActive { get; set; }
        public byte? IsAdmin { get; set; }
        public byte? IsUser { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
