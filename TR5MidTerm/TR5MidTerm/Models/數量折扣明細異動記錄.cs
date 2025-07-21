using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 數量折扣明細異動記錄
    {
        [Key]
        public int Log編號 { get; set; }
        [Required]
        [StringLength(2)]
        public string 事業 { get; set; }
        [Required]
        [StringLength(12)]
        public string 數量折扣代號 { get; set; }
        [Column(TypeName = "decimal(5, 3)")]
        public decimal 折扣順序 { get; set; }
        [Column(TypeName = "decimal(12, 0)")]
        public decimal 原數量起 { get; set; }
        [Column(TypeName = "decimal(12, 4)")]
        public decimal 原含稅單價 { get; set; }
        [Column(TypeName = "decimal(12, 0)")]
        public decimal 新數量起 { get; set; }
        [Column(TypeName = "decimal(12, 4)")]
        public decimal 新含稅單價 { get; set; }
        [Required]
        [StringLength(5)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改日期時間 { get; set; }
    }
}
