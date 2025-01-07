using apiDealManagement.Helper;
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
using static apiDealManagement.Helper.ClsHelper;

namespace apiDealManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReconcileController : Controller
    {
        private readonly IOptions<ConnectionStringConfig> ConnectionString;
        public ReconcileController(IOptions<ConnectionStringConfig> ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

        [Authorize]
        [HttpGet]
        [Route("Get")]
        public IActionResult Get(string name, [FromQuery] PaginationFilter filter)
        {
            var data = (dynamic)null;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            var totalRecords = 0;
            var totalPages = 0;

            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var count = 0;

                    if (filter.DateFrom != null && filter.DateTo != null)
                    {
                        System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                        DateTime date_from = DateTime.ParseExact(Convert.ToDateTime(filter.DateFrom).ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo).AddDays(1);


                        if (name == null || name == "" || name == "null")
                        {

                            data = (
                                from r in context.DealReconciles.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join file_qt in context.DealUploadFiles on deal.deal_quatation_id equals file_qt.id into _file_qt
                                from deal_file_qt in _file_qt.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),

                                    deal_quatation = deal_file_qt.deal_file_path,
                                    deal_quatation_link = deal_file_qt.deal_file_link,

                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    created_by = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                //.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();




                            count = (
                                from r in context.DealReconciles.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    created_by = deal.created_by,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                //.Take(validFilter.PageSize)
                                .ToList<dynamic>().Count();
                        }
                        else
                        {
                            data = (
                                from r in context.DealReconciles.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join file_qt in context.DealUploadFiles on deal.deal_quatation_id equals file_qt.id into _file_qt
                                from deal_file_qt in _file_qt.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),

                                    deal_quatation = deal_file_qt.deal_file_path,
                                    deal_quatation_link = deal_file_qt.deal_file_link,

                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    created_by = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();


                            count = (
                                from r in context.DealReconciles.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    created_by = deal.created_by,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                //.Take(validFilter.PageSize)
                                .ToList<dynamic>().Count();
                        }
                    }
                    else
                    {
                        if (name == null || name == "" || name == "null")
                        {
                            data = (
                                from r in context.DealReconciles
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join file_qt in context.DealUploadFiles on deal.deal_quatation_id equals file_qt.id into _file_qt
                                from deal_file_qt in _file_qt.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),

                                    deal_quatation = deal_file_qt.deal_file_path,
                                    deal_quatation_link = deal_file_qt.deal_file_link,

                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    created_by = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();



                            count = (
                                from r in context.DealReconciles
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    created_by = deal.created_by,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                //.Take(validFilter.PageSize)
                                .ToList<dynamic>().Count();
                        }
                        else
                        {
                            data = (
                                from r in context.DealReconciles
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join file_qt in context.DealUploadFiles on deal.deal_quatation_id equals file_qt.id into _file_qt
                                from deal_file_qt in _file_qt.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),

                                    deal_quatation = deal_file_qt.deal_file_path,
                                    deal_quatation_link = deal_file_qt.deal_file_link,

                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    created_by = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();


                            count = (
                                from r in context.DealReconciles
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    created_by = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                //.Take(validFilter.PageSize)
                                .ToList<dynamic>().Count();
                        }
                    }

                    totalRecords = count;
                    totalPages = (int)Math.Ceiling((double)totalRecords / validFilter.PageSize);
                    if (data == null)
                    {
                        return UnprocessableEntity(new { message = "data not found" });
                    }

                }
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new PagedResponse<List<dynamic>>(data, validFilter.PageNumber, validFilter.PageSize, totalPages, totalRecords));
        }

        [Authorize]
        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromForm] DealReconcile data)
        {
            string result = "";
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var user = context.UserProfiles.SingleOrDefault(x => x.emp_id == data.updated_by);
                    if (user == null)
                    {
                        return UnprocessableEntity(new { message = "User not found" });
                    }

                    var _data = context.DealReconciles.SingleOrDefault(x => x.id == data.id);
                    if (_data == null)
                    {
                        return UnprocessableEntity(new { message = "Transaction not found" });
                    }

                    _data.invoice = data.invoice;
                    _data.status = data.status;

                    context.SaveChanges();
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { response = result });
        }


        [Authorize]
        [HttpGet("Download")]
        public async Task<IActionResult> Download(string name, [FromQuery] PaginationFilter filter)
        {
            var data = (dynamic)null;
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {

                    if (filter.DateFrom != null && filter.DateTo != null)
                    {
                        System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                        DateTime date_from = DateTime.ParseExact(Convert.ToDateTime(filter.DateFrom).ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo).AddDays(1);


                        if (name == null || name == "" || name == "null")
                        {

                            data = (
                                from r in context.DealReconciles.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    created_by = deal.created_by,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                //.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                .ToList();
                        }
                        else
                        {
                            data = (
                                from r in context.DealReconciles.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    created_by = deal.created_by,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                .ToList();
                        }
                    }
                    else
                    {
                        if (name == null || name == "" || name == "null")
                        {
                            data = (
                                from r in context.DealReconciles
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    created_by = deal.created_by,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .ToList();
                        }
                        else
                        {
                            data = (
                                from r in context.DealReconciles
                                join d in context.Deals on r.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join q in context.DealRequesteds on r.deal_reference equals q.deal_reference into _req
                                from req in _req.DefaultIfEmpty()

                                select new
                                {
                                    id = r.id,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    created_by = deal.created_by,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    deal_reference = r.deal_reference,
                                    //deal_request_code = recon.deal_request_code,
                                    value = deal.deal_coupon_value,
                                    qty_code = r.qty_code,
                                    total = (r.qty_code * deal.deal_coupon_value),
                                    status = (r.status != 1 ? "Unverified" : "Verify"),
                                    invoice = r.invoice,
                                    date_upload = context.DealCodes.Where(e => e.deal_id == deal.id).OrderByDescending(x => x.id).Select(x => x.created_at).SingleOrDefault(),
                                    customer = req.deal_customer_name,
                                    requested_quatation = req.deal_quatation,
                                    pr_number = deal.deal_pr_number,
                                    remark = deal.remark,
                                    requested_file = req.deal_requested_file,
                                    created_at = deal.created_at,
                                    requested_at = req.created_at,
                                    requested_by = context.UserProfiles.Where(e => e.id == req.created_by).Select(x => x.first_name.ToString() + " " + x.last_name.ToString()).SingleOrDefault()
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                .ToList();

                        }
                    }


                    ClsHelper clsH = new ClsHelper();
                    DataTable dt = clsH.ListToDataTable(data);

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
                    string excelName = $"Reconcile-{Guid.NewGuid().ToString() + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";

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
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.Message.ToString() });
            }
        }

    }
}
