using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 稅別檔
    {
        public 稅別檔()
        {
            商品類別檔 = new HashSet<商品類別檔>();
        }

        [Key]
        [StringLength(2)]
        public string 稅別編號 { get; set; }
        [Required]
        [StringLength(20)]
        public string 稅別 { get; set; }
        [Column(TypeName = "decimal(18, 10)")]
        public decimal 稅率 { get; set; }
        public bool? 應稅 { get; set; }
        [Required]
        [StringLength(10)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

        [InverseProperty("稅別編號Navigation")]
        public virtual ICollection<商品類別檔> 商品類別檔 { get; set; }
    }
}
