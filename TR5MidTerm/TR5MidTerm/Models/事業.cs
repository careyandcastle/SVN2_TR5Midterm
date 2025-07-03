using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 事業
    {
        [Key]
        [Column("事業")]
        [StringLength(2)]
        public string 事業1 { get; set; }
        [Required]
        [StringLength(12)]
        public string 事業名稱 { get; set; }
        public bool 組織狀態 { get; set; }
        [Required]
        [StringLength(5)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改日期時間 { get; set; }
    }
}
