using apiDealManagement.Helper;
using apiDealManagement.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
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
    public class PrivilegePhysicalController : Controller
    {
        private readonly IOptions<ConnectionStringConfig> ConnectionString;
        public PrivilegePhysicalController(IOptions<ConnectionStringConfig> ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }


        [Authorize]
        [HttpGet("GetDealList")]
        public IActionResult Get(string name, [FromQuery] PaginationFilter filter, int? getall)
        {
            var data = (dynamic)null;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            var totalRecords = 0;
            var totalPages = 0;

            ClsHelper clsH = new ClsHelper();

            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var count = 0;

                    if (getall != null && getall == 1)
                    {

                        data = (
                            from a in context.Deals
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
                                deal_discount_value = a.deal_discount_value,
                                deal_summary_value = a.deal_summary_value,
                                deal_add_item = a.deal_add_item,
                                deal_add_amount = a.deal_add_amount,
                                deal_add_value = a.deal_add_value,
                                deal_pr_number = a.deal_pr_number,
                                deal_pr_file = a.deal_pr_file
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
                                //quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count(),
                                quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),

                                deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                deal_coupon_type = x.deal_coupon_type,
                                deal_coupon_value = x.deal_coupon_value,
                                deal_permission = x.deal_permission,
                                deal_quatation = x.deal_quatation,
                                deal_quatation_link = x.deal_quatation_link,
                                deal_merchant_id = x.deal_merchant_id,
                                deal_merchant_name = x.deal_merchant_name,
                                deal_invoice = x.deal_invoice,
                                deal_receipt = x.deal_receipt,
                                deal_start_date = x.deal_start_date,
                                deal_end_date = x.deal_end_date,
                                deal_coupon_name = x.deal_coupon_name,
                                deal_cost = x.deal_cost,
                                deal_condition = x.deal_condition,
                                deal_coupon_image = x.deal_coupon_image,
                                deal_major = x.deal_major,
                                deal_remaining = clsH.CompareDate(x.deal_start_date, x.deal_end_date),
                                deal_discount_value = x.deal_discount_value,
                                deal_summary_value = x.deal_summary_value,
                                deal_add_item = x.deal_add_item,
                                deal_add_amount = x.deal_add_amount,
                                deal_add_value = x.deal_add_value,
                                deal_pr_number = x.deal_pr_number,
                                deal_pr_file = x.deal_pr_file
                            })
                                .OrderByDescending(x => x.deal_remaining)
                                .ToList<dynamic>();

                        return Ok(data);
                    }

                    if (filter.DateFrom != null && filter.DateTo != null)
                    {
                        System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                        DateTime date_from = DateTime.ParseExact(Convert.ToDateTime(filter.DateFrom).ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                        DateTime date_to = DateTime.ParseExact(Convert.ToDateTime(filter.DateTo).ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo).AddDays(1);

                        if (name == null || name == "" || name == "null")
                        {
                            data = (
                                from a in context.Deals.Where(x => x.created_at >= date_from && x.created_at < date_to)
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
                                    deal_discount_value = a.deal_discount_value,
                                    deal_summary_value = a.deal_summary_value,
                                    deal_add_item = a.deal_add_item,
                                    deal_add_amount = a.deal_add_amount,
                                    deal_add_value = a.deal_add_value,
                                    deal_pr_number = a.deal_pr_number,
                                    deal_pr_file = a.deal_pr_file
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
                                        quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),

                                        deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                        deal_coupon_type = x.deal_coupon_type,
                                        deal_coupon_value = x.deal_coupon_value,
                                        deal_permission = x.deal_permission,
                                        deal_quatation = x.deal_quatation,
                                        deal_quatation_link = x.deal_quatation_link,
                                        deal_merchant_id = x.deal_merchant_id,
                                        deal_merchant_name = x.deal_merchant_name,
                                        deal_invoice = x.deal_invoice,
                                        deal_receipt = x.deal_receipt,
                                        deal_start_date = x.deal_start_date,
                                        deal_end_date = x.deal_end_date,
                                        deal_coupon_name = x.deal_coupon_name,
                                        deal_cost = x.deal_cost,
                                        deal_condition = x.deal_condition,
                                        deal_coupon_image = x.deal_coupon_image,
                                        deal_major = x.deal_major,
                                        deal_remaining = clsH.CompareDate(x.deal_start_date, x.deal_end_date),
                                        deal_discount_value = x.deal_discount_value,
                                        deal_summary_value = x.deal_summary_value,
                                        deal_add_item = x.deal_add_item,
                                        deal_add_amount = x.deal_add_amount,
                                        deal_add_value = x.deal_add_value,
                                        deal_pr_number = x.deal_pr_number,
                                        deal_pr_file = x.deal_pr_file
                                    })
                                    .OrderByDescending(x => x.deal_remaining)
                                    .ToList<dynamic>();




                            count = (
                                from a in context.Deals.Where(x => x.created_at >= date_from && x.created_at < date_to)
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
                                from a in context.Deals.Where(x => x.created_at >= date_from && x.created_at < date_to)
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
                                    deal_discount_value = a.deal_discount_value,
                                    deal_summary_value = a.deal_summary_value,
                                    deal_add_item = a.deal_add_item,
                                    deal_add_amount = a.deal_add_amount,
                                    deal_add_value = a.deal_add_value,
                                    deal_pr_number = a.deal_pr_number,
                                    deal_pr_file = a.deal_pr_file
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
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
                                    quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),

                                    deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                    deal_coupon_type = x.deal_coupon_type,
                                    deal_coupon_value = x.deal_coupon_value,
                                    deal_permission = x.deal_permission,
                                    deal_quatation = x.deal_quatation,
                                    deal_quatation_link = x.deal_quatation_link,
                                    deal_merchant_id = x.deal_merchant_id,
                                    deal_merchant_name = x.deal_merchant_name,
                                    deal_invoice = x.deal_invoice,
                                    deal_receipt = x.deal_receipt,
                                    deal_start_date = x.deal_start_date,
                                    deal_end_date = x.deal_end_date,
                                    deal_coupon_name = x.deal_coupon_name,
                                    deal_cost = x.deal_cost,
                                    deal_condition = x.deal_condition,
                                    deal_coupon_image = x.deal_coupon_image,
                                    deal_major = x.deal_major,
                                    deal_remaining = clsH.CompareDate(x.deal_start_date, x.deal_end_date),
                                    deal_discount_value = x.deal_discount_value,
                                    deal_summary_value = x.deal_summary_value,
                                    deal_add_item = x.deal_add_item,
                                    deal_add_amount = x.deal_add_amount,
                                    deal_add_value = x.deal_add_value,
                                    deal_pr_number = x.deal_pr_number,
                                    deal_pr_file = x.deal_pr_file
                                })
                                    .OrderByDescending(x => x.deal_remaining)
                                    .ToList<dynamic>();


                            count = (
                                from a in context.Deals.Where(x => x.created_at >= date_from && x.created_at < date_to)
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
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.created_at)
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
                                from a in context.Deals
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
                                    deal_discount_value = a.deal_discount_value,
                                    deal_summary_value = a.deal_summary_value,
                                    deal_add_item = a.deal_add_item,
                                    deal_add_amount = a.deal_add_amount,
                                    deal_add_value = a.deal_add_value,
                                    deal_pr_number = a.deal_pr_number,
                                    deal_pr_file = a.deal_pr_file
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
                                    quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),

                                    deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                    deal_coupon_type = x.deal_coupon_type,
                                    deal_coupon_value = x.deal_coupon_value,
                                    deal_permission = x.deal_permission,
                                    deal_quatation = x.deal_quatation,
                                    deal_quatation_link = x.deal_quatation_link,
                                    deal_merchant_id = x.deal_merchant_id,
                                    deal_merchant_name = x.deal_merchant_name,
                                    deal_invoice = x.deal_invoice,
                                    deal_receipt = x.deal_receipt,
                                    deal_start_date = x.deal_start_date,
                                    deal_end_date = x.deal_end_date,
                                    deal_coupon_name = x.deal_coupon_name,
                                    deal_cost = x.deal_cost,
                                    deal_condition = x.deal_condition,
                                    deal_coupon_image = x.deal_coupon_image,
                                    deal_major = x.deal_major,
                                    deal_remaining = clsH.CompareDate(x.deal_start_date, x.deal_end_date),
                                    deal_discount_value = x.deal_discount_value,
                                    deal_summary_value = x.deal_summary_value,
                                    deal_add_item = x.deal_add_item,
                                    deal_add_amount = x.deal_add_amount,
                                    deal_add_value = x.deal_add_value,
                                    deal_pr_number = x.deal_pr_number,
                                    deal_pr_file = x.deal_pr_file
                                })
                                    .OrderByDescending(x => x.deal_remaining)
                                    .ToList<dynamic>();




                            count = (
                                from a in context.Deals
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
                                from a in context.Deals
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
                                    deal_discount_value = a.deal_discount_value,
                                    deal_summary_value = a.deal_summary_value,
                                    deal_add_item = a.deal_add_item,
                                    deal_add_amount = a.deal_add_amount,
                                    deal_add_value = a.deal_add_value,
                                    deal_pr_number = a.deal_pr_number,
                                    deal_pr_file = a.deal_pr_file
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
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
                                    quantity_code = context.DealCodes.Where(e => e.deal_id.Value == x.deal_id && e.requested_at == null && e.requested_by == null).AsEnumerable().ToList().Count(),

                                    deal_value_total = x.deal_coupon_value * (context.DealCodes.Where(e => e.deal_id.Value == x.deal_id).AsEnumerable().ToList().Count()),

                                    deal_coupon_type = x.deal_coupon_type,
                                    deal_coupon_value = x.deal_coupon_value,
                                    deal_permission = x.deal_permission,
                                    deal_quatation = x.deal_quatation,
                                    deal_quatation_link = x.deal_quatation_link,
                                    deal_merchant_id = x.deal_merchant_id,
                                    deal_merchant_name = x.deal_merchant_name,
                                    deal_invoice = x.deal_invoice,
                                    deal_receipt = x.deal_receipt,
                                    deal_start_date = x.deal_start_date,
                                    deal_end_date = x.deal_end_date,
                                    deal_coupon_name = x.deal_coupon_name,
                                    deal_cost = x.deal_cost,
                                    deal_condition = x.deal_condition,
                                    deal_coupon_image = x.deal_coupon_image,
                                    deal_major = x.deal_major,
                                    deal_remaining = clsH.CompareDate(x.deal_start_date, x.deal_end_date),
                                    deal_discount_value = x.deal_discount_value,
                                    deal_summary_value = x.deal_summary_value,
                                    deal_add_item = x.deal_add_item,
                                    deal_add_amount = x.deal_add_amount,
                                    deal_add_value = x.deal_add_value,
                                    deal_pr_number = x.deal_pr_number,
                                    deal_pr_file = x.deal_pr_file
                                })
                                    .OrderByDescending(x => x.deal_remaining)
                                    .ToList<dynamic>();


                            count = (
                                from a in context.Deals
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
                                .Where(x => x.deal_name.Contains(name))
                                .OrderByDescending(x => x.created_at)
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
        [HttpPost("AddDeal")]
        public async Task<IActionResult> Add([FromForm] FormDeal data)
        {
            //string status = "YES";
            string result = "";
            string filename = "";
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var _deal = (dynamic)null;
                    _deal = context.Deals.ToList<dynamic>().Count();
                    int deal_id = 1;
                    if (_deal > 0)
                    {
                        deal_id = context.Deals.Max(x => x.id) + 1;
                    }

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

                    if (data.file_qt != null)
                    {
                        string _filename = await UploadFile(data.file_qt);
                        _data.deal_quatation = _filename;
                    }
                    else if (data.link_sheetQT != "" && data.link_sheetQT != null)
                    {
                        _data.deal_quatation_link = data.link_sheetQT;
                    }

                    //_data.deal_merchant_id = data.;
                    //_data.deal_merchant_name = data.;

                    _data.deal_invoice = data.invoice;

                    if (data.file_receipt != null)
                    {
                        string _filename = await UploadFile(data.file_receipt);
                        _data.deal_receipt = _filename;
                    }


                    System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                    DateTime start_date = DateTime.ParseExact(data.start_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                    DateTime end_date = DateTime.ParseExact(data.end_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                    _data.deal_start_date = start_date;
                    _data.deal_end_date = end_date;


                    //_data.deal_coupon_name = data.;
                    //_data.deal_cost = data.;
                    //_data.deal_condition = data.;

                    if (data.file_image != null)
                    {
                        string _filename = await UploadFile(data.file_image);
                        _data.deal_coupon_image = _filename;
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
                        string _filename = await UploadFile(data.deal_pr_file);
                        _data.deal_pr_file = _filename;
                    }

                    context.Deals.Add(_data);


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
                                    if (column_deal_coupon_code != "deal_coupon_code")
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
                                            DealCode _data_dealcode = new DealCode();
                                            _data_dealcode.deal_id = deal_id;
                                            _data_dealcode.deal_coupon_code = _code;
                                            _data_dealcode.created_at = DateTime.Now;
                                            _data_dealcode.created_by = data.created_by;

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
                        //_data_log.remark = remark;
                        _data_log.created_at = DateTime.Now;

                        context.DealLogs.Add(_data_log);
                    }


                    //Incorrect datetime value: '1479-09-02 00:00:00' for column `dev-recordsystems`.`deal`.`deal_end_date` at row 1
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
            public IFormFile file_receipt { get; set; }
            public decimal? deal_discount_value { get; set; }
            public int? deal_summary_value { get; set; }
            public string deal_add_item { get; set; }
            public int? deal_add_amount { get; set; }
            public decimal? deal_add_value { get; set; }
            public string deal_pr_number { get; set; }
            public IFormFile deal_pr_file { get; set; }
        }
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
        [HttpPost("UpdateDeal")]
        public async Task<IActionResult> Update([FromForm] FormDeal data)
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

                    if (data.file_qt != null)
                    {
                        string _filename = await UploadFile(data.file_qt);
                        deal_data.deal_quatation = _filename;
                    }
                    //else
                    //{
                    //    deal_data.deal_quatation = null;
                    //}

                    if (data.link_sheetQT != "" && data.link_sheetQT != null)
                    {
                        deal_data.deal_quatation_link = data.link_sheetQT;
                    }

                    //deal_data.deal_merchant_id = data.;
                    //deal_data.deal_merchant_name = data.;

                    deal_data.deal_invoice = data.invoice;

                    if (data.file_receipt != null)
                    {
                        string _filename = await UploadFile(data.file_receipt);
                        deal_data.deal_receipt = _filename;
                    }
                    //else
                    //{
                    //    deal_data.deal_receipt = null;
                    //}


                    System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                    DateTime start_date = DateTime.ParseExact(data.start_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                    DateTime end_date = DateTime.ParseExact(data.end_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                    deal_data.deal_start_date = start_date;
                    deal_data.deal_end_date = end_date;


                    //deal_data.deal_coupon_name = data.;
                    //deal_data.deal_cost = data.;
                    //deal_data.deal_condition = data.;

                    if (data.file_image != null)
                    {
                        string _filename = await UploadFile(data.file_image);
                        deal_data.deal_coupon_image = _filename;
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
                        string _filename = await UploadFile(data.deal_pr_file);
                        deal_data.deal_pr_file = _filename;
                    }


                    DealLog _data_log = new DealLog();
                    _data_log.user = data.updated_by.Value;
                    _data_log.action = 1;
                    _data_log.deal_id = data.deal_id.Value;
                    _data_log.created_at = DateTime.Now;

                    context.DealLogs.Add(_data_log);


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


    }
}
