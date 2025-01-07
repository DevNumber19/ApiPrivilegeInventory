using apiDealManagement.Helper;
using apiDealManagement.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
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
    public class TaxInvoiceController : Controller
    {
        private readonly IOptions<ConnectionStringConfig> ConnectionString;
        public TaxInvoiceController(IOptions<ConnectionStringConfig> ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }


        [Authorize]
        [HttpGet("Get")]
        public IActionResult GetUserTaxInvoices(string name, [FromQuery] PaginationFilter filter, int? getall)
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
                            from u in context.DealUserTaxInvoices
                            join inv_detail in context.DealInvoiceDetails on u.inv_no equals inv_detail.inv_no into _inv_detail
                            from inv_d in _inv_detail.DefaultIfEmpty()
                            join d in context.Deals on inv_d.deal_id equals d.id into _deal
                            from deal in _deal.DefaultIfEmpty()
                            join up in context.UserProfiles on u.created_by equals up.id into g
                            from x in g.DefaultIfEmpty()
                            join up in context.UserProfiles on u.updated_by equals up.id into h
                            from uby in h.DefaultIfEmpty()
                            join supplier in context.DealSuppliers on deal.deal_supplier_id equals supplier.id into _supplier
                            from deal_supplier in _supplier.DefaultIfEmpty()

                            select new
                            {
                                id = u.id,
                                file_id = u.file_id,
                                name = u.name,
                                address = u.address,
                                phone_number = u.phone_number,
                                email = u.email,
                                postcode = u.postcode,
                                status = u.status,
                                inv_no = u.inv_no,
                                total_price = u.total_price,
                                vat7percent = u.vat7percent,
                                total_price_without_tax = u.total_price_without_tax,
                                remark = u.remark,
                                created_at = u.created_at,
                                created_by = u.created_by,
                                created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                updated_at = u.updated_at,
                                updated_by = u.updated_by,
                                updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                tracking_no = u.tracking_no,
                                tracking_date = u.tracking_date,
                                deal_id = deal.id,
                                deal_name = deal.deal_name,
                                item_detail = inv_d.item_detail,
                                quantity = inv_d.quantity,
                                price = inv_d.price,
                                option_selected = inv_d.option_selected,
                                blob_image_invoice = u.blob_image_invoice,
                                note = deal_supplier.description
                            })
                            .AsEnumerable()
                            .OrderBy(x => x.created_at)
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
                                    from u in context.DealUserTaxInvoices.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                    join inv_detail in context.DealInvoiceDetails on u.inv_no equals inv_detail.inv_no into _inv_detail
                                    from inv_d in _inv_detail.DefaultIfEmpty()
                                    join d in context.Deals on inv_d.deal_id equals d.id into _deal
                                    from deal in _deal.DefaultIfEmpty()
                                    join up in context.UserProfiles on u.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on u.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on deal.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()

                                    select new
                                    {
                                        id = u.id,
                                        file_id = u.file_id,
                                        name = u.name,
                                        address = u.address,
                                        phone_number = u.phone_number,
                                        email = u.email,
                                        postcode = u.postcode,
                                        status = u.status,
                                        inv_no = u.inv_no,
                                        total_price = u.total_price,
                                        vat7percent = u.vat7percent,
                                        total_price_without_tax = u.total_price_without_tax,
                                        remark = u.remark,
                                        created_at = u.created_at,
                                        created_by = u.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = u.updated_at,
                                        updated_by = u.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        tracking_no = u.tracking_no,
                                        tracking_date = u.tracking_date,
                                        deal_id = deal.id,
                                        deal_name = deal.deal_name,
                                        item_detail = inv_d.item_detail,
                                        quantity = inv_d.quantity,
                                        price = inv_d.price,
                                        option_selected = inv_d.option_selected,
                                        blob_image_invoice = u.blob_image_invoice,
                                        note = deal_supplier.description
                                    })
                                    .AsEnumerable()
                                    .OrderBy(x => x.created_at)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)
                                    .ToList<dynamic>();




                            count = (
                                    from u in context.DealUserTaxInvoices.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                    join inv_detail in context.DealInvoiceDetails on u.inv_no equals inv_detail.inv_no into _inv_detail
                                    from inv_d in _inv_detail.DefaultIfEmpty()
                                    join d in context.Deals on inv_d.deal_id equals d.id into _deal
                                    from deal in _deal.DefaultIfEmpty()
                                    join up in context.UserProfiles on u.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on u.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()

                                    select new
                                    {
                                        id = u.id,
                                        file_id = u.file_id,
                                        name = u.name,
                                        address = u.address,
                                        phone_number = u.phone_number,
                                        email = u.email,
                                        postcode = u.postcode,
                                        status = u.status,
                                        inv_no = u.inv_no,
                                        total_price = u.total_price,
                                        vat7percent = u.vat7percent,
                                        total_price_without_tax = u.total_price_without_tax,
                                        remark = u.remark,
                                        created_at = u.created_at,
                                        created_by = u.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = u.updated_at,
                                        updated_by = u.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        tracking_no = u.tracking_no,
                                        tracking_date = u.tracking_date,
                                        deal_id = deal.id,
                                        deal_name = deal.deal_name,
                                        item_detail = inv_d.item_detail,
                                        quantity = inv_d.quantity,
                                        price = inv_d.price,
                                        option_selected = inv_d.option_selected,
                                        blob_image_invoice = u.blob_image_invoice
                                    })
                                    .AsEnumerable()
                                    .OrderBy(x => x.created_at)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)
                                //.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                //.Take(validFilter.PageSize)
                                .ToList<dynamic>().Count();
                        }
                        else
                        {
                            data = (
                                    from u in context.DealUserTaxInvoices.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                    join inv_detail in context.DealInvoiceDetails on u.inv_no equals inv_detail.inv_no into _inv_detail
                                    from inv_d in _inv_detail.DefaultIfEmpty()
                                    join d in context.Deals on inv_d.deal_id equals d.id into _deal
                                    from deal in _deal.DefaultIfEmpty()
                                    join up in context.UserProfiles on u.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on u.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()
                                    join supplier in context.DealSuppliers on deal.deal_supplier_id equals supplier.id into _supplier
                                    from deal_supplier in _supplier.DefaultIfEmpty()

                                    select new
                                    {
                                        id = u.id,
                                        file_id = u.file_id,
                                        name = u.name,
                                        address = u.address,
                                        phone_number = u.phone_number,
                                        email = u.email,
                                        postcode = u.postcode,
                                        status = u.status,
                                        inv_no = u.inv_no,
                                        total_price = u.total_price,
                                        vat7percent = u.vat7percent,
                                        total_price_without_tax = u.total_price_without_tax,
                                        remark = u.remark,
                                        created_at = u.created_at,
                                        created_by = u.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = u.updated_at,
                                        updated_by = u.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        tracking_no = u.tracking_no,
                                        tracking_date = u.tracking_date,
                                        deal_id = deal.id,
                                        deal_name = deal.deal_name,
                                        item_detail = inv_d.item_detail,
                                        quantity = inv_d.quantity,
                                        price = inv_d.price,
                                        option_selected = inv_d.option_selected,
                                        blob_image_invoice = u.blob_image_invoice,
                                        note = deal_supplier.description
                                    })
                                    .AsEnumerable()
                                    .Where(x => x.deal_name.Contains(name))
                                    .OrderBy(x => x.created_at)
                                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                    .Take(validFilter.PageSize)
                                    .ToList<dynamic>();


                            count = (
                                    from u in context.DealUserTaxInvoices.Where(x => x.created_at >= date_from && x.created_at < date_to)
                                    join inv_detail in context.DealInvoiceDetails on u.inv_no equals inv_detail.inv_no into _inv_detail
                                    from inv_d in _inv_detail.DefaultIfEmpty()
                                    join d in context.Deals on inv_d.deal_id equals d.id into _deal
                                    from deal in _deal.DefaultIfEmpty()
                                    join up in context.UserProfiles on u.created_by equals up.id into g
                                    from x in g.DefaultIfEmpty()
                                    join up in context.UserProfiles on u.updated_by equals up.id into h
                                    from uby in h.DefaultIfEmpty()

                                    select new
                                    {
                                        id = u.id,
                                        file_id = u.file_id,
                                        name = u.name,
                                        address = u.address,
                                        phone_number = u.phone_number,
                                        email = u.email,
                                        postcode = u.postcode,
                                        status = u.status,
                                        inv_no = u.inv_no,
                                        total_price = u.total_price,
                                        vat7percent = u.vat7percent,
                                        total_price_without_tax = u.total_price_without_tax,
                                        remark = u.remark,
                                        created_at = u.created_at,
                                        created_by = u.created_by,
                                        created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                        updated_at = u.updated_at,
                                        updated_by = u.updated_by,
                                        updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                        tracking_no = u.tracking_no,
                                        tracking_date = u.tracking_date,
                                        deal_id = deal.id,
                                        deal_name = deal.deal_name,
                                        item_detail = inv_d.item_detail,
                                        quantity = inv_d.quantity,
                                        price = inv_d.price,
                                        option_selected = inv_d.option_selected,
                                        blob_image_invoice = u.blob_image_invoice
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
                                from u in context.DealUserTaxInvoices
                                join inv_detail in context.DealInvoiceDetails on u.inv_no equals inv_detail.inv_no into _inv_detail
                                from inv_d in _inv_detail.DefaultIfEmpty()
                                join d in context.Deals on inv_d.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on u.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on u.updated_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()

                                select new
                                {
                                    id = u.id,
                                    file_id = u.file_id,
                                    name = u.name,
                                    address = u.address,
                                    phone_number = u.phone_number,
                                    email = u.email,
                                    postcode = u.postcode,
                                    status = u.status,
                                    inv_no = u.inv_no,
                                    total_price = u.total_price,
                                    vat7percent = u.vat7percent,
                                    total_price_without_tax = u.total_price_without_tax,
                                    remark = u.remark,
                                    created_at = u.created_at,
                                    created_by = u.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    updated_at = u.updated_at,
                                    updated_by = u.updated_by,
                                    updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                    tracking_no = u.tracking_no,
                                    tracking_date = u.tracking_date,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    item_detail = inv_d.item_detail,
                                    quantity = inv_d.quantity,
                                    price = inv_d.price,
                                    option_selected = inv_d.option_selected,
                                    blob_image_invoice = u.blob_image_invoice,
                                    note = deal_supplier.description
                                })
                                .AsEnumerable()
                                .OrderBy(x => x.created_at)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();




                            count = (
                                from u in context.DealUserTaxInvoices
                                join inv_detail in context.DealInvoiceDetails on u.inv_no equals inv_detail.inv_no into _inv_detail
                                from inv_d in _inv_detail.DefaultIfEmpty()
                                join d in context.Deals on inv_d.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on u.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on u.updated_by equals up.id into h
                                from uby in h.DefaultIfEmpty()

                                select new
                                {
                                    id = u.id,
                                    file_id = u.file_id,
                                    name = u.name,
                                    address = u.address,
                                    phone_number = u.phone_number,
                                    email = u.email,
                                    postcode = u.postcode,
                                    status = u.status,
                                    inv_no = u.inv_no,
                                    total_price = u.total_price,
                                    vat7percent = u.vat7percent,
                                    total_price_without_tax = u.total_price_without_tax,
                                    remark = u.remark,
                                    created_at = u.created_at,
                                    created_by = u.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    updated_at = u.updated_at,
                                    updated_by = u.updated_by,
                                    updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                    tracking_no = u.tracking_no,
                                    tracking_date = u.tracking_date,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    item_detail = inv_d.item_detail,
                                    quantity = inv_d.quantity,
                                    price = inv_d.price,
                                    option_selected = inv_d.option_selected,
                                    blob_image_invoice = u.blob_image_invoice
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
                                from u in context.DealUserTaxInvoices
                                join inv_detail in context.DealInvoiceDetails on u.inv_no equals inv_detail.inv_no into _inv_detail
                                from inv_d in _inv_detail.DefaultIfEmpty()
                                join d in context.Deals on inv_d.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on u.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on u.updated_by equals up.id into h
                                from uby in h.DefaultIfEmpty()
                                join supplier in context.DealSuppliers on deal.deal_supplier_id equals supplier.id into _supplier
                                from deal_supplier in _supplier.DefaultIfEmpty()

                                select new
                                {
                                    id = u.id,
                                    file_id = u.file_id,
                                    name = u.name,
                                    address = u.address,
                                    phone_number = u.phone_number,
                                    email = u.email,
                                    postcode = u.postcode,
                                    status = u.status,
                                    inv_no = u.inv_no,
                                    total_price = u.total_price,
                                    vat7percent = u.vat7percent,
                                    total_price_without_tax = u.total_price_without_tax,
                                    remark = u.remark,
                                    created_at = u.created_at,
                                    created_by = u.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    updated_at = u.updated_at,
                                    updated_by = u.updated_by,
                                    updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                    tracking_no = u.tracking_no,
                                    tracking_date = u.tracking_date,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    item_detail = inv_d.item_detail,
                                    quantity = inv_d.quantity,
                                    price = inv_d.price,
                                    option_selected = inv_d.option_selected,
                                    blob_image_invoice = u.blob_image_invoice,
                                    note = deal_supplier.description
                                })
                                .AsEnumerable()
                                .Where(x => x.deal_name.Contains(name))
                                .OrderBy(x => x.created_at)
                                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                .Take(validFilter.PageSize)
                                .ToList<dynamic>();


                            count = (
                                from u in context.DealUserTaxInvoices
                                join inv_detail in context.DealInvoiceDetails on u.inv_no equals inv_detail.inv_no into _inv_detail
                                from inv_d in _inv_detail.DefaultIfEmpty()
                                join d in context.Deals on inv_d.deal_id equals d.id into _deal
                                from deal in _deal.DefaultIfEmpty()
                                join up in context.UserProfiles on u.created_by equals up.id into g
                                from x in g.DefaultIfEmpty()
                                join up in context.UserProfiles on u.updated_by equals up.id into h
                                from uby in h.DefaultIfEmpty()

                                select new
                                {
                                    id = u.id,
                                    file_id = u.file_id,
                                    name = u.name,
                                    address = u.address,
                                    phone_number = u.phone_number,
                                    email = u.email,
                                    postcode = u.postcode,
                                    status = u.status,
                                    inv_no = u.inv_no,
                                    total_price = u.total_price,
                                    vat7percent = u.vat7percent,
                                    total_price_without_tax = u.total_price_without_tax,
                                    remark = u.remark,
                                    created_at = u.created_at,
                                    created_by = u.created_by,
                                    created_by_name = (x == null ? String.Empty : x.first_name.ToString() + " " + x.last_name.ToString()),
                                    updated_at = u.updated_at,
                                    updated_by = u.updated_by,
                                    updated_by_name = (uby == null ? String.Empty : uby.first_name.ToString() + " " + uby.last_name.ToString()),
                                    tracking_no = u.tracking_no,
                                    tracking_date = u.tracking_date,
                                    deal_id = deal.id,
                                    deal_name = deal.deal_name,
                                    item_detail = inv_d.item_detail,
                                    quantity = inv_d.quantity,
                                    price = inv_d.price,
                                    option_selected = inv_d.option_selected,
                                    blob_image_invoice = u.blob_image_invoice
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
        [HttpPost("AddDataFromFileExcel")]
        public async Task<IActionResult> AddDataFromFileExcel([FromForm] FormDealInvoice form)
        {
            string status = "";

            try
            {
                int user = Convert.ToInt32(form.emp_id);
                var file = form.file;
                string remark = form.remark;


                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
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
                                string column_deal_id = dt.Rows[0][0].ToString().ToLower();
                                string column_quantity = dt.Rows[0][1].ToString().ToLower();
                                string column_recipient_name = dt.Rows[0][2].ToString().ToLower();
                                string column_phone = dt.Rows[0][3].ToString().ToLower();
                                string column_email = dt.Rows[0][4].ToString().ToLower();
                                string column_address = dt.Rows[0][5].ToString().ToLower();
                                string column_postcode = dt.Rows[0][6].ToString().ToLower();
                                if (column_deal_id != "deal id")
                                {
                                    text = "ตรวจสอบ รูปแบบคอลัมม์ Deal ID ของไฟล์ Excel";
                                    return UnprocessableEntity(new { message = text });
                                }
                                if (column_quantity != "quantity")
                                {
                                    text = "ตรวจสอบ รูปแบบคอลัมม์ Quantity ของไฟล์ Excel";
                                    return UnprocessableEntity(new { message = text });
                                }
                                if (column_recipient_name != "recipient name")
                                {
                                    text = "ตรวจสอบ รูปแบบคอลัมม์ Recipient Name ของไฟล์ Excel";
                                    return UnprocessableEntity(new { message = text });
                                }
                                if (column_phone != "phone")
                                {
                                    text = "ตรวจสอบ รูปแบบคอลัมม์ Phone ของไฟล์ Excel";
                                    return UnprocessableEntity(new { message = text });
                                }
                                if (column_email != "email")
                                {
                                    text = "ตรวจสอบ รูปแบบคอลัมม์ Email ของไฟล์ Excel";
                                    return UnprocessableEntity(new { message = text });
                                }
                                if (column_address != "address")
                                {
                                    text = "ตรวจสอบ รูปแบบคอลัมม์ Address ของไฟล์ Excel";
                                    return UnprocessableEntity(new { message = text });
                                }
                                if (column_postcode != "postcode")
                                {
                                    text = "ตรวจสอบ รูปแบบคอลัมม์ Postcode ของไฟล์ Excel";
                                    return UnprocessableEntity(new { message = text });
                                }

                                var csb = new MySqlConnectionStringBuilder(ConnectionString.Value.DefaultConnection);
                                string database = csb.Database;
                                int file_id = GetIDAutoIncrementFromDbTable(database, "deal_upload_file");
                                int inv_deal_user_tax_inv = GetIDAutoIncrementFromDbTable(database, "deal_user_tax_inv");
                                //int inv_detail_id = GetIDAutoIncrementFromDbTable(database, "deal_inv_detail");

                                //add file
                                DealUploadFile _data_up_file = new DealUploadFile();
                                _data_up_file.deal_type_of_file = 0;
                                _data_up_file.created_at = DateTime.Now;
                                _data_up_file.created_by = user;
                                string _filename_up_file = "";
                                if (form.file != null)
                                {
                                    _filename_up_file = await UploadFile(form.file);
                                    _data_up_file.deal_file_path = _filename_up_file;
                                }

                                string inv_no = "ABB" + inv_deal_user_tax_inv.ToString("000000");

                                for (int i = 1; i < dt.Rows.Count; i++)
                                {
                                    int deal_id = Convert.ToInt32(dt.Rows[i][0]);
                                    var _deal = (dynamic)null;
                                    _deal = context.Deals.FirstOrDefault(x => x.id == deal_id);

                                    if (_deal == null)
                                    {
                                        return UnprocessableEntity(new { message = "Deal was not found." });
                                    }
                                    if (!CheckIfExcelFile(file))
                                    {
                                        return UnprocessableEntity(new { message = "Invalid file extension" });
                                    }

                                    int quantity = Convert.ToInt32(dt.Rows[i][1]);
                                    decimal? total_price = _deal.deal_coupon_value * quantity;
                                    decimal? vat7percent = Math.Round((decimal)(total_price * 7 / 100), 2);
                                    decimal? total_price_without_tax = total_price - vat7percent;

                                    DealUserTaxInvoice _data_user_tax_inv = new DealUserTaxInvoice();
                                    _data_user_tax_inv.file_id = file_id;
                                    _data_user_tax_inv.name = dt.Rows[i][2].ToString();
                                    _data_user_tax_inv.address = dt.Rows[i][5].ToString();
                                    _data_user_tax_inv.phone_number = dt.Rows[i][3].ToString();
                                    _data_user_tax_inv.email = dt.Rows[i][4].ToString();
                                    _data_user_tax_inv.postcode = dt.Rows[i][6].ToString();
                                    _data_user_tax_inv.status = 0;
                                    _data_user_tax_inv.inv_no = inv_no;
                                    _data_user_tax_inv.total_price = total_price;
                                    _data_user_tax_inv.vat7percent = vat7percent;
                                    _data_user_tax_inv.total_price_without_tax = total_price_without_tax;
                                    _data_user_tax_inv.remark = form.remark;
                                    _data_user_tax_inv.created_by = user;
                                    _data_user_tax_inv.created_at = DateTime.Now;

                                    DealInvoiceDetail _data_inv_detail = new DealInvoiceDetail();
                                    _data_inv_detail.inv_no = inv_no;
                                    _data_inv_detail.deal_id = deal_id;
                                    _data_inv_detail.item_detail = _deal.deal_name;
                                    _data_inv_detail.quantity = quantity;
                                    _data_inv_detail.price = total_price;
                                    _data_inv_detail.option_selected = _deal.deal_name;
                                    _data_inv_detail.remark = form.remark;
                                    _data_inv_detail.created_by = user;
                                    _data_inv_detail.created_at = DateTime.Now;

                                    context.DealUserTaxInvoices.Add(_data_user_tax_inv);
                                    context.DealInvoiceDetails.Add(_data_inv_detail);
                                }

                                DealLog _data_log = new DealLog();
                                _data_log.user = user;
                                _data_log.type = 0;
                                //_data_log.deal_id = deal_id;
                                _data_log.remark = "tax invoice";
                                _data_log.link_file = _filename_up_file;
                                _data_log.created_at = DateTime.Now;

                                context.DealLogs.Add(_data_log);

                                context.SaveChanges();
                                status = "success";

                            }


                        }
                    }

                }


            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { status = status, response = true });

        }
        public class FormDealInvoice
        {
            public int emp_id { get; set; }
            public string remark { get; set; }
            public IFormFile file { get; set; }
        }

        [Authorize]
        [HttpPost("UpdateTrackingFromFileExcel")]
        public async Task<IActionResult> UpdateTrackingFromFileExcel([FromForm] FormInvoiceTracking form)
        {
            string status = "";

            try
            {
                int user = Convert.ToInt32(form.emp_id);
                var file = form.file;
                string remark = form.remark;


                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
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
                                string column_id = dt.Rows[0][0].ToString().ToLower();
                                string column_tracking_no = dt.Rows[0][1].ToString().ToLower();
                                string column_tracking_date = dt.Rows[0][2].ToString().ToLower();
                                if (column_id != "id")
                                {
                                    text = "ตรวจสอบ รูปแบบคอลัมม์ ID ของไฟล์ Excel";
                                    return UnprocessableEntity(new { message = text });
                                }
                                if (column_tracking_no != "tracking no.")
                                {
                                    text = "ตรวจสอบ รูปแบบคอลัมม์ Tracking No. ของไฟล์ Excel";
                                    return UnprocessableEntity(new { message = text });
                                }
                                if (column_tracking_date != "tracking date")
                                {
                                    text = "ตรวจสอบ รูปแบบคอลัมม์ Tracking Date ของไฟล์ Excel";
                                    return UnprocessableEntity(new { message = text });
                                }

                                var csb = new MySqlConnectionStringBuilder(ConnectionString.Value.DefaultConnection);
                                string database = csb.Database;
                                int file_id = GetIDAutoIncrementFromDbTable(database, "deal_upload_file");

                                //add file
                                DealUploadFile _data_up_file = new DealUploadFile();
                                _data_up_file.deal_type_of_file = 0;
                                _data_up_file.created_at = DateTime.Now;
                                _data_up_file.created_by = user;
                                string _filename_up_file = "";
                                if (form.file != null)
                                {
                                    _filename_up_file = await UploadFile(form.file);
                                    _data_up_file.deal_file_path = _filename_up_file;
                                }

                                for (int i = 1; i < dt.Rows.Count; i++)
                                {
                                    string _id = dt.Rows[i][0].ToString();
                                    string tracking_no = dt.Rows[i][1].ToString();
                                    string _tracking_date = dt.Rows[i][2].ToString();

                                    if (_id == "")
                                    {
                                        text = "ตรวจสอบ ข้อมูล คอลัมม์ ID, แถว " + i + " ของไฟล์ Excel";
                                        return UnprocessableEntity(new { message = text });
                                    }
                                    if (tracking_no == "")
                                    {
                                        text = "ตรวจสอบ ข้อมูล คอลัมม์ Tracking No., แถว " + i + " ของไฟล์ Excel";
                                        return UnprocessableEntity(new { message = text });
                                    }
                                    if (_tracking_date == "")
                                    {
                                        text = "ตรวจสอบ ข้อมูล คอลัมม์ Tracking Date, แถว " + i + " ของไฟล์ Excel";
                                        return UnprocessableEntity(new { message = text });
                                    }

                                    int id = Convert.ToInt32(_id);

                                    System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                                    DateTime? tracking_date = null;
                                    if (_tracking_date != "")
                                    {
                                        try
                                        {
                                            string[] _date = _tracking_date.Split("-");
                                            int year = Convert.ToInt32(_date[0]);
                                            int month = Convert.ToInt32(_date[1]);
                                            int day = Convert.ToInt32(_date[2]);
                                            tracking_date = new DateTime(year, month, day);
                                            //tracking_date = DateTime.ParseExact(tracking_date.Value.ToString("dd.MM.yyyy HH:mm:ss").Replace("-", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                                        }
                                        catch (Exception ex)
                                        {
                                            text = "ตรวจสอบ ข้อมูล คอลัมม์ Tracking Date, แถว " + i + " ของไฟล์ Excel " + ex.Message;
                                        }
                                    }

                                    var _data_user_tax_inv = (dynamic)null;
                                    _data_user_tax_inv = context.DealUserTaxInvoices.FirstOrDefault(x => x.id == id);

                                    if (_data_user_tax_inv == null)
                                    {
                                        return UnprocessableEntity(new { message = "Deal was not found." });
                                    }
                                    if (!CheckIfExcelFile(file))
                                    {
                                        return UnprocessableEntity(new { message = "Invalid file extension" });
                                    }

                                    //_data_user_tax_inv.remark = form.remark;
                                    _data_user_tax_inv.tracking_no = tracking_no;
                                    _data_user_tax_inv.tracking_date = tracking_date;
                                    _data_user_tax_inv.updated_by = user;
                                    _data_user_tax_inv.updated_at = DateTime.Now;
                                }

                                DealLog _data_log = new DealLog();
                                _data_log.user = user;
                                _data_log.type = 0;
                                //_data_log.deal_id = deal_id;
                                _data_log.remark = "tax invoice tracking";
                                _data_log.link_file = _filename_up_file;
                                _data_log.created_at = DateTime.Now;

                                context.DealLogs.Add(_data_log);

                                context.SaveChanges();
                                status = "success";

                            }


                        }
                    }

                }


            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.ToString() });
            }

            return Ok(new { status = status, response = true });

        }
        public class FormInvoiceTracking
        {
            public int emp_id { get; set; }
            public string remark { get; set; }
            public IFormFile file { get; set; }
        }


        [Authorize]
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromForm] FormTaxInvoice data)
        {
            string result = "";
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    var _deal = (dynamic)null;
                    _deal = context.Deals.FirstOrDefault(x => x.id == data.deal_id);

                    if (_deal == null)
                    {
                        return UnprocessableEntity(new { message = "Deal was not found." });
                    }

                    decimal? total_price = _deal.deal_coupon_value * data.quantity;
                    decimal? vat7percent = Math.Round((decimal)(total_price * 7 / 100), 2);
                    decimal? total_price_without_tax = total_price - vat7percent;

                    var csb = new MySqlConnectionStringBuilder(ConnectionString.Value.DefaultConnection);
                    string database = csb.Database;
                    int inv_deal_user_tax_inv = GetIDAutoIncrementFromDbTable(database, "deal_user_tax_inv");
                    string inv_no = "ABB" + inv_deal_user_tax_inv.ToString("000000");

                    DealUserTaxInvoice _data_user_tax_inv = new DealUserTaxInvoice();
                    _data_user_tax_inv.name = data.name;
                    _data_user_tax_inv.address = data.address;
                    _data_user_tax_inv.phone_number = data.phone_number;
                    _data_user_tax_inv.email = data.email;
                    _data_user_tax_inv.postcode = data.postcode;
                    _data_user_tax_inv.status = 0;
                    _data_user_tax_inv.inv_no = inv_no;
                    _data_user_tax_inv.total_price = total_price;
                    _data_user_tax_inv.vat7percent = vat7percent;
                    _data_user_tax_inv.total_price_without_tax = total_price_without_tax;
                    _data_user_tax_inv.remark = data.remark;
                    _data_user_tax_inv.created_by = data.created_by;
                    _data_user_tax_inv.created_at = DateTime.Now;

                    var file = data.file_tax_inv;
                    if (file != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            _data_user_tax_inv.blob_image_invoice = fileBytes;
                        }
                    }
                    else
                    {
                        _data_user_tax_inv.blob_image_invoice = null;
                    }

                    DealInvoiceDetail _data_inv_detail = new DealInvoiceDetail();
                    _data_inv_detail.inv_no = inv_no;
                    _data_inv_detail.deal_id = data.deal_id;
                    _data_inv_detail.item_detail = _deal.deal_name;
                    _data_inv_detail.quantity = data.quantity;
                    _data_inv_detail.price = total_price;
                    _data_inv_detail.option_selected = _deal.deal_name;
                    _data_inv_detail.remark = data.remark;
                    _data_inv_detail.created_by = data.created_by;
                    _data_inv_detail.created_at = DateTime.Now;


                    DealLog _data_log = new DealLog();
                    _data_log.user = data.created_by.Value;
                    _data_log.type = 0;
                    //_data_log.deal_id = deal_id;
                    _data_log.remark = "tax invoice";
                    //_data_log.link_file = _filename_up_file;
                    _data_log.created_at = DateTime.Now;

                    context.DealLogs.Add(_data_log);

                    context.DealUserTaxInvoices.Add(_data_user_tax_inv);
                    context.DealInvoiceDetails.Add(_data_inv_detail);

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
        public class FormTaxInvoice
        {
            public int? id { get; set; }
            public int? deal_id { get; set; }
            public int? quantity { get; set; }
            public string name { get; set; }
            public string address { get; set; }
            public string phone_number { get; set; }
            public string email { get; set; }
            public string postcode { get; set; }
            public int? status { get; set; }
            public string remark { get; set; }
            public string note { get; set; }
            public int? created_by { get; set; }
            public DateTime? created_at { get; set; }
            public int? updated_by { get; set; }
            public DateTime? updated_at { get; set; }
            public string tracking_no { get; set; }
            public DateTime? tracking_date { get; set; }
            public IFormFile file_tax_inv { get; set; }
        }

        //[HttpPost("UploadFile")]
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
        [HttpPost("Update")]
        public IActionResult Update([FromForm] FormTaxInvoice data)
        {
            string result = "";
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {
                    if (data.file_tax_inv != null)
                    {
                        var _data_user_tax_inv = context.DealUserTaxInvoices.FirstOrDefault((c) => c.id == data.id);
                        if (_data_user_tax_inv == null)
                        {
                            return UnprocessableEntity(new { message = "Tax Invoice not found" });
                        }

                        _data_user_tax_inv.status = 1;

                        var file = data.file_tax_inv;
                        if (file != null)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                var fileBytes = ms.ToArray();
                                _data_user_tax_inv.blob_image_invoice = fileBytes;
                            }
                        }
                        else
                        {
                            _data_user_tax_inv.blob_image_invoice = null;
                        }
                    }
                    else
                    {
                        var _deal = (dynamic)null;
                        _deal = context.Deals.FirstOrDefault(x => x.id == data.deal_id);

                        if (_deal == null)
                        {
                            return UnprocessableEntity(new { message = "Deal was not found." });
                        }
                        decimal? total_price = _deal.deal_coupon_value * data.quantity;
                        decimal? vat7percent = Math.Round((decimal)(total_price * 7 / 100), 2);
                        decimal? total_price_without_tax = total_price - vat7percent;


                        var _data_user_tax_inv = context.DealUserTaxInvoices.FirstOrDefault((c) => c.id == data.id);
                        if (_data_user_tax_inv == null)
                        {
                            return UnprocessableEntity(new { message = "Tax Invoice not found" });
                        }
                        _data_user_tax_inv.name = data.name;
                        _data_user_tax_inv.address = data.address;
                        _data_user_tax_inv.phone_number = data.phone_number;
                        _data_user_tax_inv.email = data.email;
                        _data_user_tax_inv.postcode = data.postcode;
                        //_data_user_tax_inv.status = 0;
                        //_data_user_tax_inv.inv_no = inv_no;
                        _data_user_tax_inv.total_price = total_price;
                        _data_user_tax_inv.vat7percent = vat7percent;
                        _data_user_tax_inv.total_price_without_tax = total_price_without_tax;
                        _data_user_tax_inv.remark = data.remark;
                        _data_user_tax_inv.updated_by = data.updated_by;
                        _data_user_tax_inv.updated_at = DateTime.Now;
                        _data_user_tax_inv.tracking_no = data.tracking_no;

                        if (data.tracking_date != null)
                        {
                            System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
                            DateTime tracking_date = DateTime.ParseExact(Convert.ToDateTime(data.tracking_date).ToString("dd.MM.yyyy HH:mm:ss").Replace("/", "."), "dd.MM.yyyy HH:mm:ss", cultureinfo);
                            _data_user_tax_inv.tracking_date = tracking_date;
                        }

                        var file = data.file_tax_inv;
                        if (file != null)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                var fileBytes = ms.ToArray();
                                _data_user_tax_inv.blob_image_invoice = fileBytes;
                            }
                        }


                        var _data_inv_detail = context.DealInvoiceDetails.FirstOrDefault((c) => c.inv_no == _data_user_tax_inv.inv_no);
                        if (_data_user_tax_inv == null)
                        {
                            return UnprocessableEntity(new { message = "Tax Invoice not found" });
                        }
                        //_data_inv_detail.inv_no = inv_no;
                        _data_inv_detail.deal_id = data.deal_id;
                        _data_inv_detail.item_detail = _deal.deal_name;
                        _data_inv_detail.quantity = data.quantity;
                        _data_inv_detail.price = total_price;
                        _data_inv_detail.option_selected = _deal.deal_name;
                        _data_inv_detail.remark = data.remark;
                        _data_inv_detail.updated_by = data.updated_by;
                        _data_inv_detail.updated_at = DateTime.Now;
                    }


                    DealLog _data_log = new DealLog();
                    _data_log.user = data.updated_by.Value;
                    _data_log.type = 1;
                    //_data_log.deal_id = deal_id;
                    _data_log.remark = "update tax invoice id " + data.id.ToString();
                    //_data_log.link_file = _filename_up_file;
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
        [HttpGet("DownloadExampleFileUploadCode")]
        public async Task<IActionResult> DownloadExampleFileUploadCode()
        {
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {

                    var stream = new MemoryStream();
                    stream.Position = 0;

                    string path = Path.Combine("/app/download/ExampleFileUploadTaxInvoice.xlsx"); //server
                    //string path = $"{Directory.GetCurrentDirectory()}{@"\download\ExampleFileUploadCode.xlsx"}"; //local

                    await using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        fs.CopyTo(stream);

                        string excelName = $"Example-File-Tax-Invoice.xlsx";
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
        [HttpGet("DownloadExampleFileUploadTracking")]
        public async Task<IActionResult> DownloadExampleFileUploadTracking()
        {
            try
            {
                using (var context = new DealManagementDbContext(ConnectionString.Value.DefaultConnection))
                {

                    var stream = new MemoryStream();
                    stream.Position = 0;

                    string path = Path.Combine("/app/download/ExampleFileUploadTracking.xlsx"); //server
                    //string path = $"{Directory.GetCurrentDirectory()}{@"\download\ExampleFileUploadCode.xlsx"}"; //local

                    await using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        fs.CopyTo(stream);

                        string excelName = $"Example-File-Upload-Tracking.xlsx";
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
