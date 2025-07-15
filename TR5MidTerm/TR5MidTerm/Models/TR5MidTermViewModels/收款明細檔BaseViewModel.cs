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
    public partial class 收款明細檔BaseViewModel
    {
        [Key]
        //[StringLength(2)]
        public string 事業 { get; set; }
        [Key]
        //[StringLength(2)]
        public string 單位 { get; set; }
        [Key]
        //[StringLength(2)]
        public string 部門 { get; set; }
        [Key]
        //[StringLength(2)]
        public string 分部 { get; set; }
        [Key]
        //[StringLength(5)]
        [HiddenForView]
        public string 案號 { get; set; }
        [DisplayName("案名")]
        public string 案號名顯示 { get; set; }//
        public int 可收期數上限 { get; set; } // 例如 result.租期月數
        public decimal 每期租金含稅 { get; set; } //  
        public int 每期月數 { get; set; } // 
        public decimal 每月租金含稅 { get; set; } // 
        public int 剩餘可收月數 { get; set; } // 從租約主檔取得

        [Required(ErrorMessage = "請輸入收款期數")]
        public int 收幾期 { get; set; } // 
        [Key]
        public int 流水號 { get; set; }
        [Key]
        public DateTime 計租年月 { get; set; }
        public decimal 金額 { get; set; }
 
    }
}
