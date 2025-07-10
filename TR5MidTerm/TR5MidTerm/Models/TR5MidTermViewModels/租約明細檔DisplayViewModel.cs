using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TscLibCore.Attribute;

#nullable disable

namespace TR5MidTerm.Models.TR5MidTermViewModels
{
    public partial class 租約明細檔DisplayViewModel
    {
        [Key]
        [HiddenForView]
        public string 事業 { get; set; }

        [DisplayName("事業")]
        public string 事業顯示 { get; set; }

        [Key]
        [HiddenForView]
        public string 單位 { get; set; }

        [DisplayName("單位")]
        public string 單位顯示 { get; set; }

        [Key]
        [HiddenForView]
        public string 部門 { get; set; }

        [DisplayName("部門")]
        public string 部門顯示 { get; set; }

        [Key]
        [HiddenForView]
        public string 分部 { get; set; }

        [DisplayName("分部")]
        public string 分部顯示 { get; set; }

        [Key]
        public string 案號 { get; set; }
        [Key]
        [DisplayName("商品名稱")]//
        public string 商品編號 { get; set; }
        public decimal 數量 { get; set; }
        public string 修改人 { get; set; }
        public DateTime 修改時間 { get; set; }
    }
}
