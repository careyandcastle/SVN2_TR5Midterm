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
    public partial class 租賃住宅資料DisplayViewModel : IHasOrgNameDisplay
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
        //[StringLength(50)]
        public string 宿舍名稱 { get; set; }
        //[StringLength(50)]
        public string 出租大類編號 { get; set; }
        //[StringLength(50)]
        public string 出租大類名稱 { get; set; }
        //[StringLength(50)]
        public string 出租中類編號 { get; set; }
        //[StringLength(50)]
        public string 出租中類名稱 { get; set; }
        public double? 數量 { get; set; }
        //[StringLength(50)]
        public string 產品單位 { get; set; }
        [Key]
        //[StringLength(50)]
        public string 產品編號 { get; set; }
        //[StringLength(50)]
        public string 產品名稱 { get; set; }
        public int? 可承受數量 { get; set; }
        public double? 單價 { get; set; }
        public double? 坪數 { get; set; }
    }
}
