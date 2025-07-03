using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 身分別檔
    {
        public 身分別檔()
        {
            承租人檔 = new HashSet<承租人檔>();
        }

        [Key]
        [StringLength(2)]
        public string 身分別編號 { get; set; }
        [Required]
        [StringLength(20)]
        public string 身分別 { get; set; }
        [Required]
        [StringLength(10)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

        [InverseProperty("身分別編號Navigation")]
        public virtual ICollection<承租人檔> 承租人檔 { get; set; }
    }
}
