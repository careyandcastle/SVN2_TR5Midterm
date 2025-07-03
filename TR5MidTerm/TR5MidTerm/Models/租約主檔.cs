using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 租約主檔
    {
        public 租約主檔()
        {
            租約明細檔 = new HashSet<租約明細檔>();
            租約水電檔 = new HashSet<租約水電檔>();
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
        public string 案號 { get; set; }
        [StringLength(50)]
        public string 案名 { get; set; }
        [Required]
        [StringLength(5)]
        public string 承租人編號 { get; set; }
        [Required]
        [StringLength(2)]
        public string 租賃方式編號 { get; set; }
        [Required]
        [StringLength(20)]
        public string 租賃用途 { get; set; }
        [Column(TypeName = "date")]
        public DateTime 租約起始日期 { get; set; }
        public int 租期月數 { get; set; }
        public int 計租週期月數 { get; set; }
        public int 繳款期限天數 { get; set; }
        [Column(TypeName = "date")]
        public DateTime? 租約終止日期 { get; set; }
        [StringLength(200)]
        public string 備註 { get; set; }
        [Required]
        [StringLength(10)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

        [ForeignKey("事業,單位,部門,分部,承租人編號")]
        [InverseProperty("租約主檔")]
        public virtual 承租人檔 承租人檔 { get; set; }
        [ForeignKey(nameof(租賃方式編號))]
        [InverseProperty(nameof(租賃方式檔.租約主檔))]
        public virtual 租賃方式檔 租賃方式編號Navigation { get; set; }
        [InverseProperty("租約主檔")]
        public virtual ICollection<租約明細檔> 租約明細檔 { get; set; }
        [InverseProperty("租約主檔")]
        public virtual ICollection<租約水電檔> 租約水電檔 { get; set; }
    }
}
