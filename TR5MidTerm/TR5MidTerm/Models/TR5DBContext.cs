using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class TR5DBContext : DbContext
    {
        public TR5DBContext()
        {
        }

        public TR5DBContext(DbContextOptions<TR5DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Business> Business { get; set; }
        public virtual DbSet<BusinessDetail> BusinessDetail { get; set; }
        public virtual DbSet<Customers> Customers { get; set; }
        public virtual DbSet<ProductTypes> ProductTypes { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<StoreProducts> StoreProducts { get; set; }
        public virtual DbSet<SystemParameters> SystemParameters { get; set; }
        public virtual DbSet<事業> 事業 { get; set; }
        public virtual DbSet<修改人> 修改人 { get; set; }
        public virtual DbSet<分部> 分部 { get; set; }
        public virtual DbSet<單位> 單位 { get; set; }
        public virtual DbSet<建物主檔> 建物主檔 { get; set; }
        public virtual DbSet<數量折扣主檔> 數量折扣主檔 { get; set; }
        public virtual DbSet<數量折扣明細> 數量折扣明細 { get; set; }
        public virtual DbSet<數量折扣明細異動記錄> 數量折扣明細異動記錄 { get; set; }
        public virtual DbSet<租賃住宅資料> 租賃住宅資料 { get; set; }
        public virtual DbSet<部門> 部門 { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Chinese_Taiwan_Stroke_CI_AS");

            modelBuilder.Entity<Business>(entity =>
            {
                entity.HasKey(e => new { e.BusinessMonth, e.BusinessID });

                entity.Property(e => e.CustomerID)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.DepartmentNo)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.UPD_USR)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Business)
                    .HasForeignKey(d => d.CustomerID)
                    .HasConstraintName("FK_Business_Customers");
            });

            modelBuilder.Entity<BusinessDetail>(entity =>
            {
                entity.HasKey(e => new { e.BusinessMonth, e.BusinessID, e.ProductID });

                entity.Property(e => e.ProductID)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.UPD_USR)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.BusinessDetail)
                    .HasForeignKey(d => d.ProductID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BusinessDetail_Products");

                entity.HasOne(d => d.Business)
                    .WithMany(p => p.BusinessDetail)
                    .HasForeignKey(d => new { d.BusinessMonth, d.BusinessID })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BusinessDetail_Business");
            });

            modelBuilder.Entity<Customers>(entity =>
            {
                entity.Property(e => e.CustomerID)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.AlwaysEncryptName).UseCollation("Chinese_Taiwan_Stroke_BIN2");

                entity.Property(e => e.UPD_USR)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<ProductTypes>(entity =>
            {
                entity.Property(e => e.ProductType)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.UPD_USR)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.Property(e => e.ProductID)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.ProductType)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.UPD_USR)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.ProductTypeNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Products_ProductTypes");
            });

            modelBuilder.Entity<StoreProducts>(entity =>
            {
                entity.HasKey(e => new { e.DepartmentNo, e.ProductID })
                    .HasName("PK_ProductStat");

                entity.Property(e => e.DepartmentNo)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.ProductID)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.UPD_USR)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.StoreProducts)
                    .HasForeignKey(d => d.ProductID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductStat_Products");
            });

            modelBuilder.Entity<SystemParameters>(entity =>
            {
                entity.Property(e => e.UPD_USER).IsFixedLength(true);
            });

            modelBuilder.Entity<事業>(entity =>
            {
                entity.Property(e => e.事業1)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.修改人)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<修改人>(entity =>
            {
                entity.Property(e => e.修改人1)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<分部>(entity =>
            {
                entity.HasKey(e => new { e.單位, e.部門, e.分部1 });

                entity.Property(e => e.單位)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.部門)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.分部1)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.修改人)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<單位>(entity =>
            {
                entity.Property(e => e.單位1)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.修改人)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<建物主檔>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.單位, e.部門, e.分部, e.建物編號 });

                entity.Property(e => e.事業)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.單位)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.部門)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.分部)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.建物編號).IsUnicode(false);

                entity.Property(e => e.地址).IsFixedLength(true);
            });

            modelBuilder.Entity<數量折扣主檔>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.數量折扣代號 });

                entity.Property(e => e.事業)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.修改人)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.事業Navigation)
                    .WithMany(p => p.數量折扣主檔)
                    .HasForeignKey(d => d.事業)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_數量折扣主檔_事業");
            });

            modelBuilder.Entity<數量折扣明細>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.數量折扣代號, e.折扣順序 });

                entity.Property(e => e.事業)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.修改人)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.數量折扣主檔)
                    .WithMany(p => p.數量折扣明細)
                    .HasForeignKey(d => new { d.事業, d.數量折扣代號 })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_數量折扣明細_數量折扣主檔");
            });

            modelBuilder.Entity<數量折扣明細異動記錄>(entity =>
            {
                entity.Property(e => e.事業)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.修改人)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<租賃住宅資料>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.單位, e.部門, e.分部, e.產品編號 });

                entity.Property(e => e.事業)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.單位)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.部門)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.分部)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.產品編號).IsUnicode(false);
            });

            modelBuilder.Entity<部門>(entity =>
            {
                entity.HasKey(e => new { e.單位, e.部門1 });

                entity.Property(e => e.單位)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.部門1)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.修改人)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
