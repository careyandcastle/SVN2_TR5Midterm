using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 租賃用途檔
    {
        [Key]
        [StringLength(20)]
        public string 租賃用途編號 { get; set; }
        [Required]
        [StringLength(20)]
        public string 租賃用途 { get; set; }
        [Required]
        [StringLength(10)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }
    }
}
