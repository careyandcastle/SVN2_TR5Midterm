using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 計量表種類檔
    {
        public 計量表種類檔()
        {
            水電總表檔 = new HashSet<水電總表檔>();
        }

        [Key]
        [StringLength(2)]
        public string 計量表種類編號 { get; set; }
        [Required]
        [StringLength(20)]
        public string 計量表種類 { get; set; }
        [Required]
        [StringLength(10)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

        [InverseProperty("計量表種類編號Navigation")]
        public virtual ICollection<水電總表檔> 水電總表檔 { get; set; }
    }
}
