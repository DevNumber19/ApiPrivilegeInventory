using System;
using System.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using apiDealManagement.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using OfficeOpenXml;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Net.Mail;
using Ionic.Zip;
using System.Net.Mime;
using System.Net;
using Microsoft.AspNetCore.StaticFiles;
using apiDealManagement.Helper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace apiDealManagement.Controllers
{
    [EnableCors("_allowSpecificOrigins")]
    [Route("api/[controller]")]
    [ApiController]
    public class DealManagementController : ControllerBase
    {
        private readonly IOptions<ConnectionStringConfig> ConnectionString;
        private readonly IOptions<EmailAdmin> EmailAdmins;
        public DealManagementController(IOptions<ConnectionStringConfig> ConnectionString, IOptions<EmailAdmin> EmailAdmins)
        {
            this.ConnectionString = ConnectionString;
            this.EmailAdmins = EmailAdmins;
        }

        [Authorize]
        [HttpGet("GetDealList")]
        public IActionResult Get(string name, [FromQuery] PaginationFilter filter, int? getall, int? coupon_type)
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

                    if (getall != null && getall == 1) {

                        if (coupon_type != 2)
                        {
                            data = (
                                from a in context.Deals.Where(x => x.deal_coupon_type != 2)
                                join up in context.UserProfiles on a.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on a.updated_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()
                                join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                from deal_file_qt in _file_qt.DefaultIfEmpty()
                                join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                from deal_file_pr in _file_pr.DefaultIfEmpty()
                                join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                select new
                                {
                                    deal_id = a.id,
                                    deal_customer_id = a.deal_customer_id,
                                    deal_customer = deal_customer.name,
                                    deal_supplier_id = a.deal_supplier_id,
                                    deal_supplier = deal_supplier.name,
                                    deal_name = a.deal_name,
                                    remark = a.remark,
                                    status = a.status,
                                    created_at = a.created_at,
                                    created_by = a.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    updated_at = a.updated_at,
                                    updated_by = a.updated_by,
                                    updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                    deal_coupon_type = a.deal_coupon_type,
                                    deal_coupon_value = a.deal_coupon_value,
                                    deal_permission = a.deal_permission,

                                    deal_quatation = deal_file_qt.deal_file_path,
                                    deal_quatation_link = deal_file_qt.deal_file_link,
                                    //deal_quatation = a.deal_quatation,
                                    //deal_quatation_link = a.deal_quatation_link,

                                    deal_merchant_id = a.deal_merchant_id,
                                    deal_merchant_name = a.deal_merchant_name,

                                    deal_invoice = a.deal_invoice,
                                    deal_file_invoice = deal_file_invoice.deal_file_path,

                                    deal_receipt = deal_file_receipt.deal_file_path,
                                    //deal_receipt = a.deal_receipt,

                                    deal_start_date = a.deal_start_date,
                                    deal_end_date = a.deal_end_date,
                                    deal_coupon_name = a.deal_coupon_name,
                                    deal_cost = a.deal_cost,
                                    deal_condition = a.deal_condition,

                                    deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                    //deal_coupon_image = a.deal_coupon_image,

                                    deal_major = a.deal_major,
                                    deal_discount_value = a.deal_discount_value,
                                    deal_summary_value = a.deal_summary_value,
                                    deal_add_item = a.deal_add_item,
                                    deal_add_amount = a.deal_add_amount,
                                    deal_add_value = a.deal_add_value,
                                    deal_pr_number = a.deal_pr_number,

                                    deal_pr_file = deal_file_pr.deal_file_path,
                                    //deal_pr_file = a.deal_pr_file,

                                    quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                    all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                    minimum_requested = a.minimum_requested,
                                    maximum_requested = a.maximum_requested,
                                    minimum_order = a.minimum_order,
                                    maximum_order = a.maximum_order
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.created_at)


                                .Select(x => new
                                {
                                    deal_id = x.deal_id,
                                    deal_customer_id = x.deal_customer_id,
                                    deal_customer = x.deal_customer,
                                    deal_supplier_id = x.deal_supplier_id,
                                    deal_supplier = x.deal_supplier,
                                    deal_name = x.deal_name,
                                    remark = x.remark,
                                    status = x.status,
                                    created_at = x.created_at,
                                    created_by = x.created_by,
                                    created_by_name = x.created_by_name,
                                    updated_at = x.updated_at,
                                    updated_by = x.updated_by,
                                    updated_by_name = x.updated_by_name,
                                    ////quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),
                                    //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                    //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                    quantity_code = x.quantity_code,
                                    all_quantity_code = x.all_quantity_code,
                                    quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                    deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                    deal_coupon_type = x.deal_coupon_type,
                                    deal_coupon_value = x.deal_coupon_value,
                                    deal_permission = x.deal_permission,
                                    deal_quatation = x.deal_quatation,
                                    deal_quatation_link = x.deal_quatation_link,
                                    deal_merchant_id = x.deal_merchant_id,
                                    deal_merchant_name = x.deal_merchant_name,

                                    deal_invoice = x.deal_invoice,
                                    deal_file_invoice = x.deal_file_invoice,

                                    deal_receipt = x.deal_receipt,
                                    deal_start_date = x.deal_start_date,
                                    deal_end_date = x.deal_end_date,
                                    deal_coupon_name = x.deal_coupon_name,
                                    deal_cost = x.deal_cost,
                                    deal_condition = x.deal_condition,
                                    deal_coupon_image = x.deal_coupon_image,
                                    deal_major = x.deal_major,
                                    deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                    deal_discount_value = x.deal_discount_value,
                                    deal_summary_value = x.deal_summary_value,
                                    deal_add_item = x.deal_add_item,
                                    deal_add_amount = x.deal_add_amount,
                                    deal_add_value = x.deal_add_value,
                                    deal_pr_number = x.deal_pr_number,
                                    deal_pr_file = x.deal_pr_file,
                                    minimum_requested = x.minimum_requested,
                                    maximum_requested = x.maximum_requested,
                                    minimum_order = x.minimum_order,
                                    maximum_order = x.maximum_order
                                })
                                    .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                    .ToList<dynamic>();
                        }
                        else {

                            data = (
                             from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type)
                             join up in context.UserProfiles on a.created_by equals up.id into g
                             from x in g.DefaultIfEmpty()
                             join up in context.UserProfiles on a.updated_by equals up.id into h
                             from uby in h.DefaultIfEmpty()
                             join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                             from deal_customer in _customer.DefaultIfEmpty()
                             join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                             from deal_supplier in _supplier.DefaultIfEmpty()
                             join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                             from deal_file_qt in _file_qt.DefaultIfEmpty()
                             join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                             from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                             join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                             from deal_file_pr in _file_pr.DefaultIfEmpty()
                             join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                             from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                             join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                             from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                             select new
                             {
                                 deal_id = a.id,
                                 deal_customer_id = a.deal_customer_id,
                                 deal_customer = deal_customer.name,
                                 deal_supplier_id = a.deal_supplier_id,
                                 deal_supplier = deal_supplier.name,
                                 deal_name = a.deal_name,
                                 remark = a.remark,
                                 status = a.status,
                                 created_at = a.created_at,
                                 created_by = a.created_by,
                                 created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                 updated_at = a.updated_at,
                                 updated_by = a.updated_by,
                                 updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                 deal_coupon_type = a.deal_coupon_type,
                                 deal_coupon_value = a.deal_coupon_value,
                                 deal_permission = a.deal_permission,

                                 deal_quatation = deal_file_qt.deal_file_path,
                                 deal_quatation_link = deal_file_qt.deal_file_link,
                                 //deal_quatation = x.deal_quatation,
                                 //deal_quatation_link = x.deal_quatation_link,

                                 deal_receipt = deal_file_receipt.deal_file_path,
                                 //deal_receipt = a.deal_receipt,

                                 deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                 //deal_coupon_image = a.deal_coupon_image,

                                 deal_pr_file = deal_file_pr.deal_file_path,
                                 //deal_pr_file = a.deal_pr_file,

                                 deal_merchant_id = a.deal_merchant_id,
                                 deal_merchant_name = a.deal_merchant_name,

                                 deal_invoice = a.deal_invoice,
                                 deal_file_invoice = deal_file_invoice.deal_file_path,

                                 deal_start_date = a.deal_start_date,
                                 deal_end_date = a.deal_end_date,
                                 deal_coupon_name = a.deal_coupon_name,
                                 deal_cost = a.deal_cost,
                                 deal_condition = a.deal_condition,
                                 deal_major = a.deal_major,
                                 deal_discount_value = a.deal_discount_value,
                                 deal_summary_value = a.deal_summary_value,
                                 deal_add_item = a.deal_add_item,
                                 deal_add_amount = a.deal_add_amount,
                                 deal_add_value = a.deal_add_value,
                                 deal_pr_number = a.deal_pr_number,
                                 quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                 all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                 minimum_requested = a.minimum_requested,
                                 maximum_requested = a.maximum_requested,
                                 minimum_order = a.minimum_order,
                                 maximum_order = a.maximum_order
                             })
                             .AsEnumerable()
                             .OrderBy(x => x.created_at)


                             .Select(x => new
                             {
                                 deal_id = x.deal_id,
                                 deal_customer_id = x.deal_customer_id,
                                 deal_customer = x.deal_customer,
                                 deal_supplier_id = x.deal_supplier_id,
                                 deal_supplier = x.deal_supplier,
                                 deal_name = x.deal_name,
                                 remark = x.remark,
                                 status = x.status,
                                 created_at = x.created_at,
                                 created_by = x.created_by,
                                 created_by_name = x.created_by_name,
                                 updated_at = x.updated_at,
                                 updated_by = x.updated_by,
                                 updated_by_name = x.updated_by_name,
                                 //   //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),
                                 //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                 //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                 quantity_code = x.quantity_code,
                                 all_quantity_code = x.all_quantity_code,
                                 quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                 deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                 deal_coupon_type = x.deal_coupon_type,
                                 deal_coupon_value = x.deal_coupon_value,
                                 deal_permission = x.deal_permission,
                                 deal_quatation = x.deal_quatation,
                                 deal_quatation_link = x.deal_quatation_link,
                                 deal_merchant_id = x.deal_merchant_id,
                                 deal_merchant_name = x.deal_merchant_name,

                                 deal_invoice = x.deal_invoice,
                                 deal_file_invoice = x.deal_file_invoice,

                                 deal_receipt = x.deal_receipt,
                                 deal_start_date = x.deal_start_date,
                                 deal_end_date = x.deal_end_date,
                                 deal_coupon_name = x.deal_coupon_name,
                                 deal_cost = x.deal_cost,
                                 deal_condition = x.deal_condition,
                                 deal_coupon_image = x.deal_coupon_image,
                                 deal_major = x.deal_major,
                                 deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                 deal_discount_value = x.deal_discount_value,
                                 deal_summary_value = x.deal_summary_value,
                                 deal_add_item = x.deal_add_item,
                                 deal_add_amount = x.deal_add_amount,
                                 deal_add_value = x.deal_add_value,
                                 deal_pr_number = x.deal_pr_number,
                                 deal_pr_file = x.deal_pr_file,
                                 minimum_requested = x.minimum_requested,
                                 maximum_requested = x.maximum_requested,
                                 minimum_order = x.minimum_order,
                                 maximum_order = x.maximum_order
                             })
                                 .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                 .ToList<dynamic>();

                        }


                        return Ok(data);
                    }

                    if (filter.DateFrom != null && filter.DateTo != null)
                    {
                        System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                        DateTime date_from = DateTime.ParseExact(Convert.ToDateTime(filter.DateFrom).ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy 23:59:59").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);

                        if (name == null || name == "" || name == "null")
                        {
                            if (coupon_type != 2)
                            {
                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2 && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                        .AsEnumerable()
                                        .OrderBy(x => x.created_at)
                                        .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                        .Take(validFilter.PageSize)


                                        .Select(x => new
                                        {
                                            deal_id = x.deal_id,
                                            deal_customer_id = x.deal_customer_id,
                                            deal_customer = x.deal_customer,
                                            deal_supplier_id = x.deal_supplier_id,
                                            deal_supplier = x.deal_supplier,
                                            deal_name = x.deal_name,
                                            remark = x.remark,
                                            status = x.status,
                                            created_at = x.created_at,
                                            created_by = x.created_by,
                                            created_by_name = x.created_by_name,
                                            updated_at = x.updated_at,
                                            updated_by = x.updated_by,
                                            updated_by_name = x.updated_by_name,
                                            //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                            //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                            quantity_code = x.quantity_code,
                                            all_quantity_code = x.all_quantity_code,
                                            quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                            deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                            deal_coupon_type = x.deal_coupon_type,
                                            deal_coupon_value = x.deal_coupon_value,
                                            deal_permission = x.deal_permission,
                                            deal_quatation = x.deal_quatation,
                                            deal_quatation_link = x.deal_quatation_link,
                                            deal_merchant_id = x.deal_merchant_id,
                                            deal_merchant_name = x.deal_merchant_name,

                                            deal_invoice = x.deal_invoice,
                                            deal_file_invoice = x.deal_file_invoice,

                                            deal_receipt = x.deal_receipt,
                                            deal_start_date = x.deal_start_date,
                                            deal_end_date = x.deal_end_date,
                                            deal_coupon_name = x.deal_coupon_name,
                                            deal_cost = x.deal_cost,
                                            deal_condition = x.deal_condition,
                                            deal_coupon_image = x.deal_coupon_image,
                                            deal_major = x.deal_major,
                                            deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                            deal_discount_value = x.deal_discount_value,
                                            deal_summary_value = x.deal_summary_value,
                                            deal_add_item = x.deal_add_item,
                                            deal_add_amount = x.deal_add_amount,
                                            deal_add_value = x.deal_add_value,
                                            deal_pr_number = x.deal_pr_number,
                                            deal_pr_file = x.deal_pr_file,
                                            minimum_requested = x.minimum_requested,
                                            maximum_requested = x.maximum_requested,
                                            minimum_order = x.minimum_order,
                                            maximum_order = x.maximum_order
                                        })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList<dynamic>();




                                count = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2 && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.description,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,
                                        deal_quatation = a.deal_quatation,
                                        deal_quatation_link = a.deal_quatation_link,
                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,
                                        deal_invoice = a.deal_invoice,
                                        deal_receipt = a.deal_receipt,
                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_coupon_image = a.deal_coupon_image,
                                        deal_major = a.deal_major,
                                        deal_discount = a.deal_discount_value
                                    })
                                    .AsEnumerable()
                                    .OrderBy(x => x.created_at)
                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    //.Take(validFilter.PageSize)
                                    .ToList<dynamic>().Count();
                            }
                            else
                            {
                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                        .AsEnumerable()
                                        .OrderBy(x => x.created_at)
                                        .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                        .Take(validFilter.PageSize)


                                        .Select(x => new
                                        {
                                            deal_id = x.deal_id,
                                            deal_customer_id = x.deal_customer_id,
                                            deal_customer = x.deal_customer,
                                            deal_supplier_id = x.deal_supplier_id,
                                            deal_supplier = x.deal_supplier,
                                            deal_name = x.deal_name,
                                            remark = x.remark,
                                            status = x.status,
                                            created_at = x.created_at,
                                            created_by = x.created_by,
                                            created_by_name = x.created_by_name,
                                            updated_at = x.updated_at,
                                            updated_by = x.updated_by,
                                            updated_by_name = x.updated_by_name,
                                            //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                            //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                            quantity_code = x.quantity_code,
                                            all_quantity_code = x.all_quantity_code,
                                            quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                            deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                            deal_coupon_type = x.deal_coupon_type,
                                            deal_coupon_value = x.deal_coupon_value,
                                            deal_permission = x.deal_permission,
                                            deal_quatation = x.deal_quatation,
                                            deal_quatation_link = x.deal_quatation_link,
                                            deal_merchant_id = x.deal_merchant_id,
                                            deal_merchant_name = x.deal_merchant_name,

                                            deal_invoice = x.deal_invoice,
                                            deal_file_invoice = x.deal_file_invoice,

                                            deal_receipt = x.deal_receipt,
                                            deal_start_date = x.deal_start_date,
                                            deal_end_date = x.deal_end_date,
                                            deal_coupon_name = x.deal_coupon_name,
                                            deal_cost = x.deal_cost,
                                            deal_condition = x.deal_condition,
                                            deal_coupon_image = x.deal_coupon_image,
                                            deal_major = x.deal_major,
                                            deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                            deal_discount_value = x.deal_discount_value,
                                            deal_summary_value = x.deal_summary_value,
                                            deal_add_item = x.deal_add_item,
                                            deal_add_amount = x.deal_add_amount,
                                            deal_add_value = x.deal_add_value,
                                            deal_pr_number = x.deal_pr_number,
                                            deal_pr_file = x.deal_pr_file,
                                            minimum_requested = x.minimum_requested,
                                            maximum_requested = x.maximum_requested,
                                            minimum_order = x.minimum_order,
                                            maximum_order = x.maximum_order
                                        })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList<dynamic>();




                                count = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.description,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,
                                        deal_quatation = a.deal_quatation,
                                        deal_quatation_link = a.deal_quatation_link,
                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,
                                        deal_invoice = a.deal_invoice,
                                        deal_receipt = a.deal_receipt,
                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_coupon_image = a.deal_coupon_image,
                                        deal_major = a.deal_major,
                                        deal_discount = a.deal_discount_value
                                    })
                                    .AsEnumerable()
                                    .OrderBy(x => x.created_at)
                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    //.Take(validFilter.PageSize)
                                    .ToList<dynamic>().Count();
                            }
                        }
                        else
                        {
                            if (coupon_type != 2)
                            {
                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2 && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name) || x.deal_id.ToString().Contains(name) )
                                    .OrderBy(x => x.created_at)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        deal_customer_id = x.deal_customer_id,
                                        deal_customer = x.deal_customer,
                                        deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        deal_permission = x.deal_permission,
                                        deal_quatation = x.deal_quatation,
                                        deal_quatation_link = x.deal_quatation_link,
                                        deal_merchant_id = x.deal_merchant_id,
                                        deal_merchant_name = x.deal_merchant_name,

                                        deal_invoice = x.deal_invoice,
                                        deal_file_invoice = x.deal_file_invoice,

                                        deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        deal_coupon_name = x.deal_coupon_name,
                                        deal_cost = x.deal_cost,
                                        deal_condition = x.deal_condition,
                                        deal_coupon_image = x.deal_coupon_image,
                                        deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        deal_discount_value = x.deal_discount_value,
                                        deal_summary_value = x.deal_summary_value,
                                        deal_add_item = x.deal_add_item,
                                        deal_add_amount = x.deal_add_amount,
                                        deal_add_value = x.deal_add_value,
                                        deal_pr_number = x.deal_pr_number,
                                        deal_pr_file = x.deal_pr_file,
                                        minimum_requested = x.minimum_requested,
                                        maximum_requested = x.maximum_requested,
                                        minimum_order = x.minimum_order,
                                        maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList<dynamic>();


                                count = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2 && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.description,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,
                                        deal_quatation = a.deal_quatation,
                                        deal_quatation_link = a.deal_quatation_link,
                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,
                                        deal_invoice = a.deal_invoice,
                                        deal_receipt = a.deal_receipt,
                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_coupon_image = a.deal_coupon_image,
                                        deal_major = a.deal_major,
                                        deal_discount = a.deal_discount_value
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name) || x.deal_id.ToString().Contains(name) )
                                    .OrderBy(x => x.created_at)
                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    //.Take(validFilter.PageSize)
                                    .ToList<dynamic>().Count();
                            }
                            else
                            {
                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name) || x.deal_id.ToString().Contains(name) )
                                    .OrderBy(x => x.created_at)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        deal_customer_id = x.deal_customer_id,
                                        deal_customer = x.deal_customer,
                                        deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        deal_permission = x.deal_permission,
                                        deal_quatation = x.deal_quatation,
                                        deal_quatation_link = x.deal_quatation_link,
                                        deal_merchant_id = x.deal_merchant_id,
                                        deal_merchant_name = x.deal_merchant_name,

                                        deal_invoice = x.deal_invoice,
                                        deal_file_invoice = x.deal_file_invoice,

                                        deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        deal_coupon_name = x.deal_coupon_name,
                                        deal_cost = x.deal_cost,
                                        deal_condition = x.deal_condition,
                                        deal_coupon_image = x.deal_coupon_image,
                                        deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        deal_discount_value = x.deal_discount_value,
                                        deal_summary_value = x.deal_summary_value,
                                        deal_add_item = x.deal_add_item,
                                        deal_add_amount = x.deal_add_amount,
                                        deal_add_value = x.deal_add_value,
                                        deal_pr_number = x.deal_pr_number,
                                        deal_pr_file = x.deal_pr_file,
                                        minimum_requested = x.minimum_requested,
                                        maximum_requested = x.maximum_requested,
                                        minimum_order = x.minimum_order,
                                        maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList<dynamic>();


                                count = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.description,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,
                                        deal_quatation = a.deal_quatation,
                                        deal_quatation_link = a.deal_quatation_link,
                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,
                                        deal_invoice = a.deal_invoice,
                                        deal_receipt = a.deal_receipt,
                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_coupon_image = a.deal_coupon_image,
                                        deal_major = a.deal_major,
                                        deal_discount = a.deal_discount_value
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name) || x.deal_id.ToString().Contains(name))
                                    .OrderBy(x => x.created_at)
                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    //.Take(validFilter.PageSize)
                                    .ToList<dynamic>().Count();
                            }
                        }
                    }
                    else
                    {
                        if (name == null || name == "" || name == "null")
                        {
                            if (coupon_type != 2)
                            {
                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .OrderBy(x => x.created_at)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        deal_customer_id = x.deal_customer_id,
                                        deal_customer = x.deal_customer,
                                        deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        deal_permission = x.deal_permission,
                                        deal_quatation = x.deal_quatation,
                                        deal_quatation_link = x.deal_quatation_link,
                                        deal_merchant_id = x.deal_merchant_id,
                                        deal_merchant_name = x.deal_merchant_name,

                                        deal_invoice = x.deal_invoice,
                                        deal_file_invoice = x.deal_file_invoice,

                                        deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        deal_coupon_name = x.deal_coupon_name,
                                        deal_cost = x.deal_cost,
                                        deal_condition = x.deal_condition,
                                        deal_coupon_image = x.deal_coupon_image,
                                        deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        deal_discount_value = x.deal_discount_value,
                                        deal_summary_value = x.deal_summary_value,
                                        deal_add_item = x.deal_add_item,
                                        deal_add_amount = x.deal_add_amount,
                                        deal_add_value = x.deal_add_value,
                                        deal_pr_number = x.deal_pr_number,
                                        deal_pr_file = x.deal_pr_file,
                                        minimum_requested = x.minimum_requested,
                                        maximum_requested = x.maximum_requested,
                                        minimum_order = x.minimum_order,
                                        maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList<dynamic>();




                                count = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.description,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,
                                        deal_quatation = a.deal_quatation,
                                        deal_quatation_link = a.deal_quatation_link,
                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,
                                        deal_invoice = a.deal_invoice,
                                        deal_receipt = a.deal_receipt,
                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_coupon_image = a.deal_coupon_image,
                                        deal_major = a.deal_major,
                                        deal_discount = a.deal_discount_value
                                    })
                                    .AsEnumerable()
                                    .OrderBy(x => x.created_at)
                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    //.Take(validFilter.PageSize)
                                    .ToList<dynamic>().Count();
                            }
                            else
                            {
                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .OrderBy(x => x.created_at)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        deal_customer_id = x.deal_customer_id,
                                        deal_customer = x.deal_customer,
                                        deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        deal_permission = x.deal_permission,
                                        deal_quatation = x.deal_quatation,
                                        deal_quatation_link = x.deal_quatation_link,
                                        deal_merchant_id = x.deal_merchant_id,
                                        deal_merchant_name = x.deal_merchant_name,

                                        deal_invoice = x.deal_invoice,
                                        deal_file_invoice = x.deal_file_invoice,

                                        deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        deal_coupon_name = x.deal_coupon_name,
                                        deal_cost = x.deal_cost,
                                        deal_condition = x.deal_condition,
                                        deal_coupon_image = x.deal_coupon_image,
                                        deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        deal_discount_value = x.deal_discount_value,
                                        deal_summary_value = x.deal_summary_value,
                                        deal_add_item = x.deal_add_item,
                                        deal_add_amount = x.deal_add_amount,
                                        deal_add_value = x.deal_add_value,
                                        deal_pr_number = x.deal_pr_number,
                                        deal_pr_file = x.deal_pr_file,
                                        minimum_requested = x.minimum_requested,
                                        maximum_requested = x.maximum_requested,
                                        minimum_order = x.minimum_order,
                                        maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList<dynamic>();




                                count = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.description,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,
                                        deal_quatation = a.deal_quatation,
                                        deal_quatation_link = a.deal_quatation_link,
                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,
                                        deal_invoice = a.deal_invoice,
                                        deal_receipt = a.deal_receipt,
                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_coupon_image = a.deal_coupon_image,
                                        deal_major = a.deal_major,
                                        deal_discount = a.deal_discount_value
                                    })
                                    .AsEnumerable()
                                    .OrderBy(x => x.created_at)
                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    //.Take(validFilter.PageSize)
                                    .ToList<dynamic>().Count();
                            }

                        }
                        else
                        {
                            if (coupon_type != 2)
                            {
                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name) || x.deal_id.ToString().Contains(name))
                                    .OrderBy(x => x.created_at)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        deal_customer_id = x.deal_customer_id,
                                        deal_customer = x.deal_customer,
                                        deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        ////quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        deal_permission = x.deal_permission,
                                        deal_quatation = x.deal_quatation,
                                        deal_quatation_link = x.deal_quatation_link,
                                        deal_merchant_id = x.deal_merchant_id,
                                        deal_merchant_name = x.deal_merchant_name,

                                        deal_invoice = x.deal_invoice,
                                        deal_file_invoice = x.deal_file_invoice,

                                        deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        deal_coupon_name = x.deal_coupon_name,
                                        deal_cost = x.deal_cost,
                                        deal_condition = x.deal_condition,
                                        deal_coupon_image = x.deal_coupon_image,
                                        deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        deal_discount_value = x.deal_discount_value,
                                        deal_summary_value = x.deal_summary_value,
                                        deal_add_item = x.deal_add_item,
                                        deal_add_amount = x.deal_add_amount,
                                        deal_add_value = x.deal_add_value,
                                        deal_pr_number = x.deal_pr_number,
                                        deal_pr_file = x.deal_pr_file,
                                        minimum_requested = x.minimum_requested,
                                        maximum_requested = x.maximum_requested,
                                        minimum_order = x.minimum_order,
                                        maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList<dynamic>();


                                count = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.description,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,
                                        deal_quatation = a.deal_quatation,
                                        deal_quatation_link = a.deal_quatation_link,
                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,
                                        deal_invoice = a.deal_invoice,
                                        deal_receipt = a.deal_receipt,
                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_coupon_image = a.deal_coupon_image,
                                        deal_major = a.deal_major,
                                        deal_discount = a.deal_discount_value
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name) || x.deal_id.ToString().Contains(name))
                                    .OrderByDescending(x => x.created_at)
                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    //.Take(validFilter.PageSize)
                                    .ToList<dynamic>().Count();
                            }
                            else
                            {
                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name) || x.deal_id.ToString().Contains(name))
                                    .OrderBy(x => x.created_at)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        deal_customer_id = x.deal_customer_id,
                                        deal_customer = x.deal_customer,
                                        deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        ////quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),
                                        //    quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //    all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        deal_permission = x.deal_permission,
                                        deal_quatation = x.deal_quatation,
                                        deal_quatation_link = x.deal_quatation_link,
                                        deal_merchant_id = x.deal_merchant_id,
                                        deal_merchant_name = x.deal_merchant_name,

                                        deal_invoice = x.deal_invoice,
                                        deal_file_invoice = x.deal_file_invoice,

                                        deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        deal_coupon_name = x.deal_coupon_name,
                                        deal_cost = x.deal_cost,
                                        deal_condition = x.deal_condition,
                                        deal_coupon_image = x.deal_coupon_image,
                                        deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        deal_discount_value = x.deal_discount_value,
                                        deal_summary_value = x.deal_summary_value,
                                        deal_add_item = x.deal_add_item,
                                        deal_add_amount = x.deal_add_amount,
                                        deal_add_value = x.deal_add_value,
                                        deal_pr_number = x.deal_pr_number,
                                        deal_pr_file = x.deal_pr_file,
                                        minimum_requested = x.minimum_requested,
                                        maximum_requested = x.maximum_requested,
                                        minimum_order = x.minimum_order,
                                        maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList<dynamic>();


                                count = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.description,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,
                                        deal_quatation = a.deal_quatation,
                                        deal_quatation_link = a.deal_quatation_link,
                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,
                                        deal_invoice = a.deal_invoice,
                                        deal_receipt = a.deal_receipt,
                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_coupon_image = a.deal_coupon_image,
                                        deal_major = a.deal_major,
                                        deal_discount = a.deal_discount_value
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name) || x.deal_id.ToString().Contains(name))
                                    .OrderByDescending(x => x.created_at)
                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    //.Take(validFilter.PageSize)
                                    .ToList<dynamic>().Count();
                            }

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
        private int CompareDate(DateTime? start_date, DateTime? end_date)
        {
            try
            {
                int res = 0;
                if (start_date != null && end_date != null)
                {
                    System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                    DateTime d1 = DateTime.ParseExact(start_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                    DateTime d2 = DateTime.ParseExact(end_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                    //res = DateTime.Compare(d2, d1);

                    res = (int)((d2 - d1).TotalDays);
                }

                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize]
        [HttpGet("GetDealCode")]
        public IActionResult GetDealCode(string name, [FromQuery] PaginationFilter filter)
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
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy 23:59:59").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);

                        if (name == null || name == "" || name == "null")
                        {
                            data = (
                                from deal_code in context.DealCodes.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join dealC in context.Deals on deal_code.deal_id equals dealC.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.requested_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_code.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_code.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()

                                select new
                                {
                                    id = deal_code.id,
                                    deal_customer_id = deal.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_id = (int?)deal.id,
                                    deal_name = (deal.deal_name == null ? String.Empty : deal.deal_name),
                                    deal_coupon_code = deal_code.deal_coupon_code,
                                    deal_reference = deal_code.deal_reference,
                                    remark = deal_code.remark,
                                    created_at = deal_code.created_at,
                                    created_by = deal_code.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = deal_code.requested_at,
                                    requested_by = deal_code.requested_by,
                                    requested_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();




                            count = (
                                from deal_code in context.DealCodes.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join deal in context.Deals on deal_code.deal_id equals deal.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.requested_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_code.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_code.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()

                                select new
                                {
                                    id = deal_code.id,
                                    deal_customer_id = deal.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_id = (int?)deal.id,
                                    deal_name = (deal.deal_name == null ? String.Empty : deal.deal_name),
                                    deal_coupon_code = deal_code.deal_coupon_code,
                                    deal_reference = deal_code.deal_reference,
                                    remark = deal_code.remark,
                                    created_at = deal_code.created_at,
                                    created_by = deal_code.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = deal_code.requested_at,
                                    requested_by = deal_code.requested_by,
                                    requested_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
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
                                from deal_code in context.DealCodes.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join deal in context.Deals on deal_code.deal_id equals deal.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.requested_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_code.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_code.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()

                                select new
                                {
                                    id = deal_code.id,
                                    deal_customer_id = deal.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_id = (int?)deal.id,
                                    deal_name = (deal.deal_name == null ? String.Empty : deal.deal_name),
                                    deal_coupon_code = deal_code.deal_coupon_code,
                                    deal_reference = deal_code.deal_reference,
                                    remark = deal_code.remark,
                                    created_at = deal_code.created_at,
                                    created_by = deal_code.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = deal_code.requested_at,
                                    requested_by = deal_code.requested_by,
                                    requested_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name) || x.deal_coupon_code == name || x.deal_id.ToString().Contains(name))
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();


                            count = (
                                from deal_code in context.DealCodes.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join deal in context.Deals on deal_code.deal_id equals deal.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.requested_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_code.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_code.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()

                                select new
                                {
                                    id = deal_code.id,
                                    deal_customer_id = deal.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_id = (int?)deal.id,
                                    deal_name = (deal.deal_name == null ? String.Empty : deal.deal_name),
                                    deal_coupon_code = deal_code.deal_coupon_code,
                                    deal_reference = deal_code.deal_reference,
                                    remark = deal_code.remark,
                                    created_at = deal_code.created_at,
                                    created_by = deal_code.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = deal_code.requested_at,
                                    requested_by = deal_code.requested_by,
                                    requested_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name) || x.deal_coupon_code == name || x.deal_id.ToString().Contains(name))
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
                                from deal_code in context.DealCodes
                                join deal in context.Deals on deal_code.deal_id equals deal.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.requested_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_code.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_code.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()

                                select new
                                {
                                    id = deal_code.id,
                                    deal_customer_id = deal.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_id = (int?)deal.id,
                                    deal_name = (deal.deal_name == null ? String.Empty : deal.deal_name),
                                    deal_coupon_code = deal_code.deal_coupon_code,
                                    deal_reference = deal_code.deal_reference,
                                    remark = deal_code.remark,
                                    created_at = deal_code.created_at,
                                    created_by = deal_code.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = deal_code.requested_at,
                                    requested_by = deal_code.requested_by,
                                    requested_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();




                            count = (
                                from deal_code in context.DealCodes
                                join deal in context.Deals on deal_code.deal_id equals deal.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.requested_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_code.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_code.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()

                                select new
                                {
                                    id = deal_code.id,
                                    deal_customer_id = deal.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_id = (int?)deal.id,
                                    deal_name = (deal.deal_name == null ? String.Empty : deal.deal_name),
                                    deal_coupon_code = deal_code.deal_coupon_code,
                                    deal_reference = deal_code.deal_reference,
                                    remark = deal_code.remark,
                                    created_at = deal_code.created_at,
                                    created_by = deal_code.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = deal_code.requested_at,
                                    requested_by = deal_code.requested_by,
                                    requested_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
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
                                from deal_code in context.DealCodes
                                join deal in context.Deals on deal_code.deal_id equals deal.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.requested_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_code.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_code.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()

                                select new
                                {
                                    id = deal_code.id,
                                    deal_customer_id = deal.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_id = (int?)deal.id,
                                    deal_name = (deal.deal_name == null ? String.Empty : deal.deal_name),
                                    deal_coupon_code = deal_code.deal_coupon_code,
                                    deal_reference = deal_code.deal_reference,
                                    remark = deal_code.remark,
                                    created_at = deal_code.created_at,
                                    created_by = deal_code.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = deal_code.requested_at,
                                    requested_by = deal_code.requested_by,
                                    requested_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name) || x.deal_coupon_code == name || x.deal_id.ToString().Contains(name))
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();


                            count = (
                                from deal_code in context.DealCodes
                                join deal in context.Deals on deal_code.deal_id equals deal.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on deal_code.requested_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_code.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_code.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()

                                select new
                                {
                                    id = deal_code.id,
                                    deal_customer_id = deal.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_id = (int?)deal.id,
                                    deal_name = (deal.deal_name == null ? String.Empty : deal.deal_name),
                                    deal_coupon_code = deal_code.deal_coupon_code,
                                    deal_reference = deal_code.deal_reference,
                                    remark = deal_code.remark,
                                    created_at = deal_code.created_at,
                                    created_by = deal_code.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    requested_at = deal_code.requested_at,
                                    requested_by = deal_code.requested_by,
                                    requested_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name) || x.deal_coupon_code == name || x.deal_id.ToString().Contains(name))
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
        [HttpGet("GetLog")]
        public IActionResult GetLog(string name, [FromQuery] PaginationFilter filter)
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
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy 23:59:59").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);

                        if (name == null || name == "" || name == "null")
                        {
                            data = (
                                from l in context.DealLogs.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : (l.type == 1 ? "download" : "")),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();




                            count = (
                                from l in context.DealLogs.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : (l.type == 1 ? "download" : "")),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
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
                                from l in context.DealLogs.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : (l.type == 1 ? "download" : "")),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();


                            count = (
                                from l in context.DealLogs.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : (l.type == 1 ? "download" : "")),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
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
                                from l in context.DealLogs
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : (l.type == 1 ? "download" : "")),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();




                            count = (
                                from l in context.DealLogs
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : (l.type == 1 ? "download" : "")),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
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
                                from l in context.DealLogs
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : (l.type == 1 ? "download" : "")),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();


                            count = (
                                from l in context.DealLogs
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : (l.type == 1 ? "download" : "")),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
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
        [HttpGet("GetDealCustomer")]
        public IActionResult GetDealCustomer(int? type, string name, string status, [FromQuery] PaginationFilter filter)
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

                    if (type != null)
                    {
                        if (status == null || status == "" || status == "null")
                        {
                            //data = context.DealCustomers.Where(x => (x.deleted_at == null || x.deleted_at.ToString() == ""))
                            //                        .OrderBy(c => c.name)
                            //                        .ToList<dynamic>();

                            data = (
                                from d in context.DealCustomers.Where(x => (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                join file_up in context.DealUploadFiles on d.doc_id equals file_up.id into _file_up
                                from deal_file_up in _file_up.DefaultIfEmpty()
                                join up in context.UserProfiles on d.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on d.updated_by equals up.id into h
                                from uby in h.DefaultIfEmpty()

                                select new
                                {
                                    id = d.id,
                                    name = d.name,
                                    description = d.description,
                                    link = d.link,
                                    doc_id = d.doc_id,
                                    doc = deal_file_up.deal_file_path,
                                    remark = d.remark,
                                    status = d.status,
                                    contact_name = d.contact_name,
                                    contact_phone = d.contact_phone,
                                    contact_email = d.contact_email,
                                    created_at = d.created_at,
                                    created_by = d.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    updated_at = d.updated_at,
                                    updated_by = d.updated_by,
                                    updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                    deleted_at = d.deleted_at,
                                })
                                .AsEnumerable()
                                .OrderBy(c => c.name)
                                .ToList<dynamic>();
                        }
                        else
                        {
                            //data = context.DealCustomers.Where(x => x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                            //                        .OrderBy(c => c.name)
                            //                        .ToList<dynamic>();

                            data = (
                                from d in context.DealCustomers.Where(x => x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                join file_up in context.DealUploadFiles on d.doc_id equals file_up.id into _file_up
                                from deal_file_up in _file_up.DefaultIfEmpty()
                                join up in context.UserProfiles on d.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on d.updated_by equals up.id into h
                                from uby in h.DefaultIfEmpty()

                                select new
                                {
                                    id = d.id,
                                    name = d.name,
                                    description = d.description,
                                    link = d.link,
                                    doc_id = d.doc_id,
                                    doc = deal_file_up.deal_file_path,
                                    remark = d.remark,
                                    status = d.status,
                                    contact_name = d.contact_name,
                                    contact_phone = d.contact_phone,
                                    contact_email = d.contact_email,
                                    created_at = d.created_at,
                                    created_by = d.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    updated_at = d.updated_at,
                                    updated_by = d.updated_by,
                                    updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                    deleted_at = d.deleted_at,
                                })
                                .AsEnumerable()
                                .OrderBy(c => c.name)
                                .ToList<dynamic>();
                        }
                    }
                    else
                    {

                        if (status == null || status == "" || status == "null")
                        {
                            if (name == null || name == "" || name == "null")
                            {
                                //data = context.DealCustomers.Where(x => (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                //                    .AsEnumerable()
                                //                    .OrderBy(c => c.name)
                                //                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                //                    .Take(validFilter.PageSize)
                                //                    .ToList<dynamic>();

                                data = (
                                    from d in context.DealCustomers.Where(x => (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                    join file_up in context.DealUploadFiles on d.doc_id equals file_up.id into _file_up
                                    from deal_file_up in _file_up.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()

                                    select new
                                    {
                                        id = d.id,
                                        name = d.name,
                                        description = d.description,
                                        link = d.link,
                                        doc_id = d.doc_id,
                                        doc = deal_file_up.deal_file_path,
                                        remark = d.remark,
                                        status = d.status,
                                        contact_name = d.contact_name,
                                        contact_phone = d.contact_phone,
                                        contact_email = d.contact_email,
                                        created_at = d.created_at,
                                        created_by = d.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = d.updated_at,
                                        updated_by = d.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        deleted_at = d.deleted_at,
                                    })
                                    .AsEnumerable()
                                    .OrderBy(c => c.name)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)
                                    .ToList<dynamic>();


                                count = context.DealCustomers.Where(x => (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                                    .AsEnumerable()
                                                    .OrderBy(c => c.name)
                                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                    //.Take(validFilter.PageSize)
                                                    .ToList<dynamic>().Count();
                            }
                            else
                            {
                                //data = context.DealCustomers.Where(x => x.name.Contains(name) || x.description.Contains(name) && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                //                    .AsEnumerable()
                                //                    .OrderBy(c => c.name)
                                //                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                //                    .Take(validFilter.PageSize)
                                //                    .ToList<dynamic>();

                                data = (
                                    from d in context.DealCustomers.Where(x => x.name.Contains(name) || x.description.Contains(name) && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                    join file_up in context.DealUploadFiles on d.doc_id equals file_up.id into _file_up
                                    from deal_file_up in _file_up.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()

                                    select new
                                    {
                                        id = d.id,
                                        name = d.name,
                                        description = d.description,
                                        link = d.link,
                                        doc_id = d.doc_id,
                                        doc = deal_file_up.deal_file_path,
                                        remark = d.remark,
                                        status = d.status,
                                        contact_name = d.contact_name,
                                        contact_phone = d.contact_phone,
                                        contact_email = d.contact_email,
                                        created_at = d.created_at,
                                        created_by = d.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = d.updated_at,
                                        updated_by = d.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        deleted_at = d.deleted_at,
                                    })
                                    .AsEnumerable()
                                    .OrderBy(c => c.name)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)
                                    .ToList<dynamic>();


                                count = context.DealCustomers.Where(x => x.name.Contains(name) || x.description.Contains(name) && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                                    .AsEnumerable()
                                                    .OrderBy(c => c.name)
                                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                    //.Take(validFilter.PageSize)
                                                    .ToList<dynamic>().Count();
                            }
                        }
                        else
                        {
                            if (name == null || name == "" || name == "null")
                            {
                                //    data = context.DealCustomers.Where(x => x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                //                        .AsEnumerable()
                                //                        .OrderBy(c => c.name)
                                //                        .ToList<dynamic>();

                                data = (
                                    from d in context.DealCustomers.Where(x => x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                    join file_up in context.DealUploadFiles on d.doc_id equals file_up.id into _file_up
                                    from deal_file_up in _file_up.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()

                                    select new
                                    {
                                        id = d.id,
                                        name = d.name,
                                        description = d.description,
                                        link = d.link,
                                        doc_id = d.doc_id,
                                        doc = deal_file_up.deal_file_path,
                                        remark = d.remark,
                                        status = d.status,
                                        contact_name = d.contact_name,
                                        contact_phone = d.contact_phone,
                                        contact_email = d.contact_email,
                                        created_at = d.created_at,
                                        created_by = d.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = d.updated_at,
                                        updated_by = d.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        deleted_at = d.deleted_at,
                                    })
                                    .AsEnumerable()
                                    .OrderBy(c => c.name)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)
                                    .ToList<dynamic>();


                                count = context.DealCustomers.Where(x => x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                                    .AsEnumerable()
                                                    .OrderBy(c => c.name)
                                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                    //.Take(validFilter.PageSize)
                                                    .ToList<dynamic>().Count();
                            }
                            else
                            {
                                //data = context.DealCustomers.Where(x => (x.name.Contains(name) || x.description.Contains(name)) && x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                //                    .AsEnumerable()
                                //                    .OrderBy(c => c.name)
                                //                    .ToList<dynamic>();

                                data = (
                                    from d in context.DealCustomers.Where(x => (x.name.Contains(name) || x.description.Contains(name)) && x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                    join file_up in context.DealUploadFiles on d.doc_id equals file_up.id into _file_up
                                    from deal_file_up in _file_up.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()

                                    select new
                                    {
                                        id = d.id,
                                        name = d.name,
                                        description = d.description,
                                        link = d.link,
                                        doc_id = d.doc_id,
                                        doc = deal_file_up.deal_file_path,
                                        remark = d.remark,
                                        status = d.status,
                                        contact_name = d.contact_name,
                                        contact_phone = d.contact_phone,
                                        contact_email = d.contact_email,
                                        created_at = d.created_at,
                                        created_by = d.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = d.updated_at,
                                        updated_by = d.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        deleted_at = d.deleted_at,
                                    })
                                    .AsEnumerable()
                                    .OrderBy(c => c.name)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)
                                    .ToList<dynamic>();


                                count = context.DealCustomers.Where(x => (x.name.Contains(name) || x.description.Contains(name)) && x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                                    .AsEnumerable()
                                                    .OrderBy(c => c.name)
                                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                    //.Take(validFilter.PageSize)
                                                    .ToList<dynamic>().Count();
                            }
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
        [HttpGet("GetDealSupplier")]
        public IActionResult GetDealSupplier(int? type, string name, string status, [FromQuery] PaginationFilter filter)
        {
            var data = (dynamic)null;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            var totalRecords = 0;
            var totalPages = 0;

            var contextDealUser = HttpContext.Items["DealUser"];

            try
            {
                if (contextDealUser == null)
                {
                    return UnprocessableEntity(new { message = "user data not found" });
                }

                DealUser u = new DealUser();
                u = (DealUser)contextDealUser;
                int? is_Admin = (u.is_admin != null ? u.is_admin : 0);

                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var count = 0;

                    if (type != null)
                    {
                        if (status == null || status == "" || status == "null")
                        {
                            //data = context.DealSuppliers.Where(x => (x.deleted_at == null || x.deleted_at.ToString() == ""))
                            //                        .OrderBy(c => c.name)
                            //                        .ToList<dynamic>();

                            data = (
                                from d in context.DealSuppliers.Where(x => (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                join up in context.UserProfiles on d.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on d.updated_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join file_vat_20 in context.DealUploadFiles on d.file_vat_20 equals file_vat_20.id into _file_vat_20
                                from deal_file_vat_20 in _file_vat_20.DefaultIfEmpty()
                                join file_certificate in context.DealUploadFiles on d.file_certificate equals file_certificate.id into _file_certificate
                                from deal_file_certificate in _file_certificate.DefaultIfEmpty()
                                join file_bookbank in context.DealUploadFiles on d.file_bookbank equals file_bookbank.id into _file_bookbank
                                from deal_file_bookbank in _file_bookbank.DefaultIfEmpty()
                                join file_accept in context.DealUploadFiles on d.file_accept equals file_accept.id into _file_accept
                                from deal_file_accept in _file_accept.DefaultIfEmpty()
                                join file_major in context.DealUploadFiles on d.major equals file_major.id into _file_major
                                from deal_file_major in _file_major.DefaultIfEmpty()

                                select new
                                {
                                    id = d.id,
                                    name = d.name,
                                    description = d.description,
                                    link = d.link,
                                    major = deal_file_major.deal_file_path,
                                    amount_major = d.amount_major,
                                    doc = d.doc,
                                    file_vat_20 = deal_file_vat_20.deal_file_path,
                                    file_certificate = deal_file_certificate.deal_file_path,
                                    file_bookbank = deal_file_bookbank.deal_file_path,
                                    file_accept = deal_file_accept.deal_file_path,
                                    remark = d.remark,
                                    status = d.status,
                                    contact_name = (is_Admin == 1 ? d.contact_name : null),
                                    contact_phone = (is_Admin == 1 ? d.contact_phone : null),
                                    contact_email = (is_Admin == 1 ? d.contact_email : null),
                                    type = d.type,
                                    created_at = d.created_at,
                                    created_by = d.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    updated_at = d.updated_at,
                                    updated_by = d.updated_by,
                                    updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                    deleted_at = d.deleted_at,
                                })
                                .AsEnumerable()
                                .OrderBy(c => c.name)
                                .ToList<dynamic>();
                        }
                        else
                        {
                            //data = context.DealSuppliers.Where(x => x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                            //                        .OrderBy(c => c.name)
                            //                        .ToList<dynamic>();

                            data = (
                                from d in context.DealSuppliers.Where(x => x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                join up in context.UserProfiles on d.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on d.updated_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join file_vat_20 in context.DealUploadFiles on d.file_vat_20 equals file_vat_20.id into _file_vat_20
                                from deal_file_vat_20 in _file_vat_20.DefaultIfEmpty()
                                join file_certificate in context.DealUploadFiles on d.file_certificate equals file_certificate.id into _file_certificate
                                from deal_file_certificate in _file_certificate.DefaultIfEmpty()
                                join file_bookbank in context.DealUploadFiles on d.file_bookbank equals file_bookbank.id into _file_bookbank
                                from deal_file_bookbank in _file_bookbank.DefaultIfEmpty()
                                join file_accept in context.DealUploadFiles on d.file_accept equals file_accept.id into _file_accept
                                from deal_file_accept in _file_accept.DefaultIfEmpty()
                                join file_major in context.DealUploadFiles on d.major equals file_major.id into _file_major
                                from deal_file_major in _file_major.DefaultIfEmpty()

                                select new
                                {
                                    id = d.id,
                                    name = d.name,
                                    description = d.description,
                                    link = d.link,
                                    major = deal_file_major.deal_file_path,
                                    amount_major = d.amount_major,
                                    doc = d.doc,
                                    file_vat_20 = deal_file_vat_20.deal_file_path,
                                    file_certificate = deal_file_certificate.deal_file_path,
                                    file_bookbank = deal_file_bookbank.deal_file_path,
                                    file_accept = deal_file_accept.deal_file_path,
                                    remark = d.remark,
                                    status = d.status,
                                    contact_name = (is_Admin == 1 ? d.contact_name : null),
                                    contact_phone = (is_Admin == 1 ? d.contact_phone : null),
                                    contact_email = (is_Admin == 1 ? d.contact_email : null),
                                    type = d.type,
                                    created_at = d.created_at,
                                    created_by = d.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    updated_at = d.updated_at,
                                    updated_by = d.updated_by,
                                    updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                    deleted_at = d.deleted_at,
                                })
                                .AsEnumerable()
                                .OrderBy(c => c.name)
                                .ToList<dynamic>();
                        }
                    }
                    else
                    {

                        if (status == null || status == "" || status == "null")
                        {
                            if (name == null || name == "" || name == "null")
                            {
                                //data = context.DealSuppliers.Where(x => (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                //                    .AsEnumerable()
                                //                    .OrderBy(c => c.name)
                                //                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                //                    .Take(validFilter.PageSize)
                                //                    .ToList<dynamic>();

                                data = (
                                    from d in context.DealSuppliers.Where(x => (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                    join up in context.UserProfiles on d.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join file_vat_20 in context.DealUploadFiles on d.file_vat_20 equals file_vat_20.id into _file_vat_20
                                    from deal_file_vat_20 in _file_vat_20.DefaultIfEmpty()
                                    join file_certificate in context.DealUploadFiles on d.file_certificate equals file_certificate.id into _file_certificate
                                    from deal_file_certificate in _file_certificate.DefaultIfEmpty()
                                    join file_bookbank in context.DealUploadFiles on d.file_bookbank equals file_bookbank.id into _file_bookbank
                                    from deal_file_bookbank in _file_bookbank.DefaultIfEmpty()
                                    join file_accept in context.DealUploadFiles on d.file_accept equals file_accept.id into _file_accept
                                    from deal_file_accept in _file_accept.DefaultIfEmpty()
                                    join file_major in context.DealUploadFiles on d.major equals file_major.id into _file_major
                                    from deal_file_major in _file_major.DefaultIfEmpty()

                                    select new
                                    {
                                        id = d.id,
                                        name = d.name,
                                        description = d.description,
                                        link = d.link,
                                        major = deal_file_major.deal_file_path,
                                        amount_major = d.amount_major,
                                        doc = d.doc,
                                        file_vat_20 = deal_file_vat_20.deal_file_path,
                                        file_certificate = deal_file_certificate.deal_file_path,
                                        file_bookbank = deal_file_bookbank.deal_file_path,
                                        file_accept = deal_file_accept.deal_file_path,
                                        remark = d.remark,
                                        status = d.status,
                                        contact_name = (is_Admin == 1 ? d.contact_name : null),
                                        contact_phone = (is_Admin == 1 ? d.contact_phone : null),
                                        contact_email = (is_Admin == 1 ? d.contact_email : null),
                                        type = d.type,
                                        created_at = d.created_at,
                                        created_by = d.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = d.updated_at,
                                        updated_by = d.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        deleted_at = d.deleted_at,
                                    })
                                    .AsEnumerable()
                                    .OrderBy(c => c.name)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)
                                    .ToList<dynamic>();

                                count = context.DealSuppliers.Where(x => (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                                    .AsEnumerable()
                                                    .OrderBy(c => c.name)
                                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                    //.Take(validFilter.PageSize)
                                                    .ToList<dynamic>().Count();
                            }
                            else
                            {
                                //data = context.DealSuppliers.Where(x => (x.name.Contains(name) || x.description.Contains(name)) && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                //                    .AsEnumerable()
                                //                    .OrderBy(c => c.name)
                                //                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                //                    .Take(validFilter.PageSize)
                                //                    .ToList<dynamic>();

                                data = (
                                    from d in context.DealSuppliers.Where(x => (x.name.Contains(name) || x.description.Contains(name)) && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                    join up in context.UserProfiles on d.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join file_vat_20 in context.DealUploadFiles on d.file_vat_20 equals file_vat_20.id into _file_vat_20
                                    from deal_file_vat_20 in _file_vat_20.DefaultIfEmpty()
                                    join file_certificate in context.DealUploadFiles on d.file_certificate equals file_certificate.id into _file_certificate
                                    from deal_file_certificate in _file_certificate.DefaultIfEmpty()
                                    join file_bookbank in context.DealUploadFiles on d.file_bookbank equals file_bookbank.id into _file_bookbank
                                    from deal_file_bookbank in _file_bookbank.DefaultIfEmpty()
                                    join file_accept in context.DealUploadFiles on d.file_accept equals file_accept.id into _file_accept
                                    from deal_file_accept in _file_accept.DefaultIfEmpty()
                                    join file_major in context.DealUploadFiles on d.major equals file_major.id into _file_major
                                    from deal_file_major in _file_major.DefaultIfEmpty()

                                    select new
                                    {
                                        id = d.id,
                                        name = d.name,
                                        description = d.description,
                                        link = d.link,
                                        major = deal_file_major.deal_file_path,
                                        amount_major = d.amount_major,
                                        doc = d.doc,
                                        file_vat_20 = deal_file_vat_20.deal_file_path,
                                        file_certificate = deal_file_certificate.deal_file_path,
                                        file_bookbank = deal_file_bookbank.deal_file_path,
                                        file_accept = deal_file_accept.deal_file_path,
                                        remark = d.remark,
                                        status = d.status,
                                        contact_name = (is_Admin == 1 ? d.contact_name : null),
                                        contact_phone = (is_Admin == 1 ? d.contact_phone : null),
                                        contact_email = (is_Admin == 1 ? d.contact_email : null),
                                        type = d.type,
                                        created_at = d.created_at,
                                        created_by = d.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = d.updated_at,
                                        updated_by = d.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        deleted_at = d.deleted_at,
                                    })
                                    .AsEnumerable()
                                    .OrderBy(c => c.name)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)
                                    .ToList<dynamic>();

                                count = context.DealSuppliers.Where(x => (x.name.Contains(name) || x.description.Contains(name)) && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                                    .AsEnumerable()
                                                    .OrderBy(c => c.name)
                                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                    //.Take(validFilter.PageSize)
                                                    .ToList<dynamic>().Count();
                            }
                        }
                        else
                        {
                            if (name == null || name == "" || name == "null")
                            {
                                //data = context.DealSuppliers.Where(x => x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == "") && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                //                    .AsEnumerable()
                                //                    .OrderBy(c => c.name)
                                //                    .ToList<dynamic>();

                                data = (
                                    from d in context.DealSuppliers.Where(x => x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == "") && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                    join up in context.UserProfiles on d.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join file_vat_20 in context.DealUploadFiles on d.file_vat_20 equals file_vat_20.id into _file_vat_20
                                    from deal_file_vat_20 in _file_vat_20.DefaultIfEmpty()
                                    join file_certificate in context.DealUploadFiles on d.file_certificate equals file_certificate.id into _file_certificate
                                    from deal_file_certificate in _file_certificate.DefaultIfEmpty()
                                    join file_bookbank in context.DealUploadFiles on d.file_bookbank equals file_bookbank.id into _file_bookbank
                                    from deal_file_bookbank in _file_bookbank.DefaultIfEmpty()
                                    join file_accept in context.DealUploadFiles on d.file_accept equals file_accept.id into _file_accept
                                    from deal_file_accept in _file_accept.DefaultIfEmpty()
                                    join file_major in context.DealUploadFiles on d.major equals file_major.id into _file_major
                                    from deal_file_major in _file_major.DefaultIfEmpty()

                                    select new
                                    {
                                        id = d.id,
                                        name = d.name,
                                        description = d.description,
                                        link = d.link,
                                        major = deal_file_major.deal_file_path,
                                        amount_major = d.amount_major,
                                        doc = d.doc,
                                        file_vat_20 = deal_file_vat_20.deal_file_path,
                                        file_certificate = deal_file_certificate.deal_file_path,
                                        file_bookbank = deal_file_bookbank.deal_file_path,
                                        file_accept = deal_file_accept.deal_file_path,
                                        remark = d.remark,
                                        status = d.status,
                                        contact_name = (is_Admin == 1 ? d.contact_name : null),
                                        contact_phone = (is_Admin == 1 ? d.contact_phone : null),
                                        contact_email = (is_Admin == 1 ? d.contact_email : null),
                                        type = d.type,
                                        created_at = d.created_at,
                                        created_by = d.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = d.updated_at,
                                        updated_by = d.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        deleted_at = d.deleted_at,
                                    })
                                    .AsEnumerable()
                                    .OrderBy(c => c.name)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)
                                    .ToList<dynamic>();

                                count = context.DealSuppliers.Where(x => x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == "") && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                                    .AsEnumerable()
                                                    .OrderBy(c => c.name)
                                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                    //.Take(validFilter.PageSize)
                                                    .ToList<dynamic>().Count();
                            }
                            else
                            {
                                //data = context.DealSuppliers.Where(x => (x.name.Contains(name) || x.description.Contains(name)) && x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                //                    .AsEnumerable()
                                //                    .OrderBy(c => c.name)
                                //                    .ToList<dynamic>();

                                data = (
                                    from d in context.DealSuppliers.Where(x => (x.name.Contains(name) || x.description.Contains(name)) && x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                    join up in context.UserProfiles on d.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on d.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join file_vat_20 in context.DealUploadFiles on d.file_vat_20 equals file_vat_20.id into _file_vat_20
                                    from deal_file_vat_20 in _file_vat_20.DefaultIfEmpty()
                                    join file_certificate in context.DealUploadFiles on d.file_certificate equals file_certificate.id into _file_certificate
                                    from deal_file_certificate in _file_certificate.DefaultIfEmpty()
                                    join file_bookbank in context.DealUploadFiles on d.file_bookbank equals file_bookbank.id into _file_bookbank
                                    from deal_file_bookbank in _file_bookbank.DefaultIfEmpty()
                                    join file_accept in context.DealUploadFiles on d.file_accept equals file_accept.id into _file_accept
                                    from deal_file_accept in _file_accept.DefaultIfEmpty()
                                    join file_major in context.DealUploadFiles on d.major equals file_major.id into _file_major
                                    from deal_file_major in _file_major.DefaultIfEmpty()

                                    select new
                                    {
                                        id = d.id,
                                        name = d.name,
                                        description = d.description,
                                        link = d.link,
                                        major = deal_file_major.deal_file_path,
                                        amount_major = d.amount_major,
                                        doc = d.doc,
                                        file_vat_20 = deal_file_vat_20.deal_file_path,
                                        file_certificate = deal_file_certificate.deal_file_path,
                                        file_bookbank = deal_file_bookbank.deal_file_path,
                                        file_accept = deal_file_accept.deal_file_path,
                                        remark = d.remark,
                                        status = d.status,
                                        contact_name = (is_Admin == 1 ? d.contact_name : null),
                                        contact_phone = (is_Admin == 1 ? d.contact_phone : null),
                                        contact_email = (is_Admin == 1 ? d.contact_email : null),
                                        type = d.type,
                                        created_at = d.created_at,
                                        created_by = d.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = d.updated_at,
                                        updated_by = d.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        deleted_at = d.deleted_at,
                                    })
                                    .AsEnumerable()
                                    .OrderBy(c => c.name)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)
                                    .ToList<dynamic>();

                                count = context.DealSuppliers.Where(x => (x.name.Contains(name) || x.description.Contains(name)) && x.status.ToString() == status && (x.deleted_at == null || x.deleted_at.ToString() == ""))
                                                    .AsEnumerable()
                                                    .OrderBy(c => c.name)
                                                    //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                    //.Take(validFilter.PageSize)
                                                    .ToList<dynamic>().Count();
                            }
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
        [HttpGet("GetDealRequested")]
        public IActionResult GetDealRequested(string name, [FromQuery] PaginationFilter filter)
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
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy 23:59:59").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);

                        if (name == null || name == "" || name == "null")
                        {
                            data = (
                                from deal_requested in context.DealRequesteds.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal_requested.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation_ref = deal_requested.deal_quatation_ref,
                                    deal_quatation_file_ref = deal_requested.deal_quatation_file_ref,
                                    deal_quatation_link_ref = deal_requested.deal_quatation_link_ref,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_quatation_link = deal_requested.deal_quatation_link,
                                    deal_po_file = deal_requested.deal_po_file,
                                    deal_po_link = deal_requested.deal_po_link,
                                    deal_requested_file = deal_requested.deal_requested_file,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    //deal_id = context.DealCodes.Where(x => x.deal_reference == deal_requested.deal_reference).Select(x => x.deal_id).FirstOrDefault(),
                                    deal_id = deal_requested.deal_id,
                                    deal_name = context.Deals.Where(x => x.id == context.DealCodes.Where(x => x.deal_id == deal_requested.deal_id).Select(x => x.deal_id).FirstOrDefault()).Select(x => x.deal_name).FirstOrDefault(),
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
                                    updated_at = deal_requested.updated_at,
                                    updated_by = deal_requested.updated_by,
                                    status_requested = deal_requested.status
                                })
                                .AsEnumerable()
                                .OrderByDescending(x => x.id).ThenBy(x => x.status_requested)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();




                            count = (
                                from deal_requested in context.DealRequesteds.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal_requested.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_requested_file = deal_requested.deal_requested_file,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
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
                                from deal_requested in context.DealRequesteds.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal_requested.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation_ref = deal_requested.deal_quatation_ref,
                                    deal_quatation_link_ref = deal_requested.deal_quatation_link_ref,
                                    deal_quatation_file_ref = deal_requested.deal_quatation_file_ref,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_link = deal_requested.deal_quatation_link,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_po_file = deal_requested.deal_po_file,
                                    deal_po_link = deal_requested.deal_po_link,
                                    deal_requested_file = deal_requested.deal_requested_file,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    //deal_id = context.DealCodes.Where(x => x.deal_reference == deal_requested.deal_reference).Select(x => x.deal_id).FirstOrDefault(),
                                    deal_id = deal_requested.deal_id,
                                    deal_name = context.Deals.Where(x => x.id == context.DealCodes.Where(x => x.deal_id == deal_requested.deal_id).Select(x => x.deal_id).FirstOrDefault()).Select(x => x.deal_name).FirstOrDefault(),
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
                                    updated_at = deal_requested.updated_at,
                                    updated_by = deal_requested.updated_by,
                                    status_requested = deal_requested.status
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_customer.Contains(name))
                                .OrderByDescending(x => x.id).ThenBy(x => x.status_requested)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();


                            count = (
                                from deal_requested in context.DealRequesteds.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal_requested.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_requested_file = deal_requested.deal_requested_file,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_customer.Contains(name))
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
                                from deal_requested in context.DealRequesteds
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal_requested.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation_ref = deal_requested.deal_quatation_ref,
                                    deal_quatation_link_ref = deal_requested.deal_quatation_link_ref,
                                    deal_quatation_file_ref = deal_requested.deal_quatation_file_ref,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_link = deal_requested.deal_quatation_link,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_po_file = deal_requested.deal_po_file,
                                    deal_po_link = deal_requested.deal_po_link,
                                    deal_requested_file = deal_requested.deal_requested_file,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    //deal_id = context.DealCodes.Where(x => x.deal_reference == deal_requested.deal_reference).Select(x => x.deal_id).FirstOrDefault(),
                                    deal_id = deal_requested.deal_id,
                                    deal_name = context.Deals.Where(x => x.id == context.DealCodes.Where(x => x.deal_id == deal_requested.deal_id).Select(x => x.deal_id).FirstOrDefault()).Select(x => x.deal_name).FirstOrDefault(),
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
                                    status_requested = deal_requested.status
                                })
                                .AsEnumerable()
                                .OrderByDescending(x => x.id).ThenBy(x => x.status_requested)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();




                            count = (
                                from deal_requested in context.DealRequesteds
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal_requested.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_requested_file = deal_requested.deal_requested_file,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
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
                                from deal_requested in context.DealRequesteds
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal_requested.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation_ref = deal_requested.deal_quatation_ref,
                                    deal_quatation_link_ref = deal_requested.deal_quatation_link_ref,
                                    deal_quatation_file_ref = deal_requested.deal_quatation_file_ref,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_link = deal_requested.deal_quatation_link,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_po_file = deal_requested.deal_po_file,
                                    deal_po_link = deal_requested.deal_po_link,
                                    deal_requested_file = deal_requested.deal_requested_file,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    //deal_id = context.DealCodes.Where(x => x.deal_reference == deal_requested.deal_reference).Select(x => x.deal_id).FirstOrDefault(),
                                    deal_id = deal_requested.deal_id,
                                    deal_name = context.Deals.Where(x => x.id == context.DealCodes.Where(x => x.deal_id == deal_requested.deal_id).Select(x => x.deal_id).FirstOrDefault()).Select(x => x.deal_name).FirstOrDefault(),
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
                                    updated_at = deal_requested.updated_at,
                                    updated_by = deal_requested.updated_by,
                                    status_requested = deal_requested.status
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_customer.Contains(name))
                                .OrderByDescending(x => x.id).ThenBy(x => x.status_requested)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();


                            count = (
                                from deal_requested in context.DealRequesteds
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    deal_supplier_id = deal_requested.deal_supplier_id,
                                    deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_requested_file = deal_requested.deal_requested_file,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_customer.Contains(name))
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

        //[Authorize]
        [HttpGet("DownloadDeal")]
        public async Task<IActionResult> DownloadDeal(string name, [FromQuery] PaginationFilter filter, int? coupon_type, string url_domain)
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
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy 23:59:59").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);

                        if (name == null || name == "" || name == "null")
                        {
                            if (coupon_type != 2)
                            {
                                //data = (
                                //    from a in context.Deals.Where(x => x.deal_coupon_type != 2 && x.created_at >= date_from && x.created_at <= date_to)
                                //    join up in context.UserProfiles on a.created_by equals up.id into g
                                //    from x in g.DefaultIfEmpty()
                                //    join up in context.UserProfiles on a.updated_by equals up.id into h
                                //    from uby in h.DefaultIfEmpty()
                                //    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                //    from deal_customer in _customer.DefaultIfEmpty()
                                //    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                //    from deal_supplier in _supplier.DefaultIfEmpty()

                                //    select new
                                //    {
                                //        deal_id = a.id,
                                //        deal_customer_id = a.deal_customer_id,
                                //        deal_customer = deal_customer.description,
                                //        deal_supplier_id = a.deal_supplier_id,
                                //        deal_supplier = deal_supplier.description,
                                //        deal_name = a.deal_name,
                                //        deal_coupon_image = (a.deal_coupon_image == null ? String.Empty : url_domain + "/" + a.deal_coupon_image.ToString()),
                                //        term_condtion_supplier = deal_supplier.description,
                                //        status = a.status,
                                //        remark = a.remark,
                                //        created_at = a.created_at,
                                //        created_by = a.created_by,
                                //        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                //        updated_at = a.updated_at,
                                //        updated_by = a.updated_by,
                                //        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                //    })
                                //    .AsEnumerable()
                                //    .OrderBy(x => x.created_at)
                                //    .ToList();


                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2 && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = (deal_file_coupon_img.deal_file_path == null ? String.Empty : url_domain + "/" + deal_file_coupon_img.deal_file_path.ToString()),
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                        .AsEnumerable()
                                        .OrderBy(x => x.created_at)


                                        .Select(x => new
                                        {
                                            deal_id = x.deal_id,
                                            //deal_customer_id = x.deal_customer_id,
                                            //deal_customer = x.deal_customer,
                                            //deal_supplier_id = x.deal_supplier_id,
                                            deal_supplier = x.deal_supplier,
                                            deal_name = x.deal_name,
                                            remark = x.remark,
                                            status = x.status,
                                            created_at = x.created_at,
                                            //created_by = x.created_by,
                                            created_by_name = x.created_by_name,
                                            updated_at = x.updated_at,
                                            //updated_by = x.updated_by,
                                            updated_by_name = x.updated_by_name,
                                            //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                            //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                            quantity_code = x.quantity_code,
                                            all_quantity_code = x.all_quantity_code,
                                            quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                            //deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                            //deal_coupon_type = x.deal_coupon_type,
                                            deal_coupon_value = x.deal_coupon_value,
                                            //deal_permission = x.deal_permission,
                                            //deal_quatation = x.deal_quatation,
                                            //deal_quatation_link = x.deal_quatation_link,
                                            //deal_merchant_id = x.deal_merchant_id,
                                            //deal_merchant_name = x.deal_merchant_name,

                                            //deal_invoice = x.deal_invoice,
                                            //deal_file_invoice = x.deal_file_invoice,

                                            //deal_receipt = x.deal_receipt,
                                            deal_start_date = x.deal_start_date,
                                            deal_end_date = x.deal_end_date,
                                            //deal_coupon_name = x.deal_coupon_name,
                                            //deal_cost = x.deal_cost,
                                            //deal_condition = x.deal_condition,
                                            //deal_coupon_image = x.deal_coupon_image,
                                            //deal_major = x.deal_major,
                                            deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                            //deal_discount_value = x.deal_discount_value,
                                            //deal_summary_value = x.deal_summary_value,
                                            //deal_add_item = x.deal_add_item,
                                            //deal_add_amount = x.deal_add_amount,
                                            //deal_add_value = x.deal_add_value,
                                            //deal_pr_number = x.deal_pr_number,
                                            //deal_pr_file = x.deal_pr_file,
                                            //minimum_requested = x.minimum_requested,
                                            //maximum_requested = x.maximum_requested,
                                            //minimum_order = x.minimum_order,
                                            //maximum_order = x.maximum_order
                                        })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList();
                            }
                            else
                            {
                                //data = (
                                //    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type && x.created_at >= date_from && x.created_at <= date_to)
                                //    join up in context.UserProfiles on a.created_by equals up.id into g
                                //    from x in g.DefaultIfEmpty()
                                //    join up in context.UserProfiles on a.updated_by equals up.id into h
                                //    from uby in h.DefaultIfEmpty()
                                //    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                //    from deal_customer in _customer.DefaultIfEmpty()
                                //    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                //    from deal_supplier in _supplier.DefaultIfEmpty()

                                //    select new
                                //    {
                                //        deal_id = a.id,
                                //        deal_customer_id = a.deal_customer_id,
                                //        deal_customer = deal_customer.description,
                                //        deal_supplier_id = a.deal_supplier_id,
                                //        deal_supplier = deal_supplier.description,
                                //        deal_name = a.deal_name,
                                //        deal_coupon_image = (a.deal_coupon_image == null ? String.Empty : url_domain + "/" + a.deal_coupon_image.ToString()),
                                //        term_condtion_supplier = deal_supplier.description,
                                //        status = a.status,
                                //        remark = a.remark,
                                //        created_at = a.created_at,
                                //        created_by = a.created_by,
                                //        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                //        updated_at = a.updated_at,
                                //        updated_by = a.updated_by,
                                //        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                //    })
                                //    .AsEnumerable()
                                //    .OrderBy(x => x.created_at)
                                //    .ToList();


                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.description,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                        .AsEnumerable()
                                        .OrderBy(x => x.created_at)


                                        .Select(x => new
                                        {
                                            deal_id = x.deal_id,
                                            //deal_customer_id = x.deal_customer_id,
                                            //deal_customer = x.deal_customer,
                                            //deal_supplier_id = x.deal_supplier_id,
                                            deal_supplier = x.deal_supplier,
                                            deal_name = x.deal_name,
                                            remark = x.remark,
                                            status = x.status,
                                            created_at = x.created_at,
                                            //created_by = x.created_by,
                                            created_by_name = x.created_by_name,
                                            updated_at = x.updated_at,
                                            //updated_by = x.updated_by,
                                            updated_by_name = x.updated_by_name,
                                            //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                            //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                            quantity_code = x.quantity_code,
                                            all_quantity_code = x.all_quantity_code,
                                            quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                            //deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                            //deal_coupon_type = x.deal_coupon_type,
                                            deal_coupon_value = x.deal_coupon_value,
                                            //deal_permission = x.deal_permission,
                                            //deal_quatation = x.deal_quatation,
                                            //deal_quatation_link = x.deal_quatation_link,
                                            //deal_merchant_id = x.deal_merchant_id,
                                            //deal_merchant_name = x.deal_merchant_name,

                                            //deal_invoice = x.deal_invoice,
                                            //deal_file_invoice = x.deal_file_invoice,

                                            //deal_receipt = x.deal_receipt,
                                            deal_start_date = x.deal_start_date,
                                            deal_end_date = x.deal_end_date,
                                            //deal_coupon_name = x.deal_coupon_name,
                                            //deal_cost = x.deal_cost,
                                            //deal_condition = x.deal_condition,
                                            //deal_coupon_image = x.deal_coupon_image,
                                            //deal_major = x.deal_major,
                                            deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                            //deal_discount_value = x.deal_discount_value,
                                            //deal_summary_value = x.deal_summary_value,
                                            //deal_add_item = x.deal_add_item,
                                            //deal_add_amount = x.deal_add_amount,
                                            //deal_add_value = x.deal_add_value,
                                            //deal_pr_number = x.deal_pr_number,
                                            //deal_pr_file = x.deal_pr_file,
                                            //minimum_requested = x.minimum_requested,
                                            //maximum_requested = x.maximum_requested,
                                            //minimum_order = x.minimum_order,
                                            //maximum_order = x.maximum_order
                                        })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList();
                            }
                        }
                        else
                        {
                            if (coupon_type != 2)
                            {
                                //data = (
                                //    from a in context.Deals.Where(x => x.deal_coupon_type != 2 && x.created_at >= date_from && x.created_at <= date_to)
                                //    join up in context.UserProfiles on a.created_by equals up.id into g
                                //    from x in g.DefaultIfEmpty()
                                //    join up in context.UserProfiles on a.updated_by equals up.id into h
                                //    from uby in h.DefaultIfEmpty()
                                //    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                //    from deal_customer in _customer.DefaultIfEmpty()
                                //    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                //    from deal_supplier in _supplier.DefaultIfEmpty()

                                //    select new
                                //    {
                                //        deal_id = a.id,
                                //        deal_customer_id = a.deal_customer_id,
                                //        deal_customer = deal_customer.description,
                                //        deal_supplier_id = a.deal_supplier_id,
                                //        deal_supplier = deal_supplier.description,
                                //        deal_name = a.deal_name,
                                //        deal_coupon_image = (a.deal_coupon_image == null ? String.Empty : url_domain + "/" + a.deal_coupon_image.ToString()),
                                //        term_condtion_supplier = deal_supplier.description,
                                //        status = a.status,
                                //        remark = a.remark,
                                //        created_at = a.created_at,
                                //        created_by = a.created_by,
                                //        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                //        updated_at = a.updated_at,
                                //        updated_by = a.updated_by,
                                //        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                //    })
                                //    .AsEnumerable()
                                //    .Where(x => x.deal_name.Contains(name))
                                //    .OrderBy(x => x.created_at)
                                //    .ToList();

                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2 && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name))
                                    .OrderBy(x => x.created_at)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        //deal_customer_id = x.deal_customer_id,
                                        //deal_customer = x.deal_customer,
                                        //deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        //created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        //updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        //deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        //deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        //deal_permission = x.deal_permission,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,
                                        //deal_merchant_id = x.deal_merchant_id,
                                        //deal_merchant_name = x.deal_merchant_name,

                                        //deal_invoice = x.deal_invoice,
                                        //deal_file_invoice = x.deal_file_invoice,

                                        //deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        //deal_coupon_name = x.deal_coupon_name,
                                        //deal_cost = x.deal_cost,
                                        //deal_condition = x.deal_condition,
                                        //deal_coupon_image = x.deal_coupon_image,
                                        //deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        //deal_discount_value = x.deal_discount_value,
                                        //deal_summary_value = x.deal_summary_value,
                                        //deal_add_item = x.deal_add_item,
                                        //deal_add_amount = x.deal_add_amount,
                                        //deal_add_value = x.deal_add_value,
                                        //deal_pr_number = x.deal_pr_number,
                                        //deal_pr_file = x.deal_pr_file,
                                        //minimum_requested = x.minimum_requested,
                                        //maximum_requested = x.maximum_requested,
                                        //minimum_order = x.minimum_order,
                                        //maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList();
                            }
                            else
                            {
                                //data = (
                                //    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type && x.created_at >= date_from && x.created_at <= date_to)
                                //    join up in context.UserProfiles on a.created_by equals up.id into g
                                //    from x in g.DefaultIfEmpty()
                                //    join up in context.UserProfiles on a.updated_by equals up.id into h
                                //    from uby in h.DefaultIfEmpty()
                                //    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                //    from deal_customer in _customer.DefaultIfEmpty()
                                //    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                //    from deal_supplier in _supplier.DefaultIfEmpty()

                                //    select new
                                //    {
                                //        deal_id = a.id,
                                //        deal_customer_id = a.deal_customer_id,
                                //        deal_customer = deal_customer.description,
                                //        deal_supplier_id = a.deal_supplier_id,
                                //        deal_supplier = deal_supplier.description,
                                //        deal_name = a.deal_name,
                                //        deal_coupon_image = (a.deal_coupon_image == null ? String.Empty : url_domain + "/" + a.deal_coupon_image.ToString()),
                                //        term_condtion_supplier = deal_supplier.description,
                                //        status = a.status,
                                //        remark = a.remark,
                                //        created_at = a.created_at,
                                //        created_by = a.created_by,
                                //        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                //        updated_at = a.updated_at,
                                //        updated_by = a.updated_by,
                                //        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                //    })
                                //    .AsEnumerable()
                                //    .Where(x => x.deal_name.Contains(name))
                                //    .OrderBy(x => x.created_at)
                                //    .ToList();


                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type && x.created_at >= date_from && x.created_at <= date_to)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name))
                                    .OrderBy(x => x.created_at)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        //deal_customer_id = x.deal_customer_id,
                                        //deal_customer = x.deal_customer,
                                        //deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        //created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        //updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        //deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        //deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        //deal_permission = x.deal_permission,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,
                                        //deal_merchant_id = x.deal_merchant_id,
                                        //deal_merchant_name = x.deal_merchant_name,

                                        //deal_invoice = x.deal_invoice,
                                        //deal_file_invoice = x.deal_file_invoice,

                                        //deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        //deal_coupon_name = x.deal_coupon_name,
                                        //deal_cost = x.deal_cost,
                                        //deal_condition = x.deal_condition,
                                        //deal_coupon_image = x.deal_coupon_image,
                                        //deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        //deal_discount_value = x.deal_discount_value,
                                        //deal_summary_value = x.deal_summary_value,
                                        //deal_add_item = x.deal_add_item,
                                        //deal_add_amount = x.deal_add_amount,
                                        //deal_add_value = x.deal_add_value,
                                        //deal_pr_number = x.deal_pr_number,
                                        //deal_pr_file = x.deal_pr_file,
                                        //minimum_requested = x.minimum_requested,
                                        //maximum_requested = x.maximum_requested,
                                        //minimum_order = x.minimum_order,
                                        //maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList();
                            }
                        }
                    }
                    else
                    {
                        if (name == null || name == "" || name == "null")
                        {
                            if (coupon_type != 2)
                            {
                                //data = (
                                //    from a in context.Deals.Where(x => x.deal_coupon_type != 2)
                                //    join up in context.UserProfiles on a.created_by equals up.id into g
                                //    from x in g.DefaultIfEmpty()
                                //    join up in context.UserProfiles on a.updated_by equals up.id into h
                                //    from uby in h.DefaultIfEmpty()
                                //    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                //    from deal_customer in _customer.DefaultIfEmpty()
                                //    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                //    from deal_supplier in _supplier.DefaultIfEmpty()

                                //    select new
                                //    {
                                //        deal_id = a.id,
                                //        deal_customer_id = a.deal_customer_id,
                                //        deal_customer = deal_customer.description,
                                //        deal_supplier_id = a.deal_supplier_id,
                                //        deal_supplier = deal_supplier.description,
                                //        deal_name = a.deal_name,
                                //        deal_coupon_image = (a.deal_coupon_image == null ? String.Empty : url_domain + "/" + a.deal_coupon_image.ToString()),
                                //        term_condtion_supplier = deal_supplier.description,
                                //        status = a.status,
                                //        remark = a.remark,
                                //        created_at = a.created_at,
                                //        created_by = a.created_by,
                                //        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                //        updated_at = a.updated_at,
                                //        updated_by = a.updated_by,
                                //        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                //    })
                                //    .AsEnumerable()
                                //    .OrderBy(x => x.created_at)
                                //    .ToList();

                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .OrderBy(x => x.created_at)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        //deal_customer_id = x.deal_customer_id,
                                        //deal_customer = x.deal_customer,
                                        //deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        //created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        //updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        //deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        //deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        //deal_permission = x.deal_permission,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,
                                        //deal_merchant_id = x.deal_merchant_id,
                                        //deal_merchant_name = x.deal_merchant_name,

                                        //deal_invoice = x.deal_invoice,
                                        //deal_file_invoice = x.deal_file_invoice,

                                        //deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        //deal_coupon_name = x.deal_coupon_name,
                                        //deal_cost = x.deal_cost,
                                        //deal_condition = x.deal_condition,
                                        //deal_coupon_image = x.deal_coupon_image,
                                        //deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        //deal_discount_value = x.deal_discount_value,
                                        //deal_summary_value = x.deal_summary_value,
                                        //deal_add_item = x.deal_add_item,
                                        //deal_add_amount = x.deal_add_amount,
                                        //deal_add_value = x.deal_add_value,
                                        //deal_pr_number = x.deal_pr_number,
                                        //deal_pr_file = x.deal_pr_file,
                                        //minimum_requested = x.minimum_requested,
                                        //maximum_requested = x.maximum_requested,
                                        //minimum_order = x.minimum_order,
                                        //maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList();
                            }
                            else
                            {
                                //data = (
                                //    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type)
                                //    join up in context.UserProfiles on a.created_by equals up.id into g
                                //    from x in g.DefaultIfEmpty()
                                //    join up in context.UserProfiles on a.updated_by equals up.id into h
                                //    from uby in h.DefaultIfEmpty()
                                //    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                //    from deal_customer in _customer.DefaultIfEmpty()
                                //    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                //    from deal_supplier in _supplier.DefaultIfEmpty()

                                //    select new
                                //    {
                                //        deal_id = a.id,
                                //        deal_customer_id = a.deal_customer_id,
                                //        deal_customer = deal_customer.description,
                                //        deal_supplier_id = a.deal_supplier_id,
                                //        deal_supplier = deal_supplier.description,
                                //        deal_name = a.deal_name,
                                //        deal_coupon_image = (a.deal_coupon_image == null ? String.Empty : url_domain + "/" + a.deal_coupon_image.ToString()),
                                //        term_condtion_supplier = deal_supplier.description,
                                //        status = a.status,
                                //        remark = a.remark,
                                //        created_at = a.created_at,
                                //        created_by = a.created_by,
                                //        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                //        updated_at = a.updated_at,
                                //        updated_by = a.updated_by,
                                //        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                //    })
                                //    .AsEnumerable()
                                //    .OrderBy(x => x.created_at)
                                //    .ToList();

                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .OrderBy(x => x.created_at)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        //deal_customer_id = x.deal_customer_id,
                                        //deal_customer = x.deal_customer,
                                        //deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        //created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        //updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        //deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        //deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        //deal_permission = x.deal_permission,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,
                                        //deal_merchant_id = x.deal_merchant_id,
                                        //deal_merchant_name = x.deal_merchant_name,

                                        //deal_invoice = x.deal_invoice,
                                        //deal_file_invoice = x.deal_file_invoice,

                                        //deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        //deal_coupon_name = x.deal_coupon_name,
                                        //deal_cost = x.deal_cost,
                                        //deal_condition = x.deal_condition,
                                        //deal_coupon_image = x.deal_coupon_image,
                                        //deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        //deal_discount_value = x.deal_discount_value,
                                        //deal_summary_value = x.deal_summary_value,
                                        //deal_add_item = x.deal_add_item,
                                        //deal_add_amount = x.deal_add_amount,
                                        //deal_add_value = x.deal_add_value,
                                        //deal_pr_number = x.deal_pr_number,
                                        //deal_pr_file = x.deal_pr_file,
                                        //minimum_requested = x.minimum_requested,
                                        //maximum_requested = x.maximum_requested,
                                        //minimum_order = x.minimum_order,
                                        //maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList();
                            }
                        }
                        else
                        {
                            if (coupon_type != 2)
                            {
                                //data = (
                                //    from a in context.Deals.Where(x => x.deal_coupon_type != 2)
                                //    join up in context.UserProfiles on a.created_by equals up.id into g
                                //    from x in g.DefaultIfEmpty()
                                //    join up in context.UserProfiles on a.updated_by equals up.id into h
                                //    from uby in h.DefaultIfEmpty()
                                //    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                //    from deal_customer in _customer.DefaultIfEmpty()
                                //    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                //    from deal_supplier in _supplier.DefaultIfEmpty()

                                //    select new
                                //    {
                                //        deal_id = a.id,
                                //        deal_customer_id = a.deal_customer_id,
                                //        deal_customer = deal_customer.description,
                                //        deal_supplier_id = a.deal_supplier_id,
                                //        deal_supplier = deal_supplier.description,
                                //        deal_name = a.deal_name,
                                //        deal_coupon_image = (a.deal_coupon_image == null ? String.Empty : url_domain + "/" + a.deal_coupon_image.ToString()),
                                //        term_condtion_supplier = deal_supplier.description,
                                //        status = a.status,
                                //        remark = a.remark,
                                //        created_at = a.created_at,
                                //        created_by = a.created_by,
                                //        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                //        updated_at = a.updated_at,
                                //        updated_by = a.updated_by,
                                //        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                //    })
                                //    .AsEnumerable()
                                //    .Where(x => x.deal_name.Contains(name))
                                //    .OrderBy(x => x.created_at)
                                //    .ToList();

                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type != 2)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name))
                                    .OrderBy(x => x.created_at)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        //deal_customer_id = x.deal_customer_id,
                                        //deal_customer = x.deal_customer,
                                        //deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        //created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        //updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        //deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        //deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        //deal_permission = x.deal_permission,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,
                                        //deal_merchant_id = x.deal_merchant_id,
                                        //deal_merchant_name = x.deal_merchant_name,

                                        //deal_invoice = x.deal_invoice,
                                        //deal_file_invoice = x.deal_file_invoice,

                                        //deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        //deal_coupon_name = x.deal_coupon_name,
                                        //deal_cost = x.deal_cost,
                                        //deal_condition = x.deal_condition,
                                        //deal_coupon_image = x.deal_coupon_image,
                                        //deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        //deal_discount_value = x.deal_discount_value,
                                        //deal_summary_value = x.deal_summary_value,
                                        //deal_add_item = x.deal_add_item,
                                        //deal_add_amount = x.deal_add_amount,
                                        //deal_add_value = x.deal_add_value,
                                        //deal_pr_number = x.deal_pr_number,
                                        //deal_pr_file = x.deal_pr_file,
                                        //minimum_requested = x.minimum_requested,
                                        //maximum_requested = x.maximum_requested,
                                        //minimum_order = x.minimum_order,
                                        //maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList();
                            }
                            else
                            {
                                //data = (
                                //    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type)
                                //    join up in context.UserProfiles on a.created_by equals up.id into g
                                //    from x in g.DefaultIfEmpty()
                                //    join up in context.UserProfiles on a.updated_by equals up.id into h
                                //    from uby in h.DefaultIfEmpty()
                                //    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                //    from deal_customer in _customer.DefaultIfEmpty()
                                //    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                //    from deal_supplier in _supplier.DefaultIfEmpty()

                                //    select new
                                //    {
                                //        deal_id = a.id,
                                //        deal_customer_id = a.deal_customer_id,
                                //        deal_customer = deal_customer.description,
                                //        deal_supplier_id = a.deal_supplier_id,
                                //        deal_supplier = deal_supplier.description,
                                //        deal_name = a.deal_name,
                                //        deal_coupon_image = (a.deal_coupon_image == null ? String.Empty : url_domain + "/" + a.deal_coupon_image.ToString()),
                                //        term_condtion_supplier = deal_supplier.description,
                                //        status = a.status,
                                //        remark = a.remark,
                                //        created_at = a.created_at,
                                //        created_by = a.created_by,
                                //        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                //        updated_at = a.updated_at,
                                //        updated_by = a.updated_by,
                                //        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                                //    })
                                //    .AsEnumerable()
                                //    .Where(x => x.deal_name.Contains(name))
                                //    .OrderBy(x => x.created_at)
                                //    .ToList();

                                data = (
                                    from a in context.Deals.Where(x => x.deal_coupon_type == coupon_type)
                                    join up in context.UserProfiles on a.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on a.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join customer in context.DealCustomers on a.deal_customer_id equals customer.id into _customer
                                    from deal_customer in _customer.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on a.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()
                                    join file_qt in context.DealUploadFiles on a.deal_quatation_id equals file_qt.id into _file_qt
                                    from deal_file_qt in _file_qt.DefaultIfEmpty()
                                    join file_receipt in context.DealUploadFiles on a.deal_receipt_id equals file_receipt.id into _file_receipt
                                    from deal_file_receipt in _file_receipt.DefaultIfEmpty()
                                    join file_pr in context.DealUploadFiles on a.deal_pr_file_id equals file_pr.id into _file_pr
                                    from deal_file_pr in _file_pr.DefaultIfEmpty()
                                    join file_coupon_img in context.DealUploadFiles on a.deal_coupon_image_id equals file_coupon_img.id into _file_coupon_img
                                    from deal_file_coupon_img in _file_coupon_img.DefaultIfEmpty()
                                    join file_invoice in context.DealUploadFiles on a.deal_invoice_file_id equals file_invoice.id into _file_invoice
                                    from deal_file_invoice in _file_invoice.DefaultIfEmpty()

                                    select new
                                    {
                                        deal_id = a.id,
                                        deal_customer_id = a.deal_customer_id,
                                        deal_customer = deal_customer.name,
                                        deal_supplier_id = a.deal_supplier_id,
                                        deal_supplier = deal_supplier.name,
                                        deal_name = a.deal_name,
                                        remark = a.remark,
                                        status = a.status,
                                        created_at = a.created_at,
                                        created_by = a.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = a.updated_at,
                                        updated_by = a.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),

                                        deal_coupon_type = a.deal_coupon_type,
                                        deal_coupon_value = a.deal_coupon_value,
                                        deal_permission = a.deal_permission,

                                        deal_quatation = deal_file_qt.deal_file_path,
                                        deal_quatation_link = deal_file_qt.deal_file_link,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,

                                        deal_receipt = deal_file_receipt.deal_file_path,
                                        //deal_receipt = a.deal_receipt,

                                        deal_coupon_image = deal_file_coupon_img.deal_file_path,
                                        //deal_coupon_image = a.deal_coupon_image,

                                        deal_pr_file = deal_file_pr.deal_file_path,
                                        //deal_pr_file = a.deal_pr_file,

                                        deal_merchant_id = a.deal_merchant_id,
                                        deal_merchant_name = a.deal_merchant_name,

                                        deal_invoice = a.deal_invoice,
                                        deal_file_invoice = deal_file_invoice.deal_file_path,

                                        deal_start_date = a.deal_start_date,
                                        deal_end_date = a.deal_end_date,
                                        deal_coupon_name = a.deal_coupon_name,
                                        deal_cost = a.deal_cost,
                                        deal_condition = a.deal_condition,
                                        deal_major = a.deal_major,
                                        deal_discount_value = a.deal_discount_value,
                                        deal_summary_value = a.deal_summary_value,
                                        deal_add_item = a.deal_add_item,
                                        deal_add_amount = a.deal_add_amount,
                                        deal_add_value = a.deal_add_value,
                                        deal_pr_number = a.deal_pr_number,
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == a.id).AsEnumerable().ToList().Count(),
                                        minimum_requested = a.minimum_requested,
                                        maximum_requested = a.maximum_requested,
                                        minimum_order = a.minimum_order,
                                        maximum_order = a.maximum_order
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name))
                                    .OrderBy(x => x.created_at)


                                    .Select(x => new
                                    {
                                        deal_id = x.deal_id,
                                        //deal_customer_id = x.deal_customer_id,
                                        //deal_customer = x.deal_customer,
                                        //deal_supplier_id = x.deal_supplier_id,
                                        deal_supplier = x.deal_supplier,
                                        deal_name = x.deal_name,
                                        remark = x.remark,
                                        status = x.status,
                                        created_at = x.created_at,
                                        //created_by = x.created_by,
                                        created_by_name = x.created_by_name,
                                        updated_at = x.updated_at,
                                        //updated_by = x.updated_by,
                                        updated_by_name = x.updated_by_name,
                                        //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),
                                        //all_quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),

                                        quantity_code = x.quantity_code,
                                        all_quantity_code = x.all_quantity_code,
                                        quantity_requested_code = x.all_quantity_code - x.quantity_code,

                                        //deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        //deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        //deal_permission = x.deal_permission,
                                        //deal_quatation = x.deal_quatation,
                                        //deal_quatation_link = x.deal_quatation_link,
                                        //deal_merchant_id = x.deal_merchant_id,
                                        //deal_merchant_name = x.deal_merchant_name,

                                        //deal_invoice = x.deal_invoice,
                                        //deal_file_invoice = x.deal_file_invoice,

                                        //deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        //deal_coupon_name = x.deal_coupon_name,
                                        //deal_cost = x.deal_cost,
                                        //deal_condition = x.deal_condition,
                                        //deal_coupon_image = x.deal_coupon_image,
                                        //deal_major = x.deal_major,
                                        deal_remaining = CompareDate(x.deal_start_date, x.deal_end_date),
                                        //deal_discount_value = x.deal_discount_value,
                                        //deal_summary_value = x.deal_summary_value,
                                        //deal_add_item = x.deal_add_item,
                                        //deal_add_amount = x.deal_add_amount,
                                        //deal_add_value = x.deal_add_value,
                                        //deal_pr_number = x.deal_pr_number,
                                        //deal_pr_file = x.deal_pr_file,
                                        //minimum_requested = x.minimum_requested,
                                        //maximum_requested = x.maximum_requested,
                                        //minimum_order = x.minimum_order,
                                        //maximum_order = x.maximum_order
                                    })
                                        //.OrderByDescending(x => x.deal_remaining)
                                        .OrderByDescending(x => x.deal_remaining).ThenByDescending(x => x.quantity_requested_code)
                                        .ToList();
                            }
                        }
                    }

                    DataTable dt = ToDataTable(data);

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
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.Message.ToString() });
            }
        }

        [Authorize]
        [HttpGet("DownloadLog")]
        public async Task<IActionResult> DownloadLog(string name, [FromQuery] PaginationFilter filter)
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
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy 23:59:59").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);

                        if (name == null || name == "" || name == "null")
                        {
                            data = (
                                from l in context.DealLogs.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : "download"),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .ToList();

                        }
                        else
                        {
                            data = (
                                from l in context.DealLogs.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : "download"),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
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
                                from l in context.DealLogs
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : "download"),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .ToList();
                        }
                        else
                        {
                            data = (
                                from l in context.DealLogs
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.user equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    user = l.user,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    type = (l.type == 0 ? "upload" : "download"),
                                    deal_reference = l.deal_reference,
                                    deal_request_code = l.deal_request_code,
                                    qty_code = l.qty_code,
                                    bill_id = l.bill_id,
                                    remark = l.remark,
                                    link_file = l.link_file,
                                    created_at = l.created_at,
                                    requested_at = l.requested_at
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                .ToList();
                        }
                    }

                    DataTable dt = ToDataTable(data);

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
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.Message.ToString() });
            }
        }

        [Authorize]
        [HttpGet("DownloadRequested")]
        public async Task<IActionResult> DownloadRequested(string name, [FromQuery] PaginationFilter filter)
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
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy 23:59:59").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);

                        if (name == null || name == "" || name == "null")
                        {
                            data = (
                                from deal_requested in context.DealRequesteds.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    //deal_supplier_id = deal_requested.deal_supplier_id,
                                    //deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation_ref = deal_requested.deal_quatation_ref,
                                    deal_quatation_file_ref = deal_requested.deal_quatation_file_ref,
                                    deal_quatation_link_ref = deal_requested.deal_quatation_link_ref,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_quatation_link = deal_requested.deal_quatation_link,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    //deal_id = context.DealCodes.Where(x => x.deal_reference == deal_requested.deal_reference).Select(x => x.deal_id).FirstOrDefault(),
                                    deal_id = deal_requested.deal_id,
                                    deal_name = context.Deals.Where(x => x.id == context.DealCodes.Where(x => x.deal_id == deal_requested.deal_id).Select(x => x.deal_id).FirstOrDefault()).Select(x => x.deal_name).FirstOrDefault(),
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
                                    updated_at = deal_requested.updated_at,
                                    updated_by = deal_requested.updated_by,
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .ToList();

                        }
                        else
                        {
                            data = (
                                from deal_requested in context.DealRequesteds.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    //deal_supplier_id = deal_requested.deal_supplier_id,
                                    //deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation_ref = deal_requested.deal_quatation_ref,
                                    deal_quatation_file_ref = deal_requested.deal_quatation_file_ref,
                                    deal_quatation_link_ref = deal_requested.deal_quatation_link_ref,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_quatation_link = deal_requested.deal_quatation_link,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    //deal_id = context.DealCodes.Where(x => x.deal_reference == deal_requested.deal_reference).Select(x => x.deal_id).FirstOrDefault(),
                                    deal_id = deal_requested.deal_id,
                                    deal_name = context.Deals.Where(x => x.id == context.DealCodes.Where(x => x.deal_id == deal_requested.deal_id).Select(x => x.deal_id).FirstOrDefault()).Select(x => x.deal_name).FirstOrDefault(),
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
                                    updated_at = deal_requested.updated_at,
                                    updated_by = deal_requested.updated_by,
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_customer.Contains(name))
                                .OrderBy(x => x.id)
                                .ToList();
                        }
                    }
                    else
                    {
                        if (name == null || name == "" || name == "null")
                        {
                            data = (
                                from deal_requested in context.DealRequesteds
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    //deal_supplier_id = deal_requested.deal_supplier_id,
                                    //deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation_ref = deal_requested.deal_quatation_ref,
                                    deal_quatation_file_ref = deal_requested.deal_quatation_file_ref,
                                    deal_quatation_link_ref = deal_requested.deal_quatation_link_ref,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_quatation_link = deal_requested.deal_quatation_link,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    //deal_id = context.DealCodes.Where(x => x.deal_reference == deal_requested.deal_reference).Select(x => x.deal_id).FirstOrDefault(),
                                    deal_id = deal_requested.deal_id,
                                    deal_name = context.Deals.Where(x => x.id == context.DealCodes.Where(x => x.deal_id == deal_requested.deal_id).Select(x => x.deal_id).FirstOrDefault()).Select(x => x.deal_name).FirstOrDefault(),
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
                                    updated_at = deal_requested.updated_at,
                                    updated_by = deal_requested.updated_by,
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .ToList();
                        }
                        else
                        {
                            data = (
                                from deal_requested in context.DealRequesteds
                                join up in context.UserProfiles on deal_requested.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join customer in context.DealCustomers on deal_requested.deal_customer_id equals customer.id into _customer
                                from deal_customer in _customer.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal_requested.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()


                                select new
                                {
                                    id = deal_requested.id,
                                    deal_customer_id = deal_requested.deal_customer_id,
                                    deal_customer = deal_customer.description,
                                    //deal_supplier_id = deal_requested.deal_supplier_id,
                                    //deal_supplier = deal_supplier.description,
                                    deal_quantity = deal_requested.deal_quantity,
                                    deal_quatation_ref = deal_requested.deal_quatation_ref,
                                    deal_quatation_file_ref = deal_requested.deal_quatation_file_ref,
                                    deal_quatation_link_ref = deal_requested.deal_quatation_link_ref,
                                    deal_quatation = deal_requested.deal_quatation,
                                    deal_quatation_file = deal_requested.deal_quatation_file,
                                    deal_quatation_link = deal_requested.deal_quatation_link,
                                    deal_receipt = deal_requested.deal_receipt,
                                    deal_email = deal_requested.deal_email,
                                    deal_reference = deal_requested.deal_reference,
                                    //deal_id = context.DealCodes.Where(x => x.deal_reference == deal_requested.deal_reference).Select(x => x.deal_id).FirstOrDefault(),
                                    deal_id = deal_requested.deal_id,
                                    deal_name = context.Deals.Where(x => x.id == context.DealCodes.Where(x => x.deal_id == deal_requested.deal_id).Select(x => x.deal_id).FirstOrDefault()).Select(x => x.deal_name).FirstOrDefault(),
                                    remark = deal_requested.remark,
                                    created_at = deal_requested.created_at,
                                    created_by = deal_requested.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    status = (deal_requested.status == 0 ? "Waiting" : (deal_requested.status == 1 ? "Approved" : (deal_requested.status == 2 ? "Rejected" : ""))),
                                    deal_start_date = deal_requested.deal_start_date,
                                    deal_end_date = deal_requested.deal_end_date,
                                    updated_at = deal_requested.updated_at,
                                    updated_by = deal_requested.updated_by,
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_customer.Contains(name))
                                .OrderBy(x => x.id)
                                .ToList();
                        }
                    }

                    DataTable dt = ToDataTable(data);

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
                    string excelName = $"Requested-{Guid.NewGuid().ToString() + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
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
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.Message.ToString() });
            }
        }
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            System.Reflection.PropertyInfo[] Props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (System.Reflection.PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        [Authorize]
        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFile(string filename)
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = new MemoryStream())
                {
                    string contentType = GetMimeTypeForFileExtension(filename);
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "uploads", filename);
                    //string path = Path.Combine("/app/uploads/", filename);
                    System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    //System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    fs.CopyTo(stream);
                    stream.Position = 0;
                    string excelName = $"file-{Guid.NewGuid().ToString() + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
                    //string excelName = $"Report-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";

                    var content = stream.ToArray();
                    //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
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
        private string GetMimeTypeForFileExtension(string filePath)
        {
            const string DefaultContentType = "application/octet-stream";

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = DefaultContentType;
            }

            return contentType;
        }


        [Authorize]
        [HttpGet("GetUserByToken")]
        public IActionResult GetUserByToken(string token)
        {
            var data = (dynamic)null;

            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    if (token != null || token != "" || token != "null")
                    {
                        //var user = context.UserProfiles.SingleOrDefault(x => x.token == token);
                        data = context.DealUsers.SingleOrDefault(x => x.token == token && x.status == 1);
                        if (data == null)
                        {
                            return UnprocessableEntity(new { message = "Error Token not found." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { response = data });
        }


        [Authorize]
        [HttpGet("GetOrderCode")]
        public IActionResult GetOrderCode(string name, [FromQuery] PaginationFilter filter)
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
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy 23:59:59").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);

                        if (name == null || name == "" || name == "null")
                        {
                            data = (
                                from l in context.DealOrderCodes.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();




                            count = (
                                from l in context.DealOrderCodes.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
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
                                from l in context.DealOrderCodes.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();


                            count = (
                                from l in context.DealOrderCodes.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
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
                                from l in context.DealOrderCodes
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();




                            count = (
                                from l in context.DealOrderCodes
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
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
                                from l in context.DealOrderCodes
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();


                            count = (
                                from l in context.DealOrderCodes
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
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
        [HttpGet("DownloadOrderCode")]
        public async Task<IActionResult> DownloadOrderCode(string name, [FromQuery] PaginationFilter filter)
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
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy 23:59:59").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);

                        if (name == null || name == "" || name == "null")
                        {
                            data = (
                                from l in context.DealOrderCodes.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .ToList();
                        }
                        else
                        {
                            data = (
                                from l in context.DealOrderCodes.Where(x => x.created_at >= date_from && x.created_at <= date_to)
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
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
                                from l in context.DealOrderCodes
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.id)
                                .ToList();
                        }
                        else
                        {
                            data = (
                                from l in context.DealOrderCodes
                                join a in context.Deals on l.deal_id equals a.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on l.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()

                                select new
                                {
                                    id = l.id,
                                    deal_id = l.deal_id,
                                    deal_name = deal.deal_name,
                                    username = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    remark = l.remark,
                                    status = l.status,
                                    start_date = l.start_date,
                                    end_date = l.end_date,
                                    created_by = l.created_by,
                                    created_at = l.created_at,
                                    updated_by = l.updated_by,
                                    updated_at = l.updated_at,
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.id)
                                .ToList();
                        }
                    }


                    DataTable dt = ToDataTable(data);

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
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.Message.ToString() });
            }
        }

        [Authorize]
        [HttpGet("DownloadExampleFileUploadCode")]
        public async Task<IActionResult> DownloadExampleFileUploadCode()
        {
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {

                    var stream = new MemoryStream();
                    stream.Position = 0;

                    string path = Path.Combine("/app/download/ExampleFileUploadCode.xlsx"); //server
                    //string path = $"{Directory.GetCurrentDirectory()}{@"\download\ExampleFileUploadCode.xlsx"}"; //local

                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        fs.CopyTo(stream);

                        string excelName = $"Example-File-Upload-Code.xlsx";
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





        [Authorize]
        [HttpPost("ReadFileExcelForAdd")]
        public async Task<IActionResult> ReadFileExcelForAdd([FromForm] FormDealManangement form)
        //public IActionResult ReadFileExcelForAdd(IFormFile file)
        {
            string status = "";
            string filename = "";

            try
            {
                int user = Convert.ToInt32(form.emp_id);
                int deal_id = form.deal_id;
                var file = form.file;
                string remark = form.remark;


                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var _deal = (dynamic)null;
                    _deal = context.Deals.SingleOrDefault(x => x.id == form.deal_id);

                    if (_deal == null)
                    {
                        return UnprocessableEntity(new { message = "Deal was not found." });
                    }

                    if (form.coupon_type_id == 2)
                    {
                        int qty = 0;
                        for (int i = 1; i <= form.amount_code_physical_reward; i++)
                        {
                            DealCode _data = new DealCode();
                            _data.deal_id = deal_id;
                            _data.deal_coupon_code = i.ToString();
                            _data.created_at = DateTime.Now;
                            _data.created_by = user;

                            context.DealCodes.Add(_data);

                            qty++;
                        }


                        DealLog _data_log = new DealLog();
                        _data_log.user = user;
                        _data_log.type = 0;
                        _data_log.deal_id = deal_id;
                        _data_log.qty_code = qty;
                        _data_log.remark = remark;
                        _data_log.created_at = DateTime.Now;

                        context.DealLogs.Add(_data_log);

                        context.SaveChanges();
                        status = "success";
                    }
                    else
                    {

                        if (!CheckIfExcelFile(file))
                        {
                            return UnprocessableEntity(new { message = "Invalid file extension" });
                        }

                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        using (var stream = new MemoryStream())
                        {
                            file.CopyTo(stream);

                            stream.Position = 0;
                            using (var reader = ExcelReaderFactory.CreateReader(stream))
                            {
                                string text = "";
                                //while (reader.Read()) //Each row of the file
                                //{
                                //    //string val = reader.GetValue(0).ToString();
                                //}
                                var result = reader.AsDataSet();
                                DataTable dt = result.Tables[0];

                                if (dt.Rows.Count > 0)
                                {
                                    string column_deal_coupon_code = dt.Rows[0][0].ToString();
                                    if (column_deal_coupon_code != "code")
                                    {
                                        text = "ตรวจสอบ รูปแบบคอลัมม์ ของไฟล์ Excel";
                                        return UnprocessableEntity(new { message = text });
                                    }
                                    //using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                                    //{
                                    //foreach (DataRow dr in dt.Rows)
                                    //{
                                    //    string _code = dr["deal_coupon_code"].ToString();
                                    //    if (_code != "")
                                    //    {
                                    //        DealCode _data = new DealCode();
                                    //        _data.deal_coupon_code = _code;

                                    //        context.DealCodes.Add(_data);
                                    //    }
                                    //}



                                    //qt
                                    bool flagQT = false;
                                    //var _qt = (dynamic)null;
                                    //_qt = context.DealQuatations.ToList<dynamic>().Count();
                                    //int qt_id = 1;
                                    //if (_qt > 0)
                                    //{
                                    //    qt_id = context.DealQuatations.Max(x => x.id) + 1;
                                    //}

                                    var csb = new MySqlConnectionStringBuilder(ConnectionString.Value.DefaultConnection);
                                    string database = csb.Database;
                                    int qt_id = GetIDAutoIncrementFromDbTable(database, "deal_upload_file");


                                    if (form.file_qt != null || form.deal_quatation_link != null)
                                    {
                                        //DealQuatation _data_qt = new DealQuatation();
                                        //_data_qt.created_at = DateTime.Now;
                                        //_data_qt.created_by = user;
                                        //string _filename = "";
                                        //if (form.file_qt != null)
                                        //{
                                        //    _filename = await UploadFile(form.file_qt);
                                        //    _data_qt.deal_quatation_file = _filename;
                                        //}
                                        //if (form.deal_quatation_link != null)
                                        //{
                                        //    _data_qt.deal_quatation_link = form.deal_quatation_link;
                                        //}

                                        //context.DealQuatations.Add(_data_qt);


                                        //add file
                                        DealUploadFile _data_up_file = new DealUploadFile();
                                        _data_up_file.deal_type_of_file = 0;
                                        _data_up_file.created_at = DateTime.Now;
                                        _data_up_file.created_by = user;
                                        string _filename_up_file = "";
                                        if (form.file_qt != null)
                                        {
                                            _filename_up_file = await UploadFile(form.file_qt);
                                            _data_up_file.deal_file_path = _filename_up_file;
                                        }
                                        if (form.deal_quatation_link != null)
                                        {
                                            _data_up_file.deal_file_link = form.deal_quatation_link;
                                        }

                                        context.DealUploadFiles.Add(_data_up_file);


                                        flagQT = true;
                                    }


                                    int qty = 0;
                                    for (int i = 1; i < dt.Rows.Count; i++)
                                    {
                                        //string _code = dt.Rows[0]["deal_coupon_code"].ToString();
                                        string _code = dt.Rows[i][0].ToString();
                                        if (_code != "")
                                        {

                                            var _deal_code = (dynamic)null;
                                            _deal_code = context.DealCodes.FirstOrDefault(x => x.deal_coupon_code == _code);
                                            if (_deal_code != null)
                                            {
                                                return UnprocessableEntity(new { message = "Deal coupon code duplicate. ( code is " + _code + ")" });
                                            }



                                            DealCode _data = new DealCode();
                                            _data.deal_id = deal_id;
                                            _data.deal_coupon_code = _code;
                                            _data.created_at = DateTime.Now;
                                            _data.created_by = user;

                                            if (flagQT)
                                            {
                                                _data.deal_quatation_id = qt_id;
                                            }
                                            else
                                            {
                                                _data.deal_quatation_id = _deal.deal_quatation_id;
                                            }

                                            context.DealCodes.Add(_data);

                                            qty++;
                                        }
                                    }

                                    filename = await UploadFile(form.file);

                                    DealLog _data_log = new DealLog();
                                    _data_log.user = user;
                                    _data_log.type = 0;
                                    _data_log.deal_id = deal_id;
                                    _data_log.qty_code = qty;
                                    _data_log.remark = remark;
                                    _data_log.link_file = filename;
                                    _data_log.created_at = DateTime.Now;

                                    context.DealLogs.Add(_data_log);

                                    context.SaveChanges();
                                    status = "success";
                                    //}

                                }


                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { status = status, response = true, filename = filename });

        }
        public class FormDealManangement
        {
            public int emp_id { get; set; }
            public int deal_id { get; set; }
            public string deal_supplier_id { get; set; }
            public string deal_customer_id { get; set; }
            public IFormFile file_qt { get; set; }
            public string deal_quatation { get; set; }
            public string deal_quatation_link { get; set; }
            public string remark { get; set; }
            public IFormFile file { get; set; }
            public int? coupon_type_id { get; set; }
            public int? amount_code_physical_reward { get; set; }
        }
        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [Authorize]
        [HttpPost("UploadFile")]
        private async Task<string> UploadFile(IFormFile file)
        {
            var filename = "";
            filename = await WriteFile(file);

            return filename;
            //return Ok(new { filename = filename });
        }

        private bool CheckIfExcelFile(IFormFile file)
        {
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            return (extension == ".xls" || extension == ".xlsx"); // Change the extension based on your need
        }

        private async Task<string> WriteFile(IFormFile file)
        {
            string url = "";
            string fileName;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = DateTime.Now.Ticks + extension; //Create a new Name for the file due to security reasons.

                var pathBuilt = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

                if (!Directory.Exists(pathBuilt))
                {
                    Directory.CreateDirectory(pathBuilt);
                }

                var path = Path.Combine(Directory.GetCurrentDirectory(), "uploads",
                   fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                url = string.Format("{0}/{1}", "uploads", fileName);
            }
            catch (Exception e)
            {
                return e.Message.ToString();
            }

            return url;
        }




        [Authorize]
        [HttpPost("AddDeal")]
        public async Task<IActionResult> Add([FromForm] FormDeal data)
        {
            //string status = "YES";
            string result = "";
            string filename = "";

            using var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection);
            using var transaction = context.Database.BeginTransaction();
            try
            {
                //using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                //{

                    transaction.CreateSavepoint("BeforeAddData");

                    var csb = new MySqlConnectionStringBuilder(ConnectionString.Value.DefaultConnection);
                    string database = csb.Database;


                    //int deal_id = GetIDAutoIncrementFromDbTable(database, "deal");

                    var _deal = (dynamic)null;
                    _deal = context.Deals.ToList<dynamic>().Count();
                    int deal_id = 1;
                    if (_deal > 0)
                    {
                        deal_id = context.Deals.Max(x => x.id) + 1;
                    }



                    //qt
                    //var _qt = (dynamic)null;
                    //_qt = context.DealQuatations.ToList<dynamic>().Count();
                    //int qt_id = 1;
                    //if (_qt > 0)
                    //{
                    //    qt_id = context.DealQuatations.Max(x => x.id) + 1;
                    //}
                    //DealQuatation _data_qt = new DealQuatation();
                    //_data_qt.created_at = DateTime.Now;
                    //_data_qt.created_by = data.created_by;


                    ////qt
                    //var get_up_file = (dynamic)null;
                    //get_up_file = context.DealUploadFiles.ToList<dynamic>().Count();
                    //int get_up_file_id_id = 1;
                    //if (get_up_file > 0)
                    //{
                    //    get_up_file_id_id = context.DealUploadFiles.Max(x => x.id) + 1;
                    //}

                    int file_up_id = GetIDAutoIncrementFromDbTable(database, "deal_upload_file");
                    int seq_file = 0;


                    Deal _data = new Deal();
                    _data.id = deal_id;
                    _data.deal_name = data.deal_name;
                    _data.deal_supplier_id = data.deal_supplier_id;
                    _data.deal_customer_id = data.deal_customer_id;
                    _data.remark = data.remark;
                    _data.status = 1;
                    _data.created_at = DateTime.Now;
                    _data.created_by = data.created_by;
                    _data.deal_coupon_type = data.coupon_type_id;
                    _data.deal_coupon_value = data.coupon_value;
                    //_data.deal_permission = data.;


                    //quatation id
                    if (data.coupon_type_id != 2)
                    {
                        if (data.file_qt != null || (data.link_sheetQT != "" && data.link_sheetQT != null))
                        {
                            //add file
                            DealUploadFile _data_up_file = new DealUploadFile();
                            _data_up_file.deal_type_of_file = 0;
                            _data_up_file.created_at = DateTime.Now;
                            _data_up_file.created_by = data.created_by;

                            if (data.file_qt != null)
                            {
                                string _filename_up_file = await UploadFile(data.file_qt);
                                _data_up_file.deal_file_path = _filename_up_file;

                                _data.deal_quatation_id = file_up_id;
                            }
                            if (data.link_sheetQT != "" && data.link_sheetQT != null)
                            {
                                _data_up_file.deal_file_link = data.link_sheetQT;

                                _data.deal_quatation_link = data.link_sheetQT;
                            }


                            seq_file++;
                            context.DealUploadFiles.Add(_data_up_file);

                        }
                    }


                    //if (data.file_qt != null || data.link_sheetQT != null)
                    //{
                    //    //context.DealQuatations.Add(_data_qt);
                    //}


                    //if (data.file_qt != null)
                    //{
                    //    string _filename = await UploadFile(data.file_qt);
                    //    _data.deal_quatation = _filename;

                    //    _data_qt.deal_quatation_file = _filename;
                    //}
                    //if (data.link_sheetQT != "" && data.link_sheetQT != null)
                    //{
                    //    _data.deal_quatation_link = data.link_sheetQT;

                    //    _data_qt.deal_quatation_link = data.link_sheetQT;
                    //}




                    //_data.deal_merchant_id = data.;
                    //_data.deal_merchant_name = data.;

                _data.deal_invoice = data.invoice;
                if (data.file_invoice != null)
                {
                    //add file
                    DealUploadFile _data_up_file = new DealUploadFile();
                    _data_up_file.deal_type_of_file = 6;
                    _data_up_file.created_at = DateTime.Now;
                    _data_up_file.created_by = data.created_by;

                    string _filename = await UploadFile(data.file_invoice);
                    _data_up_file.deal_file_path = _filename;

                    //_data.deal_receipt = _filename;
                    _data.deal_invoice_file_id = file_up_id + seq_file;


                    seq_file++;
                    context.DealUploadFiles.Add(_data_up_file);
                }



                if (data.file_receipt != null)
                    {
                        //add file
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 5;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.file_receipt);
                        _data_up_file.deal_file_path = _filename;

                        //_data.deal_receipt = _filename;
                        _data.deal_receipt_id = file_up_id + seq_file;


                        seq_file++;
                        context.DealUploadFiles.Add(_data_up_file);
                    }


                    System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");

                    if (data.start_date != null) {
                        DateTime start_date = DateTime.ParseExact(data.start_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        _data.deal_start_date = start_date;

                    }
                    if (data.end_date != null)
                    {
                        DateTime end_date = DateTime.ParseExact(data.end_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        _data.deal_end_date = end_date;
                    }


                    //_data.deal_coupon_name = data.;
                    //_data.deal_cost = data.;
                    //_data.deal_condition = data.;

                    if (data.file_image != null)
                    {
                        //add file
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 7;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.file_image);
                        _data_up_file.deal_file_path = _filename;

                        //_data.deal_coupon_image = _filename;
                        _data.deal_coupon_image_id = file_up_id + seq_file;


                        seq_file++;
                        context.DealUploadFiles.Add(_data_up_file);
                    }

                    //_data.deal_major = data.;
                    _data.deal_discount_value = data.deal_discount_value;
                    _data.deal_summary_value = data.deal_summary_value;
                    _data.deal_add_item = data.deal_add_item;
                    _data.deal_add_amount = data.deal_add_amount;
                    _data.deal_add_value = data.deal_add_value;

                    _data.deal_pr_number = data.deal_pr_number;
                    if (data.deal_pr_file != null)
                    {
                        //add file
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 6;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.deal_pr_file);
                        _data_up_file.deal_file_path = _filename;

                        //_data.deal_pr_file = _filename;
                        _data.deal_pr_file_id = file_up_id + seq_file;


                        seq_file++;
                        context.DealUploadFiles.Add(_data_up_file);
                    }

                    _data.minimum_requested = data.minimum_requested;
                    _data.maximum_requested = data.maximum_requested;
                    _data.minimum_order = data.minimum_order;
                    _data.maximum_order = data.maximum_order;


                    context.Deals.Add(_data);


                    if (data.coupon_type_id == 2)
                    {

                        int _amount_code_physical_reward = (data.amount_code_physical_reward != null ? data.amount_code_physical_reward.Value : 1);

                        int qty = 0;
                        for (int i = 1; i <= _amount_code_physical_reward; i++)
                        {

                            DealCode _data_dealcode = new DealCode();
                            _data_dealcode.deal_id = deal_id;
                            _data_dealcode.deal_coupon_code = i.ToString();
                            _data_dealcode.created_at = DateTime.Now;
                            _data_dealcode.created_by = data.created_by;

                            //if (data.file_qt != null)
                            //{
                            //    _data_dealcode.deal_quatation_id = qt_id;
                            //}

                            context.DealCodes.Add(_data_dealcode);

                            qty++;
                        }

                        DealLog _data_log = new DealLog();
                        _data_log.user = data.created_by.Value;
                        _data_log.type = 0;
                        _data_log.deal_id = deal_id;
                        _data_log.action = 0;
                        _data_log.qty_code = qty;
                        //_data_log.remark = remark;
                        //_data_log.link_file = filename;
                        _data_log.remark = "Physical Reward";
                        _data_log.created_at = DateTime.Now;

                        context.DealLogs.Add(_data_log);

                    }
                    else
                    {

                        var file = data.file;
                        if (file != null)
                        {
                            if (!CheckIfExcelFile(file))
                            {
                                return UnprocessableEntity(new { message = "Invalid file extension" });
                            }

                            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                            using (var stream = new MemoryStream())
                            {
                                file.CopyTo(stream);

                                stream.Position = 0;
                                using (var reader = ExcelReaderFactory.CreateReader(stream))
                                {
                                    string text = "";
                                    //while (reader.Read()) //Each row of the file
                                    //{
                                    //    //string val = reader.GetValue(0).ToString();
                                    //}
                                    var _result = reader.AsDataSet();
                                    DataTable dt = _result.Tables[0];

                                    if (dt.Rows.Count > 0)
                                    {
                                        string column_deal_coupon_code = dt.Rows[0][0].ToString();
                                        if (column_deal_coupon_code != "code")
                                        {
                                            text = "ตรวจสอบ รูปแบบคอลัมม์ ของไฟล์ Excel";
                                            return UnprocessableEntity(new { message = text });
                                        }


                                        int qty = 0;
                                        for (int i = 1; i < dt.Rows.Count; i++)
                                        {
                                            //string _code = dt.Rows[0]["deal_coupon_code"].ToString();
                                            string _code = dt.Rows[i][0].ToString();
                                            if (_code != "")
                                            {

                                            var _deal_code = (dynamic)null;
                                            _deal_code = context.DealCodes.FirstOrDefault(x => x.deal_coupon_code == _code);
                                            if (_deal_code != null)
                                            {
                                                return UnprocessableEntity(new { message = "Deal coupon code duplicate. ( code is " + _code + ")" });
                                            }

                                            DealCode _data_dealcode = new DealCode();
                                                _data_dealcode.deal_id = deal_id;
                                                _data_dealcode.deal_coupon_code = _code;
                                                _data_dealcode.created_at = DateTime.Now;
                                                _data_dealcode.created_by = data.created_by;

                                                _data_dealcode.deal_quatation_id = file_up_id;
                                                //_data_dealcode.deal_quatation_id = qt_id;

                                                context.DealCodes.Add(_data_dealcode);

                                                qty++;
                                            }
                                        }

                                        filename = await UploadFile(file);

                                        DealLog _data_log = new DealLog();
                                        _data_log.user = data.created_by.Value;
                                        _data_log.type = 0;
                                        _data_log.deal_id = deal_id;
                                        _data_log.action = 0;
                                        _data_log.qty_code = qty;
                                        //_data_log.remark = remark;
                                        _data_log.link_file = filename;
                                        _data_log.created_at = DateTime.Now;

                                        context.DealLogs.Add(_data_log);

                                    }


                                }
                            }
                        }
                        else
                        {
                            DealLog _data_log = new DealLog();
                            _data_log.user = data.created_by.Value;
                            _data_log.deal_id = deal_id;
                            _data_log.action = 0;
                            _data_log.qty_code = 0;
                            //_data_log.remark = remark;
                            _data_log.created_at = DateTime.Now;

                            context.DealLogs.Add(_data_log);
                        }
                    }


                    context.SaveChanges();

                transaction.Commit();

                result = "success";
                //}
            }
            catch (Exception ex)
            {
                transaction.RollbackToSavepoint("BeforeAddData");

                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { response = result });
        }
        public class FormDeal
        {
            public int? deal_id { get; set; }
            public int? deal_supplier_id { get; set; }
            public int? deal_customer_id { get; set; }
            public string deal_name { get; set; }
            public string remark { get; set; }
            public int? status { get; set; }
            public int? created_by { get; set; }
            public DateTime? created_at { get; set; }
            public int? updated_by { get; set; }
            public DateTime? updated_at { get; set; }
            public IFormFile file { get; set; }
            public int? coupon_type_id { get; set; }
            public decimal? coupon_value { get; set; }
            public DateTime? start_date { get; set; }
            public DateTime? end_date { get; set; }
            public IFormFile file_qt { get; set; }
            public string link_sheetQT { get; set; }
            public IFormFile file_image { get; set; }
            public string invoice { get; set; }
            public IFormFile file_invoice { get; set; }
            public IFormFile file_receipt { get; set; }
            public decimal? deal_discount_value { get; set; }
            public int? deal_summary_value { get; set; }
            public string deal_add_item { get; set; }
            public int? deal_add_amount { get; set; }
            public decimal? deal_add_value { get; set; }
            public string deal_pr_number { get; set; }
            public IFormFile deal_pr_file { get; set; }
            public int? minimum_requested { get; set; }
            public int? maximum_requested { get; set; }
            public int? minimum_order { get; set; }
            public int? maximum_order { get; set; }
            public int? amount_code_physical_reward { get; set; }
        }

        [Authorize]
        [HttpPost("UpdateDeal")]
        public async Task<IActionResult> Update([FromForm] FormDeal data)
        {
            string result = "";

            using var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection);
            using var transaction = context.Database.BeginTransaction();
            try
            {
                //using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                //{
                transaction.CreateSavepoint("BeforeAddData");

                var deal_data = context.Deals.FirstOrDefault((c) => c.id == data.deal_id);
                    if (deal_data == null)
                    {
                        return UnprocessableEntity(new { message = "Deal not found" });
                    }


                    var csb = new MySqlConnectionStringBuilder(ConnectionString.Value.DefaultConnection);
                    string database = csb.Database;



                    //qt
                    int file_up_id = GetIDAutoIncrementFromDbTable(database, "deal_upload_file");
                    int seq_file = 0;

                    //var _qt = (dynamic)null;
                    //_qt = context.DealQuatations.ToList<dynamic>().Count();
                    //int qt_id = 1;
                    //if (_qt > 0)
                    //{
                    //    qt_id = context.DealQuatations.Max(x => x.id) + 1;
                    //}
                    //DealQuatation _data_qt = new DealQuatation();
                    //_data_qt.created_at = DateTime.Now;
                    //_data_qt.created_by = data.created_by;


                    ////qt
                    //var get_up_file = (dynamic)null;
                    //get_up_file = context.DealUploadFiles.ToList<dynamic>().Count();
                    //int get_up_file_id_id = 1;
                    //if (get_up_file > 0)
                    //{
                    //    get_up_file_id_id = context.DealUploadFiles.Max(x => x.id) + 1;
                    //}
                    ////add file
                    //DealUploadFile _data_up_file = new DealUploadFile();
                    //_data_up_file.deal_type_of_file = 0;
                    //_data_up_file.created_at = DateTime.Now;
                    //_data_up_file.created_by = data.created_by;




                    deal_data.deal_name = data.deal_name;
                    deal_data.deal_supplier_id = data.deal_supplier_id;
                    deal_data.deal_customer_id = data.deal_customer_id;
                    deal_data.remark = data.remark;
                    deal_data.status = data.status;
                    deal_data.updated_at = DateTime.Now;
                    deal_data.updated_by = data.updated_by;

                    deal_data.deal_coupon_type = data.coupon_type_id;
                    deal_data.deal_coupon_value = data.coupon_value;
                    //deal_data.deal_permission = data.;
                    deal_data.deal_discount_value = data.deal_discount_value;
                    deal_data.deal_summary_value = data.deal_summary_value;
                    deal_data.deal_add_item = data.deal_add_item;
                    deal_data.deal_add_amount = data.deal_add_amount;
                    deal_data.deal_add_value = data.deal_add_value;


                    //quatation id
                    //deal_data.deal_quatation_id = qt_id;
                    if (data.file_qt != null || (data.link_sheetQT != "" && data.link_sheetQT != null))
                    {
                        //add file
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 0;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        if (data.file_qt != null)
                        {
                            string _filename_up_file = await UploadFile(data.file_qt);
                            _data_up_file.deal_file_path = _filename_up_file;

                            deal_data.deal_quatation_id = file_up_id;
                        }
                        if (data.link_sheetQT != "" && data.link_sheetQT != null)
                        {
                            _data_up_file.deal_file_link = data.link_sheetQT;

                            deal_data.deal_quatation_link = data.link_sheetQT;
                        }


                        seq_file++;
                        context.DealUploadFiles.Add(_data_up_file);

                    }

                    //if (data.file_qt != null)
                    //{
                    //    string _filename = await UploadFile(data.file_qt);
                    //    deal_data.deal_quatation = _filename;

                    //    _data_qt.deal_quatation_file = _filename;
                    //}
                    ////else
                    ////{
                    ////    deal_data.deal_quatation = null;
                    ////}

                    //if (data.link_sheetQT != "" && data.link_sheetQT != null)
                    //{
                    //    deal_data.deal_quatation_link = data.link_sheetQT;

                    //    _data_qt.deal_quatation_link = data.link_sheetQT;
                    //}


                    //if (data.file_qt != null)
                    //{
                    //    string _filename_up_file = await UploadFile(data.file_qt);
                    //    _data_up_file.deal_file_path = _filename_up_file;

                    //    _data_qt.deal_quatation_file = _filename_up_file;
                    //}
                    //if (data.link_sheetQT != "" && data.link_sheetQT != null)
                    //{
                    //    _data_up_file.deal_file_link = data.link_sheetQT;

                    //    deal_data.deal_quatation_link = data.link_sheetQT;
                    //}

                    //deal_data.deal_merchant_id = data.;
                    //deal_data.deal_merchant_name = data.;

                    deal_data.deal_invoice = data.invoice;
                if (data.file_invoice != null)
                {
                    //add file
                    DealUploadFile _data_up_file = new DealUploadFile();
                    _data_up_file.deal_type_of_file = 6;
                    _data_up_file.created_at = DateTime.Now;
                    _data_up_file.created_by = data.created_by;

                    string _filename = await UploadFile(data.file_invoice);
                    _data_up_file.deal_file_path = _filename;

                    //_data.deal_receipt = _filename;
                    deal_data.deal_invoice_file_id = file_up_id + seq_file;


                    seq_file++;
                    context.DealUploadFiles.Add(_data_up_file);
                }


                if (data.file_receipt != null)
                    {
                        //add file
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 5;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.file_receipt);
                        _data_up_file.deal_file_path = _filename;

                        //deal_data.deal_receipt = _filename;
                        deal_data.deal_receipt_id = file_up_id + seq_file;


                        seq_file++;
                        context.DealUploadFiles.Add(_data_up_file);
                    }
                    //else
                    //{
                    //    deal_data.deal_receipt = null;
                    //}


                    System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                    if (data.start_date != null)
                    {
                        DateTime start_date = DateTime.ParseExact(data.start_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        deal_data.deal_start_date = start_date;
                    }
                    if (data.end_date != null)
                    {
                        DateTime end_date = DateTime.ParseExact(data.end_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        deal_data.deal_end_date = end_date;
                    }


                    //deal_data.deal_coupon_name = data.;
                    //deal_data.deal_cost = data.;
                    //deal_data.deal_condition = data.;

                    if (data.file_image != null)
                    {
                        //add file
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 7;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.file_image);
                        _data_up_file.deal_file_path = _filename;

                        //deal_data.deal_coupon_image = _filename;
                        deal_data.deal_coupon_image_id = file_up_id + seq_file;


                        seq_file++;
                        context.DealUploadFiles.Add(_data_up_file);                        
                    }
                    //else
                    //{
                    //    deal_data.deal_coupon_image = null;
                    //}

                    //deal_data.deal_major = data.;
                    //deal_data.deal_discount = data.;

                    deal_data.deal_pr_number = data.deal_pr_number;
                    if (data.deal_pr_file != null)
                    {
                        //add file
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 6;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.deal_pr_file);
                        _data_up_file.deal_file_path = _filename;

                        //deal_data.deal_pr_file = _filename;
                        deal_data.deal_pr_file_id = file_up_id + seq_file;


                        seq_file++;
                        context.DealUploadFiles.Add(_data_up_file);                        
                    }

                    deal_data.minimum_requested = data.minimum_requested;
                    deal_data.maximum_requested = data.maximum_requested;
                    deal_data.minimum_order = data.minimum_order;
                    deal_data.maximum_order = data.maximum_order;


                    //if (data.file_qt != null || data.link_sheetQT != null)
                    //{
                    //    context.DealQuatations.Add(_data_qt);

                    //    context.DealUploadFiles.Add(_data_up_file);
                    //}


                    DealLog _data_log = new DealLog();
                    _data_log.user = data.updated_by.Value;
                    _data_log.action = 1;
                    _data_log.deal_id = data.deal_id.Value;
                    _data_log.created_at = DateTime.Now;

                    context.DealLogs.Add(_data_log);


                    context.SaveChanges();

                transaction.Commit();

                result = "success";
                //}
            }
            catch (Exception ex)
            {
                transaction.RollbackToSavepoint("BeforeAddData");

                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { response = result });
        }

        [Authorize]
        [HttpPost("DeleteDeal")]
        //[Route("Delete/{id}")]
        public IActionResult Delete([FromForm] FormDeal data)
        {
            string _status = "YES";
            string result = "";
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var _data = context.Deals.SingleOrDefault(x => x.id == data.deal_id);
                    if (_data == null)
                    {
                        return UnprocessableEntity(new { message = "Transaction not found" });
                    }

                    context.Deals.Remove(_data);

                    context.SaveChanges();
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { status = _status, response = result });
        }

        [Authorize]
        [HttpPost("RequestCode")]
        public async Task<IActionResult> RequestCode([FromForm] FormRequest formdata)
        {
            string result = "";
            var data = (dynamic)null;
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var deal_data = context.Deals.FirstOrDefault((c) => c.id == formdata.deal_id);
                    if (deal_data == null)
                    {
                        return UnprocessableEntity(new { message = "Deal not found" });
                    }
                    if (deal_data.minimum_requested > formdata.quantity && deal_data.minimum_requested != null)
                    {
                        return UnprocessableEntity(new { message = "Quantity of request less than deal quantity minimum. Deal quantity minimum is " + deal_data.minimum_requested.ToString() });
                    }
                    if (deal_data.maximum_requested < formdata.quantity && deal_data.maximum_requested != null)
                    {
                        return UnprocessableEntity(new { message = "Quantity of request more than deal quantity maximum. Deal quantity maximum is " + deal_data.maximum_requested.ToString() });
                    }

                    int? coupon_type = deal_data.deal_coupon_type;

                    var dealCode = context.DealCodes.FirstOrDefault((c) => c.deal_id == formdata.deal_id);
                    if (dealCode == null)
                    {
                        return UnprocessableEntity(new { message = "Transaction deal code not found" });
                    }

                    var count = 0;
                    count = context.DealCodes.Where(x => x.deal_id == formdata.deal_id && x.requested_at == null && x.requested_by == null)
                            .ToList().Count();
                    if (count < formdata.quantity)
                    {
                        return UnprocessableEntity(new { message = "Deal code not enough. (Quantity is " + count + ")" });
                    }



                    string reference_code = RandomString(10);
                    var dataDealCode = context.DealCodes.Where(x => x.deal_id == formdata.deal_id && x.requested_at == null && x.requested_by == null)
                                        .AsEnumerable()
                                        .OrderBy(x => x.id)
                                        .Take(formdata.quantity)
                                        .ToList();
                    //dataDealCode.ForEach(x => x.requested_at = DateTime.Now);
                    //dataDealCode.ForEach(x => x.requested_by = formdata.requested_by);

                    System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                    DateTime start_date = new DateTime();
                    DateTime end_date = new DateTime();

                    if (formdata.deal_start_date != null)
                    {
                        start_date = DateTime.ParseExact(formdata.deal_start_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                    }
                    if (formdata.deal_end_date != null)
                    {
                        end_date = DateTime.ParseExact(formdata.deal_end_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                    }

                    foreach (var x in dataDealCode)
                    {
                        if (formdata.deal_start_date != null) x.deal_start_date = start_date;
                        if (formdata.deal_end_date != null) x.deal_end_date = end_date;
                        x.requested_at = DateTime.Now;
                        x.requested_by = formdata.requested_by;
                        x.deal_reference = reference_code;
                        x.remark = formdata.remark;
                    }



                    data = (
                        from deal_code in context.DealCodes.Where(x => x.deal_id == formdata.deal_id && x.requested_at == null && x.requested_by == null)
                        join dealC in context.Deals on deal_code.deal_id equals dealC.id into _deal
                        from deal in _deal.DefaultIfEmpty()
                        join up in context.UserProfiles on deal_code.created_by equals up.id into g
                        from x in g.DefaultIfEmpty()
                        join up in context.UserProfiles on deal_code.requested_by equals up.id into h
                        from uby in h.DefaultIfEmpty()
                        join customer in context.DealCustomers on deal_code.deal_customer_id equals customer.id into _customer
                        from deal_customer in _customer.DefaultIfEmpty()
                        join supplier in context.DealSuppliers on deal_code.deal_supplier_id equals supplier.id into _supplier
                        from deal_supplier in _supplier.DefaultIfEmpty()

                            //select new
                            //{
                            //    id = deal_code.id,
                            //    deal_customer_id = deal.deal_customer_id,
                            //    deal_customer = deal_customer.description,
                            //    deal_supplier_id = deal.deal_supplier_id,
                            //    deal_supplier = deal_supplier.description,
                            //    deal_id = (int?)deal.id,
                            //    deal_name = deal.deal_name,
                            //    deal_coupon_code = deal_code.deal_coupon_code,
                            //    deal_reference = deal_code.deal_reference,
                            //    remark = deal_code.remark,
                            //    created_at = deal_code.created_at,
                            //    created_by = deal_code.created_by,
                            //    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                            //    deal_start_date = formdata.deal_start_date,
                            //    deal_end_date = formdata.deal_end_date,
                            //    //requested_at = deal_code.requested_at,
                            //    //requested_by = deal_code.requested_by,
                            //    //requested_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                            //})

                        select new
                        {
                            //id = deal_code.id,
                            //deal_customer_id = deal.deal_customer_id,
                            Name = deal.deal_name,
                            Value = deal.deal_coupon_value,
                            Ecoupon = deal_code.deal_coupon_code,
                            Customer = formdata.deal_customer_name,
                            StartDate = formdata.deal_start_date,
                            EndDate = formdata.deal_end_date,
                            QT = formdata.deal_quatation,
                            //deal_supplier_id = deal.deal_supplier_id,
                            //deal_supplier = deal_supplier.description,
                            //deal_id = (int?)deal.id,
                            //deal_reference = deal_code.deal_reference,
                            //remark = deal_code.remark,
                            //created_at = deal_code.created_at,
                            //created_by = deal_code.created_by,
                            //created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                            //requested_at = deal_code.requested_at,
                            //requested_by = deal_code.requested_by,
                            //requested_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString())
                        })

                        .AsEnumerable()
                        .OrderBy(x => x.Name)
                        //.OrderBy(x => x.id)
                        .Take(formdata.quantity)
                        .ToList();


                    DataTable dt = ToDataTable(data);

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
                    string excelName = $"Requested-{Guid.NewGuid().ToString() + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
                    //string excelName = $"Report-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";

                    var content = stream.ToArray();
                    string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    ExcelFileViewModel excelFile = new ExcelFileViewModel()
                    {
                        fileName = excelName,
                        content = content,
                        contentType = contentType
                    };


                    var filename = "";
                    //byte[] _filename = (byte[])content;
                    filename = await WriteFileStream(content);


                    string filename_qt_ref = "";
                    string filename_qt = "";
                    string filename_receipt = "";
                    string filename_po = "";
                    if (formdata.file_qt_ref != null) filename_qt_ref = await UploadFile(formdata.file_qt_ref);
                    if (formdata.file_qt != null) filename_qt = await UploadFile(formdata.file_qt);
                    if (formdata.file_receipt != null) filename_receipt = await UploadFile(formdata.file_receipt);
                    if (formdata.file_po != null) filename_po = await UploadFile(formdata.file_po);

                    DealRequested _data_requested = new DealRequested();
                    //_data_requested.deal_supplier_id = formdata.deal_customer_id;
                    _data_requested.deal_id = formdata.deal_id;

                    _data_requested.deal_customer_id = formdata.deal_customer_id.Value;
                    _data_requested.deal_requested_file = filename;

                    _data_requested.deal_quatation_ref = formdata.deal_quatation_ref;
                    _data_requested.deal_quatation_file_ref = filename_qt_ref;
                    _data_requested.deal_quatation_link_ref = formdata.link_sheetQT_ref;

                    _data_requested.deal_quatation = formdata.deal_quatation;
                    _data_requested.deal_quatation_file = filename_qt;
                    _data_requested.deal_quatation_link = formdata.link_sheetQT;

                    _data_requested.deal_receipt = filename_receipt;
                    _data_requested.deal_reference = reference_code;
                    _data_requested.deal_quantity = formdata.quantity;
                    _data_requested.deal_email = formdata.email;
                    _data_requested.deal_customer_name = formdata.customer_name;
                    _data_requested.remark = formdata.remark;
                    _data_requested.deal_po_file = filename_po;
                    _data_requested.deal_po_link = formdata.link_sheetPO;


                    if (formdata.deal_start_date != null) _data_requested.deal_start_date = start_date;
                    if (formdata.deal_end_date != null) _data_requested.deal_end_date = end_date;


                    _data_requested.created_by = formdata.requested_by.Value;
                    _data_requested.created_at = DateTime.Now;

                    if (coupon_type == 2) {
                        _data_requested.status = 1;
                    }
                    else
                    {
                        _data_requested.status = 0;
                    }

                    context.DealRequesteds.Add(_data_requested);


                    DealLog _data_log = new DealLog();
                    _data_log.user = formdata.requested_by.Value;
                    _data_log.deal_id = formdata.deal_id.Value;
                    _data_log.requested_at = DateTime.Now;
                    _data_log.created_at = DateTime.Now;
                    _data_log.link_file = filename;
                    _data_log.type = 1;
                    _data_log.qty_code = formdata.quantity;
                    _data_log.deal_reference = reference_code;
                    _data_log.remark = formdata.remark;

                    context.DealLogs.Add(_data_log);








                    //Deal Reconcile
                    DealReconcile _data_reconcile = new DealReconcile();
                    _data_reconcile.deal_id = formdata.deal_id.Value;
                    _data_reconcile.deal_reference = reference_code;
                    _data_reconcile.qty_code = formdata.quantity;
                    _data_reconcile.status = 0;
                    //_data_reconcile.remark = remark;
                    _data_reconcile.created_at = DateTime.Now;
                    _data_reconcile.created_by = formdata.requested_by.Value;

                    context.DealReconciles.Add(_data_reconcile);






                    context.SaveChanges();

                    //return Ok(excelFile);
                    result = "success";




                    //Flow approve
                    if (coupon_type != 2)
                    {

                        //send mail
                        //string html = "<p>Alert requested code. waiting for approve</p>";
                        string title = "Alert requested code. waiting for approve";
                        //string body = @"<img src=cid:companyLogo alt='' /><br />" +
                        string body = @"<br /> Hi (Admin), <br />" +
                                    "<br /> Please check, Having order code waiting for approve. <br />";
                        body += "<br /> Deal Customer : " + formdata.deal_customer_name.Replace("&#13;&#10;", "<br />");
                        body += "<br /> Quantity : " + formdata.quantity;
                        body += "<br /> Customer Name : " + formdata.customer_name;
                        body += "<br /> Customer Email : " + formdata.email;

                        ClsHelper clsH = new ClsHelper();
                        clsH.SendEmailToAdmin(title, body, EmailAdmins);


                    }

                    //Flow approve physical reward
                    if (coupon_type == 2) 
                    {
                        //var _deal_req = (dynamic)null;
                        //_deal_req = context.DealRequesteds.ToList<dynamic>().Count();
                        //int deal_req_id = 1;
                        //if (_deal_req > 0)
                        //{
                        //    deal_req_id = context.DealRequesteds.Max(x => x.id) + 1;
                        //}

                        int deal_req_id = 1;
                        deal_req_id = context.DealRequesteds.Max(x => x.id);

                        var _data_req = context.DealRequesteds.SingleOrDefault(x => x.id == deal_req_id);
                        if (_data_req == null)
                        {
                            //return UnprocessableEntity(new { message = "Transaction not found" });
                        }

                        //_data.remark = data.remark;
                        _data_req.updated_at = DateTime.Now;
                        _data_req.updated_by = formdata.requested_by.Value;
                        _data_req.status = 1;

                        SendRequestedCodeFile(_data_req, deal_data.deal_name);
                    }


                    return Ok(new { response = result });
                }
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.ToString()});
            }
        }
        public class FormRequest
        {
            public int? deal_id { get; set; }
            public int? deal_customer_id { get; set; }
            public string deal_customer_name { get; set; }
            public int quantity { get; set; }
            public string deal_quatation_ref { get; set; }
            public IFormFile file_qt_ref { get; set; }
            public string link_sheetQT_ref { get; set; }
            public string deal_quatation { get; set; }
            public IFormFile file_qt { get; set; }
            public string link_sheetQT { get; set; }
            public string email { get; set; }
            public string customer_name { get; set; }
            public IFormFile file_receipt { get; set; }
            public DateTime? deal_start_date { get; set; }
            public DateTime? deal_end_date { get; set; }
            public string remark { get; set; }
            public int? requested_by { get; set; }
            public DateTime? requested_at { get; set; }
            public int? coupon_type { get; set; }
            public IFormFile file_po { get; set; }
            public string link_sheetPO { get; set; }
        }

        private void SendEmailToAdmin(string title, string body)
        {
            try
            {
                //send mail
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("xxxx@xxxx.com", "Privilege Management");
                foreach (var address in EmailAdmins.Value.Emails.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (address != "")
                    {
                        mail.To.Add(new MailAddress(address));
                    }
                }

                mail.Subject = title;

                mail.IsBodyHtml = true;

                string path = Path.Combine("/app/images/header_800.png");
                //string path = $"{Directory.GetCurrentDirectory()}{@"\images\header_800.png"}"; //local

                LinkedResource headerImage = new LinkedResource(path, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                headerImage.ContentId = "companyLogo";
                headerImage.ContentType = new ContentType("image/png");


                body += @"<br /><br /><br /> Best regards,<br />
                            Feyverly Co., Ltd.<br /><br />

                            Thai CC Tower, 270 27th,30th Floor, 43 South Sathorn Road<br />
                            Yannawa, Sathorn, Bangkok Thailand 10120<br />
                            www.feyverly.com<br /><br />

                            m: +66 (0)91 910 5555<br />
                            t: +66 (0)2 673 9335<br />
                            e: Teeyarak@feyverly.com";

                AlternateView av = AlternateView.CreateAlternateViewFromString(body, null, System.Net.Mime.MediaTypeNames.Text.Html);
                av.LinkedResources.Add(headerImage);

                mail.AlternateViews.Add(av);

                string HOST = "email-smtp.ap-southeast-1.amazonaws.com";
                int PORT = 587;

                using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
                {
                    // Pass SMTP credentials
                    client.Credentials = new NetworkCredential("xxxxx", "xxxxxxxxxxxxxxxx");
                    client.EnableSsl = true;

                    try
                    {
                        client.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void SendEmailToCustomer(string title, string body, string email, string customer_name, bool flag_attachment_file, string path_file)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("xxxxx@xxxx.com", "Privilege Management");
                mail.To.Add(new MailAddress(email, customer_name));

                mail.Subject = title;

                mail.IsBodyHtml = true;

                if (flag_attachment_file)
                {
                    string password = RandomString(10);
                    //html += "<p>Password : " + password + "</p>";
                    var path_zip = (dynamic)null;
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.Password = password;
                        string filename = path_file.Replace("uploads/", "").Replace(".xlsx", "") + ".zip";
                        //var pathBuilt = Path.Combine("/app/uploads");
                        var pathBuilt = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                        //zip.AddDirectory(pathBuilt);
                        //zip.AddFile(path_file);
                        zip.AddFile(path_file, @"\");
                        //path_zip = Path.Combine("/app/uploads", filename);
                        path_zip = Path.Combine(Directory.GetCurrentDirectory(), "uploads", filename);
                        zip.Save(path_zip);
                    }

                    if (path_zip != null)
                    {
                        Attachment attachment = new Attachment(path_zip);
                        mail.Attachments.Add(attachment);
                    }
                }


                string path = $"{Directory.GetCurrentDirectory()}{@"\app\images\header_800.png"}";
                //string path = $"{Directory.GetCurrentDirectory()}{@"\images\header_800.png"}"; //local

                LinkedResource headerImage = new LinkedResource(path, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                headerImage.ContentId = "companyLogo";
                headerImage.ContentType = new ContentType("image/png");

                //string body = @"<img src='" + path + "' alt=\"\" />" + html +
                //string body = @"<img src=cid:companyLogo alt='' /><br />" +
                //            "<br /> Hi (" + customer_name + "), <br />" +
                //            "<br /> Your requested code was rejected because " + data.remark.Replace("&#13;&#10;", "<br />");

                body += @"<br /><br /><br /> Best regards,<br />
                            Feyverly Co., Ltd.<br /><br />

                            Thai CC Tower, 270 27th,30th Floor, 43 South Sathorn Road<br />
                            Yannawa, Sathorn, Bangkok Thailand 10120<br />
                            www.feyverly.com<br /><br />

                            m: +66 (0)91 910 5555<br />
                            t: +66 (0)2 673 9335<br />
                            e: Teeyarak@feyverly.com";

                AlternateView av = AlternateView.CreateAlternateViewFromString(body, null, System.Net.Mime.MediaTypeNames.Text.Html);
                av.LinkedResources.Add(headerImage);

                mail.AlternateViews.Add(av);
                //ContentType mimeType = new System.Net.Mime.ContentType("text/html");
                //AlternateView alternate = AlternateView.CreateAlternateViewFromString(body, mimeType);
                //mail.AlternateViews.Add(alternate);

                foreach (var address in EmailAdmins.Value.Emails.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (address != "")
                    {
                        mail.CC.Add(new MailAddress(address));
                    }
                }

                //SmtpClient client = new SmtpClient();
                string HOST = "email-smtp.ap-southeast-1.amazonaws.com";
                int PORT = 587;

                using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
                {
                    // Pass SMTP credentials
                    client.Credentials = new NetworkCredential("xxxxxxxxxxx", "xxxxxxxxxxxxxxx");
                    client.EnableSsl = true;

                    try
                    {
                        client.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private async Task<string> WriteFileStream(byte[] data)
        {
            string url = "";
            string fileName = "";
            try
            {
                fileName = DateTime.Now.Ticks + "-request-code.xlsx";

                var pathBuilt = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

                if (!Directory.Exists(pathBuilt))
                {
                    Directory.CreateDirectory(pathBuilt);
                }

                var path = Path.Combine(Directory.GetCurrentDirectory(), "uploads",
                   fileName);

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    fs.Write(data);
                    //memoryStream.WriteTo(fs);
                    //long fsPos = fs.Position;  // Position= 1869: Good!
                }

                //using (var stream = new FileStream(path, FileMode.Create))
                //{
                //    stream.Write(data);
                //}

                url = string.Format("{0}/{1}", "uploads", fileName);
            }
            catch (Exception e)
            {
                return e.Message.ToString();
            }

            return url;
        }

        private int GetIDAutoIncrementFromDbTable(string schema, string table) 
        {
            int id = 1;
            DataTable dt = new DataTable();
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var setObject = new List<dynamic>();

                    using (var command = context.Database.GetDbConnection().CreateCommand())
                    {

                        command.CommandText = "get_new_id_table";
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        if (command.Connection.State != System.Data.ConnectionState.Open) command.Connection.Open();
                        command.Parameters.Add(new MySqlParameter("_schema", schema));
                        command.Parameters.Add(new MySqlParameter("_table", table));

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var list = new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    var name = reader.GetName(i);
                                    var val = reader.IsDBNull(i) ? null : reader[i];
                                    list.Add(name, val);

                                }
                                setObject.Add(list);
                            }
                        }

                        var json = JsonConvert.SerializeObject(setObject);
                        dt = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));

                        if (dt.Rows.Count > 0) 
                        {
                            id = Convert.ToInt32(dt.Rows[0][0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return id;
        }

        [Authorize]
        [HttpPost("AddSupplier")]
        public async Task<IActionResult> AddSupplier([FromForm] FormDealSupplier data)
        {
            string result = "";

            using var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection);
            using var transaction = context.Database.BeginTransaction();

            try
            {
                transaction.CreateSavepoint("BeforeAddData");

                //using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                //{
                DealSupplier _data = new DealSupplier();
                _data.name = data.name;
                _data.description = data.description;
                _data.link = data.link;

                var csb = new MySqlConnectionStringBuilder(ConnectionString.Value.DefaultConnection);
                string database = csb.Database;
                int id = GetIDAutoIncrementFromDbTable(database, "deal_upload_file");
                int seq_file = 0;
                if (data.file_vat_20 != null)
                {
                    DealUploadFile _data_up_file = new DealUploadFile();
                    _data_up_file.deal_type_of_file = 1;
                    _data_up_file.created_at = DateTime.Now;
                    _data_up_file.created_by = data.created_by;

                    string _filename = await UploadFile(data.file_vat_20);
                    _data_up_file.deal_file_path = _filename;

                    context.DealUploadFiles.Add(_data_up_file);
                    context.SaveChanges();

                    _data.file_vat_20 = id;
                    seq_file++;
                }
                if (data.file_certificate != null)
                {
                    DealUploadFile _data_up_file = new DealUploadFile();
                    _data_up_file.deal_type_of_file = 2;
                    _data_up_file.created_at = DateTime.Now;
                    _data_up_file.created_by = data.created_by;

                    string _filename = await UploadFile(data.file_certificate);
                    _data_up_file.deal_file_path = _filename;

                    context.DealUploadFiles.Add(_data_up_file);
                    context.SaveChanges();

                    _data.file_certificate = id + seq_file;
                    seq_file++;
                }
                if (data.file_bookbank != null)
                {
                    DealUploadFile _data_up_file = new DealUploadFile();
                    _data_up_file.deal_type_of_file = 3;
                    _data_up_file.created_at = DateTime.Now;
                    _data_up_file.created_by = data.created_by;

                    string _filename = await UploadFile(data.file_bookbank);
                    _data_up_file.deal_file_path = _filename;

                    context.DealUploadFiles.Add(_data_up_file);
                    context.SaveChanges();

                    _data.file_bookbank = id + seq_file;
                    seq_file++;
                }
                if (data.file_accept != null)
                {
                    DealUploadFile _data_up_file = new DealUploadFile();
                    _data_up_file.deal_type_of_file = 4;
                    _data_up_file.created_at = DateTime.Now;
                    _data_up_file.created_by = data.created_by;

                    string _filename = await UploadFile(data.file_accept);
                    _data_up_file.deal_file_path = _filename;

                    context.DealUploadFiles.Add(_data_up_file);
                    context.SaveChanges();

                    _data.file_accept = id + seq_file;
                    seq_file++;
                }
                if (data.major != null)
                {
                    DealUploadFile _data_up_file = new DealUploadFile();
                    _data_up_file.deal_type_of_file = 8;
                    _data_up_file.created_at = DateTime.Now;
                    _data_up_file.created_by = data.created_by;

                    string _filename = await UploadFile(data.major);
                    _data_up_file.deal_file_path = _filename;

                    context.DealUploadFiles.Add(_data_up_file);
                    context.SaveChanges();

                    _data.major = id + seq_file;
                }

                _data.remark = data.remark;
                _data.status = 1;
                _data.created_at = DateTime.Now;
                _data.created_by = data.created_by;
                _data.contact_name = data.contact_name;
                _data.contact_phone = data.contact_phone;
                _data.contact_email = data.contact_email;
                _data.type = data.type;
                _data.amount_major = data.amount_major;


                context.DealSuppliers.Add(_data);
                context.SaveChanges();

                transaction.Commit();

                result = "success";
                //}
            }
            catch (Exception ex)
            {
                transaction.RollbackToSavepoint("BeforeAddData");

                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { response = result });
        }

        [Authorize]
        [HttpPost("UpdateSupplier")]
        public async Task<IActionResult> UpdateSupplier([FromForm] FormDealSupplier data)
        {
            string result = "";

            using var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection);
            using var transaction = context.Database.BeginTransaction();

            try
            {
                transaction.CreateSavepoint("BeforeAddData");

                //using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                //{
                var user = context.UserProfiles.SingleOrDefault(x => x.emp_id == data.updated_by);
                    if (user == null)
                    {
                        return UnprocessableEntity(new { message = "User not found" });
                    }

                    var _data = context.DealSuppliers.SingleOrDefault(x => x.id == data.id);
                    if (_data == null)
                    {
                        return UnprocessableEntity(new { message = "Transaction not found" });
                    }

                    _data.name = data.name;
                    _data.description = data.description;
                    _data.link = data.link;

                    var csb = new MySqlConnectionStringBuilder(ConnectionString.Value.DefaultConnection);
                    string database = csb.Database;
                    int id = GetIDAutoIncrementFromDbTable(database, "deal_upload_file");
                    int seq_file = 0;
                    if (data.file_vat_20 != null)
                    {
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 1;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.file_vat_20);
                        _data_up_file.deal_file_path = _filename;

                        context.DealUploadFiles.Add(_data_up_file);
                        context.SaveChanges();

                        _data.file_vat_20 = id;
                        seq_file++;
                    }
                    if (data.file_certificate != null)
                    {
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 2;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.file_certificate);
                        _data_up_file.deal_file_path = _filename;

                        context.DealUploadFiles.Add(_data_up_file);
                        context.SaveChanges();

                        _data.file_certificate = id + seq_file;
                        seq_file++;
                    }
                    if (data.file_bookbank != null)
                    {
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 3;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.file_bookbank);
                        _data_up_file.deal_file_path = _filename;

                        context.DealUploadFiles.Add(_data_up_file);
                        context.SaveChanges();

                        _data.file_bookbank = id + seq_file;
                        seq_file++;
                    }
                    if (data.file_accept != null)
                    {
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 4;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.file_accept);
                        _data_up_file.deal_file_path = _filename;

                        context.DealUploadFiles.Add(_data_up_file);
                        context.SaveChanges();

                        _data.file_accept = id + seq_file;
                        seq_file++;
                }
                if (data.major != null)
                    {
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 8;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.major);
                        _data_up_file.deal_file_path = _filename;

                        context.DealUploadFiles.Add(_data_up_file);
                        context.SaveChanges();

                        _data.major = id + seq_file;
                    }

                _data.remark = data.remark;
                    _data.updated_at = DateTime.Now;
                    _data.updated_by = user.id;
                    _data.status = data.status.Value;
                    _data.contact_name = data.contact_name;
                    _data.contact_phone = data.contact_phone;
                    _data.contact_email = data.contact_email;
                    _data.type = data.type;
                    _data.amount_major = data.amount_major;

                    context.SaveChanges();

                transaction.Commit();

                result = "success";
                //}
            }
            catch (Exception ex)
            {
                transaction.RollbackToSavepoint("BeforeAddData");

                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { response = result });
        }

        [Authorize]
        [HttpPost("DeleteSupplier")]
        public IActionResult DeleteSupplier([FromQuery] string emp_id, [FromQuery] string id)
        {
            string result = "";
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var user = context.UserProfiles.SingleOrDefault(x => x.emp_id.ToString() == emp_id);
                    if (user == null)
                    {
                        return UnprocessableEntity(new { message = "User not found" });
                    }

                    var _data = context.DealSuppliers.SingleOrDefault(x => x.id.ToString() == id);
                    if (_data == null)
                    {
                        return UnprocessableEntity(new { message = "Transaction not found" });
                    }

                    _data.status = 0;
                    _data.updated_by = user.id;
                    _data.updated_at = DateTime.Now;
                    _data.deleted_at = DateTime.Now;

                    //context.DealSuppliers.Remove(_data);

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
        public class FormDealSupplier
        {
            public int? id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string link { get; set; }
            public IFormFile major { get; set; }
            public IFormFile file_vat_20 { get; set; }
            public IFormFile file_certificate { get; set; }
            public IFormFile file_bookbank { get; set; }
            public IFormFile file_accept { get; set; }
            public string remark { get; set; }
            public int? status { get; set; }
            public string contact_name { get; set; }
            public string contact_phone { get; set; }
            public string contact_email { get; set; }
            public int? type { get; set; }
            public int? amount_major { get; set; }
            public int? created_by { get; set; }
            public DateTime? created_at { get; set; }
            public int? updated_by { get; set; }
            public DateTime? updated_at { get; set; }
            public DateTime? deleted_at { get; set; }
        }


        [Authorize]
        [HttpPost("AddCustomer")]
        public async Task<IActionResult> AddCustomer([FromForm] FormDealCustomer data)
        {
            string result = "";
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var csb = new MySqlConnectionStringBuilder(ConnectionString.Value.DefaultConnection);
                    string database = csb.Database;
                    int id = GetIDAutoIncrementFromDbTable(database, "deal_upload_file");

                    DealCustomer _data = new DealCustomer();
                    _data.name = data.name;
                    _data.description = data.description;
                    if (data.doc != null)
                    {
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 9;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.doc);
                        _data_up_file.deal_file_path = _filename;

                        context.DealUploadFiles.Add(_data_up_file);
                        _data.doc_id = id;

                        //string _filename = await UploadFile(data.doc);
                        //_data.doc = _filename;
                    }
                    _data.remark = data.remark;
                    _data.status = 1;
                    _data.created_at = DateTime.Now;
                    _data.created_by = data.created_by;

                    _data.contact_name = data.contact_name;
                    _data.contact_phone = data.contact_phone;
                    _data.contact_email = data.contact_email;

                    context.DealCustomers.Add(_data);
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
        [HttpPost("UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer([FromForm] FormDealCustomer data)
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

                    var _data = context.DealCustomers.SingleOrDefault(x => x.id == data.id);
                    if (_data == null)
                    {
                        return UnprocessableEntity(new { message = "Transaction not found" });
                    }

                    _data.name = data.name;
                    _data.description = data.description;

                    var csb = new MySqlConnectionStringBuilder(ConnectionString.Value.DefaultConnection);
                    string database = csb.Database;
                    int id = GetIDAutoIncrementFromDbTable(database, "deal_upload_file");
                    if (data.doc != null)
                    {
                        DealUploadFile _data_up_file = new DealUploadFile();
                        _data_up_file.deal_type_of_file = 9;
                        _data_up_file.created_at = DateTime.Now;
                        _data_up_file.created_by = data.created_by;

                        string _filename = await UploadFile(data.doc);
                        _data_up_file.deal_file_path = _filename;

                        context.DealUploadFiles.Add(_data_up_file);
                        _data.doc_id = id;

                        //string _filename = await UploadFile(data.doc);
                        //_data.doc = _filename;
                    }
                    //else
                    //{
                    //    _data.doc = null;
                    //}
                    _data.remark = data.remark;
                    _data.updated_at = DateTime.Now;
                    _data.updated_by = user.id;
                    _data.status = data.status;

                    _data.contact_name = data.contact_name;
                    _data.contact_phone = data.contact_phone;
                    _data.contact_email = data.contact_email;

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
        [HttpPost("DeleteCustomer")]
        public IActionResult DeleteCustomer([FromQuery] string emp_id, [FromQuery] string id)
        {
            string result = "";
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var user = context.UserProfiles.SingleOrDefault(x => x.emp_id.ToString() == emp_id);
                    if (user == null)
                    {
                        return UnprocessableEntity(new { message = "User not found" });
                    }

                    var _data = context.DealCustomers.SingleOrDefault(x => x.id.ToString() == id);
                    if (_data == null)
                    {
                        return UnprocessableEntity(new { message = "Transaction not found" });
                    }

                    _data.updated_by = user.id;
                    _data.updated_at = DateTime.Now;
                    _data.deleted_at = DateTime.Now;
                    _data.status = 0;

                    //context.DealSuppliers.Remove(_data);

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
        public class FormDealCustomer
        {
            public int id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public IFormFile doc { get; set; }
            public string remark { get; set; }
            public int status { get; set; }
            public string contact_name { get; set; }
            public string contact_phone { get; set; }
            public string contact_email { get; set; }
            public int? created_by { get; set; }
            public DateTime? created_at { get; set; }
            public int? updated_by { get; set; }
            public DateTime? updated_at { get; set; }
            public DateTime? deleted_at { get; set; }
        }


        //[Authorize]
        [HttpPost("ApprovedRequested")]
        public IActionResult ApprovedRequested([FromForm] DealRequested data)
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

                    var _data = context.DealRequesteds.SingleOrDefault(x => x.id == data.id);
                    if (_data == null)
                    {
                        return UnprocessableEntity(new { message = "Transaction not found" });
                    }

                    var data_deal = context.Deals.SingleOrDefault(x => x.id == data.deal_id);
                    if (data_deal == null)
                    {
                        return UnprocessableEntity(new { message = "Deal not found" });
                    }
                    string deal_name = data_deal.deal_name;

                    //_data.remark = data.remark;
                    _data.updated_at = DateTime.Now;
                    _data.updated_by = user.id;
                    _data.status = data.status;

                    if (data.status == 1)
                    {
                        SendRequestedCodeFile(_data, deal_name);
                        //SendRequestedCodeFile(_data.deal_requested_file, _data.deal_email);
                    }
                    else if (data.status == 2)
                    {
                        ReverseRequestedCode(_data.deal_reference);


                        ////string html = "<p>Rejected</p>";
                        string title = "Rejected Requested Code";
                        string customer_name = _data.deal_customer_name;
                        //string body = @"<img src=cid:companyLogo alt='' /><br />";
                        string body = "<br /> Hi (" + customer_name + "), <br />" +
                                    "<br /> Your requested code was rejected because " + data.remark.Replace("&#13;&#10;", "<br />");
                        string email = _data.deal_email;

                        ClsHelper clsH = new ClsHelper();
                        clsH.SendEmailToCustomer(title, body, email, customer_name, false, "", "", EmailAdmins);

                    }

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
        //[HttpPost("SendRequestedCodeFile")]
        private void SendRequestedCodeFile(DealRequested data, string deal_name)
        {
            try
            {

                string path_file = data.deal_requested_file;
                string email = data.deal_email;
                string customer_name = data.deal_customer_name;
                //string html = "<p>File Requested Code</p>";
                //string emailMe = "";
                //DateTime d = new DateTime();
                string title = "File Requested Code " + DateTime.Now.ToShortDateString();


                string password = RandomString(10);
                //string body = @"<img src=cid:companyLogo alt='' /><br />";
                string body = @"<br /> Hi (" + customer_name + "), <br />" +
                            "<br /> Your Requested code was approved <br />" +
                            "<br /> Deal name : " + deal_name + " <br />" +
                            "<br /> Quantity : " + data.deal_quantity + " <br />" +
                            "<br /> Password file is " + password + "<br /> ";


                ClsHelper clsH = new ClsHelper();
                clsH.SendEmailToCustomer(title, body, email, customer_name, true, path_file, password, EmailAdmins);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //[HttpPost("ReverseRequestedCode")]
        private void ReverseRequestedCode(string deal_reference)
        {
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var dataDealCode = context.DealCodes.Where(x => x.deal_reference == deal_reference)
                                        .AsEnumerable()
                                        .ToList();
                    if (dataDealCode != null)
                    {
                        foreach (var x in dataDealCode)
                        {
                            x.requested_at = null;
                            x.requested_by = null;
                        }

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [Authorize]
        [HttpPost("AddOrderCode")]
        public IActionResult AddOrderCode([FromForm] FormDealOrderCode data)
        {
            string result = "";
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var deal_data = context.Deals.FirstOrDefault((c) => c.id == data.deal_id);
                    if (deal_data == null)
                    {
                        return UnprocessableEntity(new { message = "Deal not found" });
                    }
                    if (deal_data.minimum_order != null)
                    {
                        if (deal_data.minimum_order > data.quantity)
                        {
                            return UnprocessableEntity(new { message = "Quantity of order less than deal order minimum. Deal order minimum is " + deal_data.minimum_order.ToString() });
                        }
                    }
                    if (deal_data.maximum_order != null)
                    {
                        if (deal_data.maximum_order < data.quantity)
                        {
                            return UnprocessableEntity(new { message = "Quantity of order more than deal order maximum. Deal order maximum is " + deal_data.maximum_order.ToString() });
                        }
                    }

                    DealOrderCode _data = new DealOrderCode();
                    _data.deal_id = data.deal_id;
                    _data.quantity = data.quantity;
                    _data.remark = data.remark;
                    _data.status = 0;
                    _data.created_at = DateTime.Now;
                    _data.created_by = data.created_by;

                    DateTime? start_date = null;
                    DateTime? end_date = null;
                    if (data.physical_reward != 1)
                    {
                        System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                        //DateTime start_date = DateTime.ParseExact(data.start_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        //DateTime end_date = DateTime.ParseExact(data.end_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        start_date = DateTime.ParseExact(data.start_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        end_date = DateTime.ParseExact(data.end_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        _data.start_date = start_date.Value;
                        _data.end_date = end_date.Value;
                    }

                    context.DealOrderCodes.Add(_data);


                    context.SaveChanges();
                    result = "success";




                    //send mail
                    //string html = "<p>Alert order code.</p>";
                    string title = "Alert order code";
                    //string body = @"<img src=cid:companyLogo alt='' /><br />" +
                    string body = @"<br /> Hi (Admin), <br />" +
                                "<br /> Please check, Having order code waiting for approve. <br />";

                    body += "<br /> Deal ID : " + data.deal_id.ToString();
                    body += "<br /> Deal Name : " + data.deal_name;
                    body += "<br /> Remark : " + (data.remark != null ? data.remark.Replace("&#13;&#10;", "<br />") : "");

                    if (data.physical_reward != 1)
                    {
                        body += "<br /> Start Date : " + start_date.Value.ToShortDateString();
                        body += "<br /> End Date : " + end_date.Value.ToShortDateString();
                        body += "<br /> Quantity : " + data.quantity;
                    }

                    body += "<br /> ";

                    ClsHelper clsH = new ClsHelper();
                    clsH.SendEmailToAdmin(title, body, EmailAdmins);



                }
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { response = result });
        }
        public class FormDealOrderCode
        {
            public int? id { get; set; }
            public int deal_id { get; set; }
            public string deal_name { get; set; }
            public string remark { get; set; }
            public int? quantity { get; set; }
            public int? status { get; set; }
            public int? created_by { get; set; }
            public DateTime? created_at { get; set; }
            public int? updated_by { get; set; }
            public DateTime? updated_at { get; set; }
            public DateTime? start_date { get; set; }
            public DateTime? end_date { get; set; }
            public int? physical_reward { get; set; }
        }

        [Authorize]
        [HttpPost("UpdateOrderCode")]
        public IActionResult UpdateOrderCode([FromForm] FormDealOrderCode data)
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

                    var _data_deal = context.Deals.SingleOrDefault(x => x.id == data.deal_id);
                    if (_data_deal == null)
                    {
                        return UnprocessableEntity(new { message = "Deal Transaction not found" });
                    }

                    var _data = context.DealOrderCodes.SingleOrDefault(x => x.id == data.id);
                    if (_data == null)
                    {
                        return UnprocessableEntity(new { message = "Transaction not found" });
                    }

                    var user_created = context.UserProfiles.SingleOrDefault(x => x.emp_id == data.created_by);
                    if (user_created == null)
                    {
                        return UnprocessableEntity(new { message = "User created not found" });
                    }

                    _data.deal_id = data.deal_id;
                    _data.remark = data.remark;
                    _data.status = data.status;
                    _data.created_at = DateTime.Now;
                    _data.remark = data.remark;
                    _data.updated_at = DateTime.Now;
                    _data.updated_by = user.id;
                    _data.status = data.status;

                    context.SaveChanges();
                    result = "success";


                    //send mail
                    //string html = "<p>Alert update status order code.</p>";
                    string title = "Alert update status order code";
                    //string body = @"<img src=cid:companyLogo alt='' /><br />" +
                    string body = @"<br /> Hi (Admin), <br />" +
                                "<br /> Please check, Having order code waiting for approve. <br />";

                    body += "<br /> Deal ID : " + data.deal_id.ToString();
                    body += "<br /> Deal Name : " + (data.deal_name.ToString() != "" ? data.deal_name.ToString().Replace("&#13;&#10;", "<br />") : "");
                    body += "<br /> Remark : " + (data.remark != null ? data.remark.ToString().Replace("&#13;&#10;", "<br />") : "");
                    body += "<br /> Status : " + (data.status == 0 ? "Pending" : (data.status == 1 ? "On Progress" : (data.status == 2 ? "Done" : (data.status == 3 ? "Reject" : ""))));


                    if (_data_deal.deal_coupon_type != 2)
                    {
                        body += "<br /> Start Date : " + _data.start_date.Value.ToShortDateString();
                        body += "<br /> End Date : " + _data.end_date.Value.ToShortDateString();
                    }

                    ClsHelper clsH = new ClsHelper();
                    clsH.SendEmailToAdmin(title, body, EmailAdmins);



                }
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { response = result });
        }

        [Authorize]
        [HttpPost("DeleteOrderCode")]
        public IActionResult DeleteOrderCode([FromQuery] string emp_id, [FromQuery] string id)
        {
            string result = "";
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var user = context.UserProfiles.SingleOrDefault(x => x.emp_id.ToString() == emp_id);
                    if (user == null)
                    {
                        return UnprocessableEntity(new { message = "User not found" });
                    }

                    var _data = context.DealOrderCodes.SingleOrDefault(x => x.id.ToString() == id);
                    if (_data == null)
                    {
                        return UnprocessableEntity(new { message = "Transaction not found" });
                    }
                    _data.status = 4;
                    _data.updated_by = user.id;
                    _data.updated_at = DateTime.Now;
                    //_data.deleted_at = DateTime.Now;

                    //context.DealSuppliers.Remove(_data);

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



        public class Response<T>
        {
            public Response()
            {
            }
            public Response(T data)
            {
                Succeeded = true;
                Message = string.Empty;
                Errors = null;
                Data = data;
            }
            public T Data { get; set; }
            public bool Succeeded { get; set; }
            public string[] Errors { get; set; }
            public string Message { get; set; }
        }
        public class PagedResponse<T> : Response<T>
        {
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
            public int TotalRecords { get; set; }
            public PagedResponse(T data, int pageNumber, int pageSize, int totalPage, int totalRecord)
            {
                this.PageNumber = pageNumber;
                this.PageSize = pageSize;
                this.TotalPages = totalPage;
                this.TotalRecords = totalRecord;
                this.Data = data;
                this.Message = null;
                this.Succeeded = true;
                this.Errors = null;
            }
        }
        public class PaginationFilter
        {
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public string DateFrom { get; set; }
            public string DateTo { get; set; }
            public string Search { get; set; }

            public PaginationFilter()
            {
                this.PageNumber = 1;
                this.PageSize = 10;
            }
            public PaginationFilter(int pageNumber, int pageSize)
            {
                this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
                this.PageSize = pageSize;
            }
            public PaginationFilter(int pageNumber, int pageSize, string dateFrom, string dateTo, string search)
            {
                this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
                this.PageSize = pageSize;
                this.DateFrom = dateFrom;
                this.DateTo = dateTo;
                this.Search = search;
            }
        }
    }

}
