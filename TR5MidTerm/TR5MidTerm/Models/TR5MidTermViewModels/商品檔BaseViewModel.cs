using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TR5MidTerm.Attributes;
using TscLibCore.Attribute;

#nullable disable

namespace TR5MidTerm.Models.TR5MidTermViewModels
{
    public partial class 商品檔BaseViewModel
    {
        [Key]
        [HiddenForView]
        //[StringLength(2)]
        public string 事業 { get; set; }
        [Key]
        [HiddenForView]
        public string 單位 { get; set; }
        [Key]
        [HiddenForView]
        public string 部門 { get; set; }
        [Key]
        [HiddenForView]
        public string 分部 { get; set; }
        [CTRequired]
        [Key]
        [StringLength(5)]
        public string 商品編號 { get; set; }
        [CTRequired]
        [StringLength(20)]
        public string 商品名稱 { get; set; }
        [CTRequired]
        [StringLength(2)]
        public string 商品類別編號 { get; set; }
        [CTRequired]
        [StringLength(50)]
        public string 物件編號 { get; set; }
        [CTRequired]
        [Column(TypeName = "decimal(18, 0)")]
        public decimal 單價 { get; set; }
    }
}
