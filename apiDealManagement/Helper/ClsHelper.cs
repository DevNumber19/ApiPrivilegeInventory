using apiDealManagement.Models;
using Ionic.Zip;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace apiDealManagement.Helper
{
    public class ClsHelper
    {
        //private readonly IOptions<ConnectionStringConfig> ConnectionString;
        //private readonly IOptions<EmailAdmin> EmailAdmins;
        //public ClsHelper(IOptions<ConnectionStringConfig> ConnectionString, IOptions<EmailAdmin> EmailAdmins)
        //{
        //    this.ConnectionString = ConnectionString;
        //    this.EmailAdmins = EmailAdmins;
        //}

        public int CompareDate(DateTime? start_date, DateTime? end_date)
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

        public DataTable ListToDataTable<T>(List<T> items)
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

        public void SendEmailToAdmin(string title, string body, IOptions<EmailAdmin> EmailAdmins)
        {
            try
            {
                //send mail
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("xxxxxx-noreply@xxxx.com", "Privilege Management");
                foreach (var address in EmailAdmins.Value.Emails.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (address != "")
                    {
                        mail.To.Add(new MailAddress(address));
                    }
                }

                mail.Subject = title;

                mail.IsBodyHtml = true;

                string path = Path.Combine("/app/images/header-feyverly.png"); //server
                //string path = $"{Directory.GetCurrentDirectory()}{@"\images\header-feyverly.png"}"; //local

                LinkedResource headerImage = new LinkedResource(path, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                headerImage.ContentId = "companyLogo";
                headerImage.ContentType = new ContentType("image/png");

                //MemoryStream ms = new MemoryStream(invoice);
                //LinkedResource invImage = new LinkedResource(ms);
                //invImage.ContentId = "invImage";
                //invImage.ContentType = new ContentType("image/png");

                                //<span style='font-size:10.0pt;color:black'><br></span><span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>
                                //    Chief Legal Officer&nbsp;<br>Feyverly Co., Ltd.
                                //</span>

                 body += @"<br /><br /><br /> Best regards,<br />";
                body += @"<br />
                    <table border='0' cellspacing='0' cellpadding='0' width='403' style='width:302.25pt;border-collapse:collapse'>
                       <tbody>
                          <tr>
                             <td width='403' colspan='2' valign='top' style='width:302.25pt;border:none;border-top:solid #f87b00 1.0pt;padding:7.5pt 0cm 7.5pt 0cm'>
                                <p><b><span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>Feyverly Co., Ltd.</span></b>
                                <u></u><u></u></p>
                             </td>
                          </tr>
                          <tr>
                             <td colspan='2' valign='top' style='padding:7.5pt 0cm 7.5pt 0cm'>
                                <p><span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>Thai CC Tower, 270 27<sup>th</sup>,30<sup>th</sup> Floor, 43 South Sathorn Road<br>Yannawa, Sathorn, Bangkok Thailand 10120<br></span>
                                <a href='http://www.feyverly.com/' target='_blank' data-saferedirecturl='https://www.google.com/url?q=http://www.feyverly.com/&amp;source=gmail&amp;ust=1669360469877000&amp;usg=AOvVaw0gENO0I961yluALCkVYlk1'>
                                <span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>www.feyverly.com</span></a><u></u><u></u></p>
                             </td>
                          </tr>
                          <tr>
                             <td valign='top' style='padding:7.5pt 0cm 7.5pt 0cm'>
                                <p><b><span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>m:</span></b>
                                <span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>
                                &nbsp;+66 (0)91 910 5555<br>
                                <b>t:</b>&nbsp;+66 (0)2 673 9335<br><b>e:</b>&nbsp;
                                </span><a href='mailto:Privilege-Team@feyverly.com' target='_blank'>
                                <span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif'>Privilege-Team@feyverly.com</span></a><u></u><u></u></p>
                                <p>
                                <span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>&nbsp;</span>
                                <img border='0' width='473' height='230' style='width:4.927in;height:2.3958in' src=cid:companyLogo data-image-whitelisted='' data-bit='iit' tabindex='0'>
                                <u></u><u></u></p>
                             </td>
                          </tr>
                          <tr>
                             <td colspan='2' valign='top' style='border:none;border-top:solid #f87b00 1.0pt;padding:3.75pt 0cm 0cm 0cm'></td>
                          </tr>
                       </tbody>
                    </table>
                ";
                //body += @"<br /><img src=cid:companyLogo alt='' /><br />";

                AlternateView av = AlternateView.CreateAlternateViewFromString(body, null, System.Net.Mime.MediaTypeNames.Text.Html);
                av.LinkedResources.Add(headerImage);
                //av.LinkedResources.Add(invImage);

                mail.AlternateViews.Add(av);

                ////string path = Path.Combine("/app/images/header_800.png");
                //string path = $"{Directory.GetCurrentDirectory()}{@"\images\header_800.png"}"; //local

                //LinkedResource headerImage = new LinkedResource(path, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                //headerImage.ContentId = "companyLogo";
                //headerImage.ContentType = new ContentType("image/png");

                //body += @"<br /><br /><br /> Best regards,<br />
                //            Feyverly Co., Ltd.<br /><br />

                //            Thai CC Tower, 270 27th,30th Floor, 43 South Sathorn Road<br />
                //            Yannawa, Sathorn, Bangkok Thailand 10120<br />
                //            www.feyverly.com<br /><br />

                //            m: +66 (0)91 910 5555<br />
                //            t: +66 (0)2 673 9335<br />
                //            e: Teeyarak@feyverly.com";

                //AlternateView av = AlternateView.CreateAlternateViewFromString(body, null, System.Net.Mime.MediaTypeNames.Text.Html);
                //av.LinkedResources.Add(headerImage);

                //mail.AlternateViews.Add(av);

                ////mail.CC.Add(new MailAddress("Teeyarak@feyverly.com", "Teeyarak"));
                ////mail.CC.Add(new MailAddress("vorawee@feyverly.com", "P'Vorawee"));

                //SmtpClient client = new SmtpClient();
                string HOST = "email-smtp.ap-southeast-1.amazonaws.com";
                int PORT = 587;

                using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
                {
                    // Pass SMTP credentials
                    client.Credentials = new NetworkCredential("xxxxxxx", "xxxxxxxxxxxxxxxxxxxxxxxxxx");
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

        public void SendEmailToCustomer(string title, string body, string email, string customer_name, bool flag_attachment_file, string path_file, string password, IOptions<EmailAdmin> EmailAdmins)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("xxxx-noreply@xxx.com", "Privilege Management");
                mail.To.Add(new MailAddress(email, customer_name));

                mail.Subject = title;

                mail.IsBodyHtml = true;

                if (flag_attachment_file)
                {
                    //string password = RandomString(10);
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



                string path = Path.Combine("/app/images/header-feyverly.png"); //server
                //string path = $"{Directory.GetCurrentDirectory()}{@"\images\header-feyverly.png"}"; //local

                LinkedResource headerImage = new LinkedResource(path, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                headerImage.ContentId = "companyLogo";
                headerImage.ContentType = new ContentType("image/png");

                //MemoryStream ms = new MemoryStream(invoice);
                //LinkedResource invImage = new LinkedResource(ms);
                //invImage.ContentId = "invImage";
                //invImage.ContentType = new ContentType("image/png");

                //body += "<img width='800' height='auto' src=cid:invImage >";


                body += @"<br /><br /><br /> Best regards,<br />";
                body += @"<br />
                    <table border='0' cellspacing='0' cellpadding='0' width='403' style='width:302.25pt;border-collapse:collapse'>
                       <tbody>
                          <tr>
                             <td width='403' colspan='2' valign='top' style='width:302.25pt;border:none;border-top:solid #f87b00 1.0pt;padding:7.5pt 0cm 7.5pt 0cm'>
                                <p><b><span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>Feyverly Co., Ltd.</span></b>
                                <u></u><u></u></p>
                             </td>
                          </tr>
                          <tr>
                             <td colspan='2' valign='top' style='padding:7.5pt 0cm 7.5pt 0cm'>
                                <p><span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>Thai CC Tower, 270 27<sup>th</sup>,30<sup>th</sup> Floor, 43 South Sathorn Road<br>Yannawa, Sathorn, Bangkok Thailand 10120<br></span>
                                <a href='http://www.feyverly.com/' target='_blank' data-saferedirecturl='https://www.google.com/url?q=http://www.feyverly.com/&amp;source=gmail&amp;ust=1669360469877000&amp;usg=AOvVaw0gENO0I961yluALCkVYlk1'>
                                <span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>www.feyverly.com</span></a><u></u><u></u></p>
                             </td>
                          </tr>
                          <tr>
                             <td valign='top' style='padding:7.5pt 0cm 7.5pt 0cm'>
                                <p><b><span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>m:</span></b>
                                <span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>
                                &nbsp;+66 (0)91 910 5555<br>
                                <b>t:</b>&nbsp;+66 (0)2 673 9335<br><b>e:</b>&nbsp;
                                </span><a href='mailto:Privilege-Team@feyverly.com' target='_blank'>
                                <span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif'>Privilege-Team@feyverly.com</span></a><u></u><u></u></p>
                                <p>
                                <span style='font-size:10.0pt;font-family:&quot;Verdana&quot;,sans-serif;color:black'>&nbsp;</span>
                                <img border='0' width='473' height='230' style='width:4.927in;height:2.3958in' src=cid:companyLogo data-image-whitelisted='' data-bit='iit' tabindex='0'>
                                <u></u><u></u></p>
                             </td>
                          </tr>
                          <tr>
                             <td colspan='2' valign='top' style='border:none;border-top:solid #f87b00 1.0pt;padding:3.75pt 0cm 0cm 0cm'></td>
                          </tr>
                       </tbody>
                    </table>
                ";
                //body += @"<br /><img src=cid:companyLogo alt='' /><br />";

                AlternateView av = AlternateView.CreateAlternateViewFromString(body, null, System.Net.Mime.MediaTypeNames.Text.Html);
                av.LinkedResources.Add(headerImage);
                //av.LinkedResources.Add(invImage);

                mail.AlternateViews.Add(av);


                //string path = $"{Directory.GetCurrentDirectory()}{@"\app\images\header_800.png"}";
                ////string path = $"{Directory.GetCurrentDirectory()}{@"\images\header_800.png"}"; //local

                //LinkedResource headerImage = new LinkedResource(path, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                //headerImage.ContentId = "companyLogo";
                //headerImage.ContentType = new ContentType("image/png");

                ////string body = @"<img src='" + path + "' alt=\"\" />" + html +
                ////string body = @"<img src=cid:companyLogo alt='' /><br />" +
                ////            "<br /> Hi (" + customer_name + "), <br />" +
                ////            "<br /> Your requested code was rejected because " + data.remark.Replace("&#13;&#10;", "<br />");

                //body += @"<br /><br /><br /> Best regards,<br />
                //            Feyverly Co., Ltd.<br /><br />

                //            Thai CC Tower, 270 27th,30th Floor, 43 South Sathorn Road<br />
                //            Yannawa, Sathorn, Bangkok Thailand 10120<br />
                //            www.feyverly.com<br /><br />

                //            m: +66 (0)91 910 5555<br />
                //            t: +66 (0)2 673 9335<br />
                //            e: Teeyarak@feyverly.com";

                //AlternateView av = AlternateView.CreateAlternateViewFromString(body, null, System.Net.Mime.MediaTypeNames.Text.Html);
                //av.LinkedResources.Add(headerImage);

                //mail.AlternateViews.Add(av);
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
                    client.Credentials = new NetworkCredential("xxxxxxxxxxxx", "xxxxxxxxxxxxxxxxxxxxxxxx");
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

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
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
