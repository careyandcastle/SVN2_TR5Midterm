using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 租約水電檔
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
        [Key]
        [StringLength(20)]
        public string 總表號 { get; set; }
        [Key]
        public int 分表號 { get; set; }
        [Required]
        [StringLength(10)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

        [ForeignKey("事業,單位,部門,分部,案號")]
        [InverseProperty("租約水電檔")]
        public virtual 租約主檔 租約主檔 { get; set; }
    }
}
