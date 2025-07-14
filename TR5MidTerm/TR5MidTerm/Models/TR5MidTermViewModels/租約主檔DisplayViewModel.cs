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
    public partial class 租約主檔DisplayViewModel
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
        public string 案名 { get; set; }
        //[DisplayName("承租人名")]//
        //[HiddenForView]
        public string 承租人編號 { get; set; }
        //[DisplayName("承租方式")]
        //[HiddenForView]
        public string 租賃方式編號 { get; set; }
        [DisplayName("租賃方式")]
        public string 租賃方式顯示 { get; set; } //自定義欄位
        public string 租賃用途 { get; set; }
        //[Column(TypeName = "date")]
        public DateTime 租約起始日期 { get; set; }
        public int 累計月數 { get; set; }  // 計算從起始日期至今天過了幾個月
        public int 未繳期數 { get; set; } // 累計應收期數 - 已繳期數
        public DateTime? 下次收租日期 { get; set; } //
        public decimal 每期租金含稅 { get; set; }  //  
        public decimal 累計應收租金含稅 { get; set; }  // 
        //public decimal 累計金額 { get; set; }  //
        public int 租期月數 { get; set; }
        public int 計租週期月數 { get; set; }
        public int 繳款期限天數 { get; set; }
        public DateTime? 租約終止日期 { get; set; }
        public string 備註 { get; set; }
        public string 修改人 { get; set; }
        public DateTime 修改時間 { get; set; }
        [HiddenForView]
        public bool 可否展開明細{ get; set; }
        [HiddenForView]
        public bool 可否新增明細{ get; set; }
        
    }
}
