using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 商品檔
    {
        public 商品檔()
        {
            租約明細檔 = new HashSet<租約明細檔>();
        }

        [Key]
        [StringLength(2)]
        public string 事業 { get; set; }
        [Key]
        [StringLength(2)]
        public string 單位 { get; set; }
        [Key]
        [StringLength(2)]
        public string 部門 { get; set; }
        [Key]
        [StringLength(2)]
        public string 分部 { get; set; }
        [Key]
        [StringLength(5)]
        public string 商品編號 { get; set; }
        [Required]
        [StringLength(20)]
        public string 商品名稱 { get; set; }
        [Required]
        [StringLength(2)]
        public string 商品類別編號 { get; set; }
        [StringLength(50)]
        public string 物件編號 { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal 單價 { get; set; }
        [Required]
        [StringLength(10)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

        [ForeignKey(nameof(商品類別編號))]
        [InverseProperty(nameof(商品類別檔.商品檔))]
        public virtual 商品類別檔 商品類別編號Navigation { get; set; }
        [InverseProperty("商品檔")]
        public virtual ICollection<租約明細檔> 租約明細檔 { get; set; }
    }
}
