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
    public partial class 水電總表檔BaseViewModel
    {

        [Key]
        //[StringLength(2)]
        [HiddenForView]
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
        [CTStringLength(20)]
        [CTRequired]
        public string 總表號 { get; set; } 
        [CTStringLength(5)]
        public string 案號 { get; set; }
        [CTStringLength(2)]
        [CTRequired]
        [DisplayName("計量表種類")]
        public string 計量表種類編號 { get; set; }
        //[Required]
        [CTStringLength(20)]
        [CTRequired]
        public string 計量對象 { get; set; }
        //[Required]
        //[StringLength(10)]
        //public string 修改人 { get; set; }
        //[Column(TypeName = "datetime")]
        //public DateTime 修改時間 { get; set; }


    }
}
