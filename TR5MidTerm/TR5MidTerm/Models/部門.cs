using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 部門
    {
        public 部門()
        {
            分部 = new HashSet<分部>();
        }

        [Key]
        [StringLength(2)]
        public string 單位 { get; set; }
        [Key]
        [Column("部門")]
        [StringLength(2)]
        public string 部門1 { get; set; }
        [Required]
        [StringLength(12)]
        public string 部門名稱 { get; set; }
        public bool 組織狀態 { get; set; }
        [Required]
        [StringLength(5)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改日期時間 { get; set; }

        [ForeignKey(nameof(單位))]
        [InverseProperty("部門")]
        public virtual 單位 單位Navigation { get; set; }
        [InverseProperty("部門Navigation")]
        public virtual ICollection<分部> 分部 { get; set; }
    }
}
