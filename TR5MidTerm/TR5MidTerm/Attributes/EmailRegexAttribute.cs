using System.ComponentModel.DataAnnotations;

namespace TR5MidTerm.Attributes
{
    public class EmailRegexAttribute : RegularExpressionAttribute
    {
        public EmailRegexAttribute()
            : base(@"^[\w\.-]+@[\w\.-]+\.(com|com\.tw|org|edu\.tw)$")
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name}格式錯誤，請輸入有效 Email（僅限 .com、.com.tw 等）";
        }
    }
}
