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
    public partial class 租約明細檔BaseViewModel
    {
        [Key]
        [HiddenForView]
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
        [Key]
        [CTRequired]
        [CTStringLength(5)]
        //[StringLength(5)]
        public string 案號 { get; set; }
        [Key]
        [CTRequired]
        [CTStringLength(5)]
        //[StringLength(5)]
        public string 商品編號 { get; set; }
        [CTRequired]
        [Column(TypeName = "decimal(18, 0)")]
        public decimal 數量 { get; set; }
    }
}
