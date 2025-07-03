using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 分部
    {
        [Key]
        [StringLength(2)]
        public string 單位 { get; set; }
        [Key]
        [StringLength(2)]
        public string 部門 { get; set; }
        [Key]
        [Column("分部")]
        [StringLength(2)]
        public string 分部1 { get; set; }
        [Required]
        [StringLength(12)]
        public string 分部名稱 { get; set; }
        public bool 組織狀態 { get; set; }
        [Required]
        [StringLength(5)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改日期時間 { get; set; }

        [ForeignKey("單位,部門")]
        [InverseProperty("分部")]
        public virtual 部門 部門Navigation { get; set; }
    }
}
