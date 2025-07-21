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
    public partial class 建物主檔DisplayViewModel : IHasOrgNameDisplay
    {
        [DisplayName("事業")]
        public string 事業顯示 { get; set; }
        [DisplayName("單位")]
        public string 單位顯示 { get; set; }
        [DisplayName("部門")]
        public string 部門顯示 { get; set; }
        [DisplayName("分部")]
        public string 分部顯示 { get; set; }
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
        //[StringLength(50)]
        public string 建物編號 { get; set; }
        [Required]
        //[StringLength(20)]
        public string 建物名稱 { get; set; }
        //[StringLength(50)]
        public string 地址 { get; set; }
        //[Required]
        //[StringLength(10)]
        public string 修改人 { get; set; }
        //[Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }
    }
}
