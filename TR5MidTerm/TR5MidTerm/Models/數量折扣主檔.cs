using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 數量折扣主檔
    {
        public 數量折扣主檔()
        {
            數量折扣明細 = new HashSet<數量折扣明細>();
        }

        [Key]
        [StringLength(2)]
        public string 事業 { get; set; }
        [Key]
        [StringLength(12)]
        public string 數量折扣代號 { get; set; }
        [Required]
        [StringLength(20)]
        public string 數量折扣名稱 { get; set; }
        [Required]
        [StringLength(20)]
        public string 備註 { get; set; }
        public bool 是否停用 { get; set; }
        [Required]
        [StringLength(5)]
        public string 修改人 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime 修改日期時間 { get; set; }

        [ForeignKey(nameof(事業))]
        [InverseProperty("數量折扣主檔")]
        public virtual 事業 事業Navigation { get; set; }
        [InverseProperty("數量折扣主檔")]
        public virtual ICollection<數量折扣明細> 數量折扣明細 { get; set; }
    }
}
