using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 租賃方式檔
    {
        public 租賃方式檔()
        {
            租約主檔 = new HashSet<租約主檔>();
        }

        [Key]
        [StringLength(2)]
        public string 租賃方式編號 { get; set; }
        [Required]
        [StringLength(20)]
        public string 租賃方式 { get; set; }
        [Required]
        [StringLength(10)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

        [InverseProperty("租賃方式編號Navigation")]
        public virtual ICollection<租約主檔> 租約主檔 { get; set; }
    }
}
