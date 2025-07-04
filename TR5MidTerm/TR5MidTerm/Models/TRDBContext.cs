using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TscLibCore.BaseObject;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class TRDBContext : BaseDbContext
    {
        public TRDBContext()
        {
        }

        public TRDBContext(DbContextOptions<TRDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<事業> 事業 { get; set; }
        public virtual DbSet<分部> 分部 { get; set; }
        public virtual DbSet<商品檔> 商品檔 { get; set; }
        public virtual DbSet<商品類別檔> 商品類別檔 { get; set; }
        public virtual DbSet<單位> 單位 { get; set; }
        public virtual DbSet<承租人檔> 承租人檔 { get; set; }
        public virtual DbSet<收款主檔> 收款主檔 { get; set; }
        public virtual DbSet<收款明細檔> 收款明細檔 { get; set; }
        public virtual DbSet<水電分表檔> 水電分表檔 { get; set; }
        public virtual DbSet<水電總表檔> 水電總表檔 { get; set; }
        public virtual DbSet<租約主檔> 租約主檔 { get; set; }
        public virtual DbSet<租約明細檔> 租約明細檔 { get; set; }
        public virtual DbSet<租約水電檔> 租約水電檔 { get; set; }
        public virtual DbSet<租賃方式檔> 租賃方式檔 { get; set; }
        public virtual DbSet<租賃用途檔> 租賃用途檔 { get; set; }
        public virtual DbSet<稅別檔> 稅別檔 { get; set; }
        public virtual DbSet<計量表種類檔> 計量表種類檔 { get; set; }
        public virtual DbSet<身分別檔> 身分別檔 { get; set; }
        public virtual DbSet<部門> 部門 { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Chinese_Taiwan_Stroke_CI_AS");

            modelBuilder.Entity<事業>(entity =>
            {
                entity.Property(e => e.事業1)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.修改人)
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

                entity.HasOne(d => d.部門Navigation)
                    .WithMany(p => p.分部)
                    .HasForeignKey(d => new { d.單位, d.部門 })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_分部_部門");
            });

            modelBuilder.Entity<商品檔>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.單位, e.部門, e.分部, e.商品編號 });

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

                entity.Property(e => e.商品編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.商品類別編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.物件編號).IsUnicode(false);

                entity.HasOne(d => d.商品類別編號Navigation)
                    .WithMany(p => p.商品檔)
                    .HasForeignKey(d => d.商品類別編號)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_商品檔_商品類別檔");
            });

            modelBuilder.Entity<商品類別檔>(entity =>
            {
                entity.Property(e => e.商品類別編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.作業別編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.稅別編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.稅別編號Navigation)
                    .WithMany(p => p.商品類別檔)
                    .HasForeignKey(d => d.稅別編號)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_商品類別檔_稅別檔");
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

            modelBuilder.Entity<承租人檔>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.單位, e.部門, e.分部, e.承租人編號 });

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

                entity.Property(e => e.承租人編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.修改時間).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.身分別編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.身分別編號Navigation)
                    .WithMany(p => p.承租人檔)
                    .HasForeignKey(d => d.身分別編號)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_承租人檔_身分別檔");
            });

            modelBuilder.Entity<收款主檔>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.單位, e.部門, e.分部, e.案號 });

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

                entity.Property(e => e.案號)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<收款明細檔>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.單位, e.部門, e.分部, e.案號, e.計租年月 });

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

                entity.Property(e => e.案號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.收款主檔)
                    .WithMany(p => p.收款明細檔)
                    .HasForeignKey(d => new { d.事業, d.單位, d.部門, d.分部, d.案號 })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_收款明細檔_收款主檔");
            });

            modelBuilder.Entity<水電分表檔>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.單位, e.部門, e.分部, e.總表號, e.分表號 });

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

                entity.Property(e => e.總表號).IsUnicode(false);

                entity.HasOne(d => d.水電總表檔)
                    .WithMany(p => p.水電分表檔)
                    .HasForeignKey(d => new { d.事業, d.單位, d.部門, d.分部, d.總表號 })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_水電分表檔_水電總表檔");
            });

            modelBuilder.Entity<水電總表檔>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.單位, e.部門, e.分部, e.總表號 });

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

                entity.Property(e => e.總表號).IsUnicode(false);

                entity.Property(e => e.案號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.計量表種類編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.計量表種類編號Navigation)
                    .WithMany(p => p.水電總表檔)
                    .HasForeignKey(d => d.計量表種類編號)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_水電總表檔_計量表種類檔");
            });

            modelBuilder.Entity<租約主檔>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.單位, e.部門, e.分部, e.案號 });

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

                entity.Property(e => e.案號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.承租人編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.租約終止日期).HasDefaultValueSql("('0001-01-01')");

                entity.Property(e => e.租賃方式編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.租賃方式編號Navigation)
                    .WithMany(p => p.租約主檔)
                    .HasForeignKey(d => d.租賃方式編號)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_租約主檔_租賃方式檔");

                entity.HasOne(d => d.承租人檔)
                    .WithMany(p => p.租約主檔)
                    .HasForeignKey(d => new { d.事業, d.單位, d.部門, d.分部, d.承租人編號 })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_租約主檔_承租人檔");
            });

            modelBuilder.Entity<租約明細檔>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.單位, e.部門, e.分部, e.案號, e.商品編號 });

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

                entity.Property(e => e.案號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.商品編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.商品檔)
                    .WithMany(p => p.租約明細檔)
                    .HasForeignKey(d => new { d.事業, d.單位, d.部門, d.分部, d.商品編號 })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_租約明細檔_商品檔");

                entity.HasOne(d => d.租約主檔)
                    .WithMany(p => p.租約明細檔)
                    .HasForeignKey(d => new { d.事業, d.單位, d.部門, d.分部, d.案號 })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_租約明細檔_租約主檔");
            });

            modelBuilder.Entity<租約水電檔>(entity =>
            {
                entity.HasKey(e => new { e.事業, e.單位, e.部門, e.分部, e.案號, e.總表號, e.分表號 })
                    .HasName("PK_租約水電");

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

                entity.Property(e => e.案號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.總表號).IsUnicode(false);

                entity.HasOne(d => d.租約主檔)
                    .WithMany(p => p.租約水電檔)
                    .HasForeignKey(d => new { d.事業, d.單位, d.部門, d.分部, d.案號 })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_租約水電_租約主檔");
            });

            modelBuilder.Entity<租賃方式檔>(entity =>
            {
                entity.Property(e => e.租賃方式編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<稅別檔>(entity =>
            {
                entity.Property(e => e.稅別編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<計量表種類檔>(entity =>
            {
                entity.HasKey(e => e.計量表種類編號)
                    .HasName("PK_計量表種類");

                entity.Property(e => e.計量表種類編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<身分別檔>(entity =>
            {
                entity.Property(e => e.身分別編號)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.修改時間).HasDefaultValueSql("(getdate())");
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

                entity.HasOne(d => d.單位Navigation)
                    .WithMany(p => p.部門)
                    .HasForeignKey(d => d.單位)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_部門_單位");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        internal Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
