using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 收款明細檔
    {
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

        [Key] // ✅ 新增：流水號也是主鍵一部分
        public int 流水號 { get; set; }

        [Key]
        [Column(TypeName = "date")]
        public DateTime 計租年月 { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal 金額 { get; set; }
        [Required]
        [StringLength(10)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

        [ForeignKey("事業,單位,部門,分部,案號")]
        [InverseProperty("收款明細檔")]
        public virtual 收款主檔 收款主檔 { get; set; }
    }
}
