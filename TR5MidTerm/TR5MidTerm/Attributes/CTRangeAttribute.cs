using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TR5MidTerm.Attributes
{
    public class CTRangeAttribute : RangeAttribute
    {
        public CTRangeAttribute(double minimum, double maximum) : base(minimum, maximum)
        {

        }

        public CTRangeAttribute(int minimum, int maximum) : base(minimum, maximum)
        {

        }

        public CTRangeAttribute(Type type, string minimum, string maximum) : base(type, minimum, maximum)
        {

        }

        public override string FormatErrorMessage(string name)
        {
            return !String.IsNullOrEmpty(ErrorMessage)
                ? ErrorMessage
                : $"{name}請在{this.Minimum}到{this.Maximum}之間";
        }
    }
}
