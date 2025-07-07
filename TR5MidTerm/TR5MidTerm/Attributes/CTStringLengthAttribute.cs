using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TR5MidTerm.Attributes
{
    public class CTStringLengthAttribute : StringLengthAttribute
    {
        public CTStringLengthAttribute(int maximumLength) : base(maximumLength)
        {

        }

        public override string FormatErrorMessage(string name)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                return ErrorMessage;
            }

            if (this.MinimumLength == 0)
            {
                return $"{name}長度最多為{MaximumLength}";
            }
            return $"{name}長度請在{MinimumLength}到{MaximumLength}之間";
        }
    }
}
