using System.ComponentModel.DataAnnotations;

namespace TR5MidTerm.Attributes
{
    public class TenDigitCodeAttribute : RegularExpressionAttribute
    {
        public TenDigitCodeAttribute()
            : base(@"^0\d{9}$")
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name}必須為 0 開頭的 10 碼數字";
        }
    }
}
