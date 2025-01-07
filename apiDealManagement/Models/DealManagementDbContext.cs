using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace apiDealManagement.Models
{
    public partial class DealManagementDbContext : DbContext
    {
        string _connectionString = "";
        public DealManagementDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public DealManagementDbContext(DbContextOptions<DealManagementDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<UserProfile> UserProfiles { get; set; }
        //public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Deal> Deals { get; set; }
        public virtual DbSet<DealCode> DealCodes { get; set; }
        public virtual DbSet<DealCustomer> DealCustomers { get; set; }
        public virtual DbSet<DealSupplier> DealSuppliers { get; set; }
        public virtual DbSet<DealLog> DealLogs { get; set; }
        public virtual DbSet<DealRequested> DealRequesteds { get; set; }
        public virtual DbSet<DealUser> DealUsers { get; set; }
        public virtual DbSet<DealOrderCode> DealOrderCodes { get; set; }
        public virtual DbSet<ReportTemplate> ReportTemplates { get; set; }
        public virtual DbSet<DealReconcile> DealReconciles { get; set; }
        public virtual DbSet<InvoiceProduct> InvoiceProducts { get; set; }
        public virtual DbSet<InvoiceOrderItem> InvoiceOrderItems { get; set; }
        public virtual DbSet<InvoicePrivilge> InvoicePrivilges { get; set; }
        public virtual DbSet<InvoiceOrder> InvoiceOrders { get; set; }
        public virtual DbSet<DealQuatation> DealQuatations { get; set; }
        public virtual DbSet<DealUploadFile> DealUploadFiles { get; set; }
        public virtual DbSet<DealUserTaxInvoice> DealUserTaxInvoices { get; set; }
        public virtual DbSet<DealInvoiceDetail> DealInvoiceDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySQL(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Thai_CI_AI");

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("std_employee")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");

                entity.Property(e => e.email)
                    .HasColumnType("text")
                    .HasColumnName("email");

                entity.Property(e => e.emp_id)
                    .HasColumnType("int")
                    .HasColumnName("id");

                entity.Property(e => e.finger_print_id)
                    .HasColumnType("text")
                    .HasColumnName("finger_print_id");

                entity.Property(e => e.username)
                    .HasColumnType("text")
                    .HasColumnName("fname");

                entity.Property(e => e.first_name)
                    .HasColumnType("text")
                    .HasColumnName("fname");

                entity.Property(e => e.last_name)
                    .HasColumnType("text")
                    .HasColumnName("lname");

                entity.Property(e => e.nick_name)
                    .HasColumnType("text")
                    .HasColumnName("nickname");

                entity.Property(e => e.phone_number)
                    .HasColumnType("text")
                    .HasColumnName("tel");

                entity.Property(e => e.department)
                    .HasColumnType("text")
                    .HasColumnName("edepartment");

                entity.Property(e => e.position)
                    .HasColumnType("text")
                    .HasColumnName("eposition");

                entity.Property(e => e.user_office)
                    .HasColumnType("text")
                    .HasColumnName("user_office");

                entity.Property(e => e.is_admin)
                    //.HasColumnType("int")
                    .HasColumnName("is_Admin");

                entity.Property(e => e.created_at)
                    .HasColumnType("datetime")
                    .HasColumnName("created_date");

                entity.Property(e => e.created_by)
                    .HasColumnName("created_by");

                entity.Property(e => e.updated_at)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_date");

                entity.Property(e => e.updated_by)
                    .HasColumnName("updated_by");

                entity.Property(e => e.blob_image)
                    .HasColumnName("eimage");

                entity.Property(e => e.token)
                    //.HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("token");

                entity.Property(e => e.enabled)
                    //.HasColumnType("int")
                    .HasColumnName("enabled");

                entity.Property(e => e.account)
                    .HasColumnType("text")
                    .HasColumnName("account");

                entity.Property(e => e.initial_name)
                    .HasColumnType("text")
                    .HasColumnName("initial_name");

            });

            modelBuilder.Entity<Deal>(entity =>
            {
                entity.ToTable("deal")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.deal_supplier_id).HasColumnName("deal_supplier_id");
                entity.Property(e => e.deal_customer_id).HasColumnName("deal_customer_id");
                entity.Property(e => e.deal_name).HasColumnName("deal_name");
                entity.Property(e => e.remark).HasColumnName("remark");
                entity.Property(e => e.status).HasColumnName("status");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
                entity.Property(e => e.deal_coupon_type).HasColumnName("deal_coupon_type");
                entity.Property(e => e.deal_coupon_value).HasColumnName("deal_coupon_value");
                entity.Property(e => e.deal_permission).HasColumnName("deal_permission");
                entity.Property(e => e.deal_quatation).HasColumnName("deal_quatation");
                entity.Property(e => e.deal_merchant_id).HasColumnName("deal_merchant_id");
                entity.Property(e => e.deal_merchant_name).HasColumnName("deal_merchant_name");
                entity.Property(e => e.deal_invoice).HasColumnName("deal_invoice");
                entity.Property(e => e.deal_receipt).HasColumnName("deal_receipt");
                entity.Property(e => e.deal_start_date).HasColumnName("deal_start_date");
                entity.Property(e => e.deal_end_date).HasColumnName("deal_end_date");
                entity.Property(e => e.deal_coupon_name).HasColumnName("deal_coupon_name");
                entity.Property(e => e.deal_cost).HasColumnName("deal_cost");
                entity.Property(e => e.deal_condition).HasColumnName("deal_condition");
                entity.Property(e => e.deal_coupon_image).HasColumnName("deal_coupon_image");
                entity.Property(e => e.deal_major).HasColumnName("deal_major");
                entity.Property(e => e.deal_quatation_link).HasColumnName("deal_quatation_link");
                entity.Property(e => e.deal_discount_value).HasColumnName("deal_discount_value");
                entity.Property(e => e.deal_summary_value).HasColumnName("deal_summary_value");
                entity.Property(e => e.deal_add_item).HasColumnName("deal_add_item");
                entity.Property(e => e.deal_add_amount).HasColumnName("deal_add_amount");
                entity.Property(e => e.deal_add_value).HasColumnName("deal_add_value");
                entity.Property(e => e.deal_pr_number).HasColumnName("deal_pr_number");
                entity.Property(e => e.deal_pr_file).HasColumnName("deal_pr_file");
                entity.Property(e => e.minimum_requested).HasColumnName("minimum_requested");
                entity.Property(e => e.maximum_requested).HasColumnName("maximum_requested");
                entity.Property(e => e.minimum_order).HasColumnName("minimum_order");
                entity.Property(e => e.maximum_order).HasColumnName("maximum_order");
                entity.Property(e => e.deal_quatation_id).HasColumnName("deal_quatation_id");
                entity.Property(e => e.deal_receipt_id).HasColumnName("deal_receipt_id");
                entity.Property(e => e.deal_coupon_image_id).HasColumnName("deal_coupon_image_id");
                entity.Property(e => e.deal_pr_file_id).HasColumnName("deal_pr_file_id");
                entity.Property(e => e.deal_invoice_file_id).HasColumnName("deal_invoice_file_id");
            });

            modelBuilder.Entity<DealCode>(entity =>
            {
                entity.ToTable("deal_code")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.deal_supplier_id).HasColumnName("deal_supplier_id");
                entity.Property(e => e.deal_customer_id).HasColumnName("deal_customer_id");
                entity.Property(e => e.deal_id).HasColumnName("deal_id");
                entity.Property(e => e.deal_quatation).HasColumnName("deal_quatation");
                entity.Property(e => e.deal_quatation_file).HasColumnName("deal_quatation_file");
                entity.Property(e => e.deal_quatation_link).HasColumnName("deal_quatation_link");
                entity.Property(e => e.deal_coupon_code).HasColumnName("deal_coupon_code");
                entity.Property(e => e.deal_reference).HasColumnName("deal_reference");
                entity.Property(e => e.remark).HasColumnName("remark");
                entity.Property(e => e.deal_start_date).HasColumnName("deal_start_date");
                entity.Property(e => e.deal_end_date).HasColumnName("deal_end_date");
                entity.Property(e => e.requested_at).HasColumnName("requested_at");
                entity.Property(e => e.requested_by).HasColumnName("requested_by");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
                entity.Property(e => e.deal_quatation_id).HasColumnName("deal_quatation_id");

            });

            modelBuilder.Entity<DealCustomer>(entity =>
            {
                entity.ToTable("deal_customer")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.name).HasColumnName("name");
                entity.Property(e => e.description).HasColumnName("description");
                entity.Property(e => e.link).HasColumnName("link");
                entity.Property(e => e.doc).HasColumnName("doc");
                entity.Property(e => e.doc_id).HasColumnName("doc_id");
                entity.Property(e => e.remark).HasColumnName("remark");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
                entity.Property(e => e.deleted_at).HasColumnName("deleted_at");
                entity.Property(e => e.status).HasColumnName("status");
                entity.Property(e => e.contact_name).HasColumnName("contact_name");
                entity.Property(e => e.contact_phone).HasColumnName("contact_phone");
                entity.Property(e => e.contact_email).HasColumnName("contact_email");

            });

            modelBuilder.Entity<DealSupplier>(entity =>
            {
                entity.ToTable("deal_supplier")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.name).HasColumnName("name");
                entity.Property(e => e.description).HasColumnName("description");
                entity.Property(e => e.link).HasColumnName("link");
                entity.Property(e => e.major).HasColumnName("major");
                entity.Property(e => e.doc).HasColumnName("doc");
                entity.Property(e => e.file_vat_20).HasColumnName("file_vat_20");
                entity.Property(e => e.file_certificate).HasColumnName("file_certificate");
                entity.Property(e => e.file_bookbank).HasColumnName("file_bookbank");
                entity.Property(e => e.file_accept).HasColumnName("file_accept");
                entity.Property(e => e.remark).HasColumnName("remark");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
                entity.Property(e => e.deleted_at).HasColumnName("deleted_at");
                entity.Property(e => e.status).HasColumnName("status");
                entity.Property(e => e.contact_name).HasColumnName("contact_name");
                entity.Property(e => e.contact_phone).HasColumnName("contact_phone");
                entity.Property(e => e.contact_email).HasColumnName("contact_email");
                entity.Property(e => e.type).HasColumnName("type");
                entity.Property(e => e.amount_major).HasColumnName("amount_major");

            });

            modelBuilder.Entity<DealLog>(entity =>
            {
                entity.ToTable("deal_log")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.user).HasColumnName("user");
                entity.Property(e => e.action).HasColumnName("action");
                entity.Property(e => e.type).HasColumnName("type");
                entity.Property(e => e.deal_id).HasColumnName("deal_id");
                entity.Property(e => e.deal_request_code).HasColumnName("deal_request_code");
                entity.Property(e => e.deal_reference).HasColumnName("deal_reference");
                entity.Property(e => e.qty_code).HasColumnName("qty_code");
                entity.Property(e => e.bill_id).HasColumnName("bill_id");
                entity.Property(e => e.remark).HasColumnName("remark");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.requested_at).HasColumnName("requested_at");
                entity.Property(e => e.link_file).HasColumnName("link_file");

            });

            modelBuilder.Entity<DealRequested>(entity =>
            {
                entity.ToTable("deal_requested")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.deal_id).HasColumnName("deal_id");
                entity.Property(e => e.deal_supplier_id).HasColumnName("deal_supplier_id");
                entity.Property(e => e.deal_customer_id).HasColumnName("deal_customer_id");
                entity.Property(e => e.deal_quantity).HasColumnName("deal_quantity");
                entity.Property(e => e.deal_reference).HasColumnName("deal_reference");
                entity.Property(e => e.deal_quatation_ref).HasColumnName("deal_quatation_ref");
                entity.Property(e => e.deal_quatation_file_ref).HasColumnName("deal_quatation_file_ref");
                entity.Property(e => e.deal_quatation_link_ref).HasColumnName("deal_quatation_link_ref");
                entity.Property(e => e.deal_quatation).HasColumnName("deal_quatation");
                entity.Property(e => e.deal_quatation_file).HasColumnName("deal_quatation_file");
                entity.Property(e => e.deal_quatation_link).HasColumnName("deal_quatation_link");
                entity.Property(e => e.deal_receipt).HasColumnName("deal_receipt");
                entity.Property(e => e.deal_reference).HasColumnName("deal_reference");
                entity.Property(e => e.deal_start_date).HasColumnName("deal_start_date");
                entity.Property(e => e.deal_end_date).HasColumnName("deal_end_date");
                entity.Property(e => e.deal_reference).HasColumnName("deal_reference");
                entity.Property(e => e.deal_email).HasColumnName("deal_email");
                entity.Property(e => e.deal_customer_name).HasColumnName("deal_customer_name");
                entity.Property(e => e.remark).HasColumnName("remark");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.status).HasColumnName("status");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
                entity.Property(e => e.deal_requested_file).HasColumnName("deal_requested_file");
                entity.Property(e => e.deal_po_file).HasColumnName("deal_po_file");
                entity.Property(e => e.deal_po_link).HasColumnName("deal_po_link");

            });

            modelBuilder.Entity<DealUser>(entity =>
            {
                entity.ToTable("deal_user")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.user_id).HasColumnName("user_id");
                entity.Property(e => e.is_admin).HasColumnName("is_admin");
                entity.Property(e => e.token).HasColumnName("token");
                entity.Property(e => e.status).HasColumnName("status");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");

            });

            modelBuilder.Entity<DealOrderCode>(entity =>
            {
                entity.ToTable("deal_order_code")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.deal_id).HasColumnName("deal_id");
                entity.Property(e => e.remark).HasColumnName("remark");
                entity.Property(e => e.status).HasColumnName("status");
                entity.Property(e => e.created_at).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
                entity.Property(e => e.start_date).HasColumnName("start_date");
                entity.Property(e => e.end_date).HasColumnName("end_date");
                entity.Property(e => e.quantity).HasColumnName("quantity");

            });

            modelBuilder.Entity<ReportTemplate>(entity =>
            {
                entity.ToTable("deal_report_template");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("created_by");

                entity.Property(e => e.DetailQuery).HasColumnName("detailQuery");

                entity.Property(e => e.DetailQueryCount).HasColumnName("detailQueryCount");

                entity.Property(e => e.DisplayColumnWidth)
                    .HasColumnName("displayColumnWidth")
                    .HasComment("[20, 40, 40]");

                entity.Property(e => e.FilteredFieldList)
                    .HasColumnName("filteredFieldList")
                    .HasComment("(first_name, last_name)");

                entity.Property(e => e.FilteredPlaceHolder)
                    .HasColumnName("filteredPlaceHolder")
                    .HasComment("'First Name', 'Last Name'");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.IsAdmin).HasColumnName("isAdmin");

                entity.Property(e => e.IsUser).HasColumnName("isUser");

                entity.Property(e => e.KeyField)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("keyField");

                entity.Property(e => e.Query).HasColumnName("query");

                entity.Property(e => e.QueryCount).HasColumnName("queryCount");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("title");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("updated_by");

                entity.Property(e => e.Weight).HasColumnName("weight");
            });

            modelBuilder.Entity<DealReconcile>(entity =>
            {
                entity.ToTable("deal_reconcile")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.deal_id).HasColumnName("deal_id");
                entity.Property(e => e.deal_reference).HasColumnName("deal_reference");
                entity.Property(e => e.invoice).HasColumnName("invoice");
                entity.Property(e => e.qty_code).HasColumnName("qty_code");
                entity.Property(e => e.remark).HasColumnName("remark");
                entity.Property(e => e.status).HasColumnName("status");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
            });

            modelBuilder.Entity<InvoiceProduct>(entity =>
            {
                entity.ToTable("t_inv_product")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.product_id).HasColumnName("product_id");
                entity.Property(e => e.product_name).HasColumnName("product_name");
                entity.Property(e => e.remark).HasColumnName("remark");
                entity.Property(e => e.status).HasColumnName("status");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
                entity.Property(e => e.deleted_by).HasColumnName("deleted_by");
                entity.Property(e => e.deleted_at).HasColumnName("deleted_at");
            });

            modelBuilder.Entity<InvoiceOrderItem>(entity =>
            {
                entity.ToTable("t_inv_order_item")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.product_id).HasColumnName("product_id");
                entity.Property(e => e.inv_order).HasColumnName("inv_order");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
            });

            modelBuilder.Entity<InvoicePrivilge>(entity =>
            {
                entity.ToTable("t_inv_privilege")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.receive_date).HasColumnName("receive_date");
                entity.Property(e => e.redemption_date).HasColumnName("redemption_date");
                entity.Property(e => e.order_id).HasColumnName("order_id");
                entity.Property(e => e.order_total).HasColumnName("order_total");
                entity.Property(e => e.unit_price).HasColumnName("unit_price");
                entity.Property(e => e.quantity).HasColumnName("quantity");
                entity.Property(e => e.inv_order_id).HasColumnName("inv_order_id");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
            });

            modelBuilder.Entity<InvoiceOrder>(entity =>
            {
                entity.ToTable("t_inv_order")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.inv_order_id).HasColumnName("inv_order_id");
                entity.Property(e => e.recipient_name).HasColumnName("recipient_name");
                entity.Property(e => e.phone).HasColumnName("phone");
                entity.Property(e => e.email).HasColumnName("email");
                entity.Property(e => e.address).HasColumnName("address");
                entity.Property(e => e.postcode).HasColumnName("postcode");
                entity.Property(e => e.invoice).HasColumnName("invoice");
                entity.Property(e => e.status).HasColumnName("status");
                entity.Property(e => e.tracking).HasColumnName("tracking");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
            });

            modelBuilder.Entity<DealQuatation>(entity =>
            {
                entity.ToTable("deal_quatation")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.deal_quatation).HasColumnName("deal_quatation");
                entity.Property(e => e.deal_quatation_file).HasColumnName("deal_quatation_file");
                entity.Property(e => e.deal_quatation_link).HasColumnName("deal_quatation_link");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
            });

            modelBuilder.Entity<DealUploadFile>(entity =>
            {
                entity.ToTable("deal_upload_file")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.deal_file_description).HasColumnName("deal_file_description");
                entity.Property(e => e.deal_file_path).HasColumnName("deal_file_path");
                entity.Property(e => e.deal_file_link).HasColumnName("deal_file_link");
                entity.Property(e => e.deal_type_of_file).HasColumnName("deal_type_of_file");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
            });

            modelBuilder.Entity<DealUserTaxInvoice>(entity =>
            {
                entity.ToTable("deal_user_tax_inv")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.file_id).HasColumnName("file_id");
                entity.Property(e => e.name).HasColumnName("name");
                entity.Property(e => e.address).HasColumnName("address");
                entity.Property(e => e.phone_number).HasColumnName("phone_number");
                entity.Property(e => e.email).HasColumnName("email");
                entity.Property(e => e.postcode).HasColumnName("postcode");
                entity.Property(e => e.status).HasColumnName("status");
                entity.Property(e => e.inv_no).HasColumnName("inv_no");
                entity.Property(e => e.total_price).HasColumnName("total_price");
                entity.Property(e => e.vat7percent).HasColumnName("vat7percent");
                entity.Property(e => e.total_price_without_tax).HasColumnName("total_price_without_tax");
                entity.Property(e => e.remark).HasColumnName("remark");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
                entity.Property(e => e.tracking_no).HasColumnName("tracking_no");
                entity.Property(e => e.tracking_date).HasColumnName("tracking_date");
                entity.Property(e => e.blob_image_invoice).HasColumnName("blob_image_invoice");
            });

            modelBuilder.Entity<DealInvoiceDetail>(entity =>
            {
                entity.ToTable("deal_inv_detail")
                .HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.inv_no).HasColumnName("inv_no");
                entity.Property(e => e.deal_id).HasColumnName("deal_id");
                entity.Property(e => e.item_detail).HasColumnName("item_detail");
                entity.Property(e => e.quantity).HasColumnName("quantity");
                entity.Property(e => e.price).HasColumnName("price");
                entity.Property(e => e.option_selected).HasColumnName("option_selected");
                entity.Property(e => e.remark).HasColumnName("remark");
                entity.Property(e => e.created_by).HasColumnName("created_by");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.updated_by).HasColumnName("updated_by");
                entity.Property(e => e.updated_at).HasColumnName("updated_at");
            });



            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
