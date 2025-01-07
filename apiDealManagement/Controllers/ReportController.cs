using apiDealManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static apiDealManagement.Controllers.DealManagementController;

namespace apiDealManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : Controller
    {
        private readonly IOptions<ConnectionStringConfig> ConnectionString;
        public ReportController(IOptions<ConnectionStringConfig> ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

        [Authorize]
        [HttpGet]
        [Route("List")]
        public IActionResult List()
        {
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var pagedData = context.ReportTemplates.ToList();
                    if (pagedData == null)
                    {
                        return UnprocessableEntity(new { message = "Report Template not found" });
                    }
                    return Ok(pagedData);
                }
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.Message.ToString() });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("Preview/{reportId}")]
        public async Task<IActionResult> Preview(int reportId, [FromQuery] PaginationFilter filter)
        {
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var reportData = context.ReportTemplates.FirstOrDefault(r => r.Id == reportId && r.IsActive == 1);
                    if (reportData == null)
                    {
                        return UnprocessableEntity(new { message = "Report not found" });
                    }
                    string dateFrom = string.Empty;
                    string dateTo = string.Empty;
                    if (filter.DateFrom != null)
                    {
                        dateFrom = filter.DateFrom + " " + "00:00:00";
                    }

                    if (filter.DateTo != null)
                    {
                        dateTo = filter.DateTo + " " + "23:59:59";
                    }

                    var query = reportData.Query;
                    if (dateTo != "" && dateFrom != "")
                    {
                        query = query.Replace("##dateFrom##", "'" + dateFrom + "'").Replace("##dateTo##", "'" + dateTo + "'");
                    }

                    //if ((reportData.FilteredFieldList != null || reportData.FilteredFieldList != "") && filter.Search != null)
                    if (reportData.FilteredFieldList != null && filter.Search != null)
                        {
                        string search = filter.Search;
                        var querySearch = string.Empty;
                        var txtReplace = reportData.FilteredFieldList.Replace(" ", "");
                        //var txtReplace = (reportData.FilteredFieldList != null ? reportData.FilteredFieldList.Replace(" ", "") : null);
                        var filteredFieldListArray = txtReplace.Split(",");
                        querySearch += " and ( ";
                        for (int i = 0; i < filteredFieldListArray.Length; i++)
                        {
                            if (i == filteredFieldListArray.Length - 1)
                            {
                                querySearch += filteredFieldListArray[i] + " like '%" + search + "%' ";
                            }
                            else
                            {
                                querySearch += filteredFieldListArray[i] + " like '%" + search + "%' or ";
                            }
                        }

                        querySearch += " ) ";
                        query = query.Replace("##search##", querySearch);
                    }
                    else
                    {
                        query = query.Replace("##search##", "");

                    }

                    var retObject = new List<dynamic>();
                    using (var command = context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = query;
                        command.CommandType = CommandType.Text;
                        context.Database.OpenConnection();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var list = new ExpandoObject() as IDictionary<string, Object>;
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    var name = reader.GetName(i);
                                    var val = reader.IsDBNull(i) ? null : reader[i];
                                    list.Add(name, val);
                                }
                                retObject.Add(list);
                            }

                        }
                        var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
                        var pagedData = retObject
                                       .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                       .Take(validFilter.PageSize)
                                       .ToList();

                        var totalRecords = retObject.Count();
                        var totalPages = (int)Math.Ceiling((double)totalRecords / validFilter.PageSize);
                        if (pagedData == null)
                        {
                            return UnprocessableEntity(new { message = "ReportTemplate not found" });
                        }
                        return Ok(new PagedResponse<List<dynamic>>(pagedData, validFilter.PageNumber, validFilter.PageSize, totalPages, totalRecords));
                    }
                }
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.Message.ToString() });
            }
        }


        [Authorize]
        [HttpGet]
        [Route("Download/{reportId}")]
        public async Task<IActionResult> Download(int reportId, [FromQuery] PaginationFilter filter)
        {
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var reportData = context.ReportTemplates.FirstOrDefault(r => r.Id == reportId && r.IsActive == 1);
                    if (reportData == null)
                    {
                        return UnprocessableEntity(new { message = "Report not found" });
                    }
                    string dateFrom = string.Empty;
                    string dateTo = string.Empty;
                    if (filter.DateFrom != null)
                    {
                        dateFrom = filter.DateFrom + " " + "00:00:00";
                    }

                    if (filter.DateTo != null)
                    {
                        dateTo = filter.DateTo + " " + "23:59:59";
                    }

                    var query = reportData.Query;
                    if (dateTo != "" && dateFrom != "")
                    {
                        query = query.Replace("##dateFrom##", "'" + dateFrom + "'").Replace("##dateTo##", "'" + dateTo + "'");
                    }

                    if ((reportData.FilteredFieldList != null || reportData.FilteredFieldList != "") && filter.Search != null)
                    {
                        string search = filter.Search;
                        var querySearch = string.Empty;
                        var txtReplace = reportData.FilteredFieldList.Replace(" ", "");
                        var filteredFieldListArray = txtReplace.Split(",");
                        querySearch += " and ( ";
                        for (int i = 0; i < filteredFieldListArray.Length; i++)
                        {
                            if (i == filteredFieldListArray.Length - 1)
                            {
                                querySearch += filteredFieldListArray[i] + " like '%" + search + "%' ";
                            }
                            else
                            {
                                querySearch += filteredFieldListArray[i] + " like '%" + search + "%' or ";
                            }
                        }

                        querySearch += " ) ";
                        query = query.Replace("##search##", querySearch);
                    }
                    else
                    {
                        query = query.Replace("##search##", "");

                    }

                    DataTable dt = new DataTable();
                    var retObject = new List<dynamic>();
                    using (var command = context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = query;
                        command.CommandType = CommandType.Text;
                        context.Database.OpenConnection();


                        using (IDataReader dr = command.ExecuteReader())
                        {
                            if (dr.FieldCount > 0)
                            {
                                for (int i = 0; i < dr.FieldCount; i++)
                                {
                                    DataColumn dc = new DataColumn(dr.GetName(i));
                                    //DataColumn dc = new DataColumn(dr.GetName(i), dr.GetFieldType(i));
                                    dt.Columns.Add(dc);
                                }
                                object[] rowobject = new object[dr.FieldCount];
                                while (dr.Read())
                                {
                                    dr.GetValues(rowobject);
                                    dt.LoadDataRow(rowobject, true);
                                }
                            }
                        }
                        var stream = new MemoryStream();
                        //required using OfficeOpenXml;
                        // If you use EPPlus in a noncommercial context
                        // according to the Polyform Noncommercial license:
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (var package = new ExcelPackage(stream))
                        {
                            var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                            workSheet.Cells["A1"].LoadFromDataTable(dt, true);

                            //workSheet.Cells.LoadFromCollection(retObject, true);
                            package.Save();
                        }
                        stream.Position = 0;
                        string excelName = $"Report-{Guid.NewGuid().ToString() + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
                        //string excelName = $"Report-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";

                        var content = stream.ToArray();
                        string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        ExcelFileViewModel excelFile = new ExcelFileViewModel()
                        {
                            fileName = excelName,
                            content = content,
                            contentType = contentType
                        };
                        return Ok(excelFile);

                    }
                }
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.Message.ToString() });
            }
        }


    }
}
