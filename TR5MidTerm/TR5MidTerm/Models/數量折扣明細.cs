using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 數量折扣明細
    {
        [Key]
        [StringLength(2)]
        public string 事業 { get; set; }
        [Key]
        [StringLength(12)]
        public string 數量折扣代號 { get; set; }
        [Key]
        [Column(TypeName = "decimal(5, 3)")]
        public decimal 折扣順序 { get; set; }
        [Column(TypeName = "decimal(12, 0)")]
        public decimal 數量起 { get; set; }
        [Column(TypeName = "decimal(12, 4)")]
        public decimal 含稅單價 { get; set; }
        [Required]
        [StringLength(5)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改日期時間 { get; set; }

        [ForeignKey("事業,數量折扣代號")]
        [InverseProperty("數量折扣明細")]
        public virtual 數量折扣主檔 數量折扣主檔 { get; set; }
    }
}
