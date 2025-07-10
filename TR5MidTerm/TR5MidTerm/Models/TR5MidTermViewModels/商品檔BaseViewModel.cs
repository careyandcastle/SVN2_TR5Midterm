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
        //[StringLength(2)]
        public string 單位 { get; set; }
        [Key]
        [HiddenForView]
        //[StringLength(2)]
        public string 部門 { get; set; }
        [Key]
        [HiddenForView]
        //[StringLength(2)]
        public string 分部 { get; set; }
        [Key]
        //[StringLength(5)]
        public string 商品編號 { get; set; }
        [CTRequired]
        //[Required]
        //[StringLength(20)]
        public string 商品名稱 { get; set; }
        [CTRequired]
        //[Required]
        //[StringLength(2)]
        public string 商品類別編號 { get; set; }
        //[StringLength(50)]
        public string 物件編號 { get; set; }
        [CTRequired]
        //[Column(TypeName = "decimal(18, 0)")]
        public decimal 單價 { get; set; }
        //[Required]
        //[StringLength(10)]
        //public string 修改人 { get; set; }
        //[Column(TypeName = "datetime")]
        //public DateTime 修改時間 { get; set; }

    }
}
