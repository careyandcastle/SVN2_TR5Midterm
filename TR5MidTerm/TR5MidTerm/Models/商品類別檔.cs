using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 商品類別檔
    {
        public 商品類別檔()
        {
            商品檔 = new HashSet<商品檔>();
        }

        [Key]
        [StringLength(2)]
        public string 商品類別編號 { get; set; }
        [Required]
        [StringLength(20)]
        public string 商品類別 { get; set; }
        [Required]
        [StringLength(2)]
        public string 稅別編號 { get; set; }
        [Required]
        [StringLength(2)]
        public string 作業別編號 { get; set; }
        public bool? 租約選項 { get; set; }
        [Required]
        [StringLength(10)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

        [ForeignKey(nameof(稅別編號))]
        [InverseProperty(nameof(稅別檔.商品類別檔))]
        public virtual 稅別檔 稅別編號Navigation { get; set; }
        [InverseProperty("商品類別編號Navigation")]
        public virtual ICollection<商品檔> 商品檔 { get; set; }
    }
}
