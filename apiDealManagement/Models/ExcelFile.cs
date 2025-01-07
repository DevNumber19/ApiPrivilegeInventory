using System;
using System.Collections.Generic;

#nullable disable

namespace apiDealManagement.Models
{
    public class ExcelFileViewModel
    {
        public byte[] content { get; set; }
        public string contentType { get; set; }
        public string fileName { get; set; }
    }
}
