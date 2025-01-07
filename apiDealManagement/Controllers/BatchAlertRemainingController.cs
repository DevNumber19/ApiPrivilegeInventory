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

namespace apiDealManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BatchAlertRemainingController : Controller
    {
        private readonly IOptions<ConnectionStringConfig> ConnectionString;
        private readonly IOptions<EmailAdmin> EmailAdmins;
        public BatchAlertRemainingController(IOptions<ConnectionStringConfig> ConnectionString, IOptions<EmailAdmin> EmailAdmins)
        {
            this.ConnectionString = ConnectionString;
            this.EmailAdmins = EmailAdmins;
        }

        //[Authorize]
        [HttpGet]
        [Route("GetDealTimeOut")]
        public IActionResult Get()
        {
            var data = (dynamic)null;

            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    ClsHelper clsH = new ClsHelper();

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
                            .Where(x => x.deal_remaining <= 120)
                            .OrderByDescending(x => x.deal_remaining)
                            .ToList();

                    if (data == null)
                    {
                        return UnprocessableEntity(new { message = "data not found" });
                    }


                    DataTable dt = clsH.ListToDataTable(data);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            string deal_name = dr["deal_name"].ToString();
                            string deal_start_date = dr["deal_start_date"].ToString();
                            string deal_end_date = dr["deal_end_date"].ToString();
                            string quantity_code = dr["quantity_code"].ToString();
                            string deal_supplier = dr["deal_supplier"].ToString();

                            string title = "Alert remaining code. Waiting for check";
                            //string body = @"<img src=cid:companyLogo alt='' /><br />" +
                            string body = @"<br /> Hi (Admin), <br />" +
                                        "<br /> Please check, Having deal remaining less than 120 days. <br />";
                            body += "<br /> Deal Name : " + deal_name;
                            body += "<br /> Supplier : " + deal_supplier.Replace("&#13;&#10;", "<br />");
                            body += "<br /> Quantity : " + quantity_code;
                            body += "<br /> Start Date : " + deal_start_date;
                            body += "<br /> End Date : " + deal_end_date;

                            clsH.SendEmailToAdmin(title, body, EmailAdmins);
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok();
        }

    }
}
