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
    public partial class 承租人檔BaseViewModel
    {

        [Key]
        [HiddenForView]
        //[Required]
        //[StringLength(2)]
        public string 事業 { get; set; }
        [Key]
        [HiddenForView]
        //[Required]
        [StringLength(2)]
        public string 單位 { get; set; }
        [Key]
        [HiddenForView]
        //[Required]
        //[StringLength(2)]
        public string 部門 { get; set; }
        [Key]
        [HiddenForView]
        //[Required]
        //[StringLength(2)]
        public string 分部 { get; set; }
        [Key]
        [HiddenForView]
        //[Required]
        [CTStringLength(5)]
        //[StringLength(5)]
        public string 承租人編號 { get; set; }
        //[Required]
        [HiddenForView]
        public byte[] 承租人 { get; set; }
        [CTRequired]
        [CTStringLength(2)]
        //[StringLength(2)]
        [HiddenForView]
        public string 身分別編號 { get; set; }
        //[Required]
        [HiddenForView]
        public byte[] 統一編號 { get; set; }
        [HiddenForView]
        public byte[] 行動電話 { get; set; }
        [HiddenForView]
        public byte[] 電子郵件 { get; set; }
 
        [HiddenForView]
        public bool 刪除註記 { get; set; }
        //[Required]
        [StringLength(10)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

         
    }
}
