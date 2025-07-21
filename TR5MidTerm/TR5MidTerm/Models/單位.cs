using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 單位
    {
        public 單位()
        {
            部門 = new HashSet<部門>();
        }

        [Key]
        [Column("單位")]
        [StringLength(2)]
        public string 單位1 { get; set; }
        [Required]
        [StringLength(12)]
        public string 單位名稱 { get; set; }
        public bool 組織狀態 { get; set; }
        [Required]
        [StringLength(5)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改日期時間 { get; set; }
        [InverseProperty("單位Navigation")]
        public virtual ICollection<部門> 部門 { get; set; } 
    }
}
