using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR5MidTerm.Attributes
{
    public class DecimalRegexAttribute : RegularExpressionAttribute
    {
        private int _precision;

        private int _storageBytes;

        private bool _allowNegative;


        private static string GeneratePattern(int precision, int storageBytes, bool allowNegative)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("^");
            if (allowNegative)
            {
                sb.Append("-?");
            }

            int intPart = precision - storageBytes;

            if(intPart > 0)
            {
                sb.Append($"\\d{{1,{intPart}}}");
            }
            else
            {
                sb.Append("0?");
            }

            if(storageBytes > 0)
            {
                sb.Append($"(\\.\\d{{1,{storageBytes}}})?");
            }

            sb.Append("$");

            return sb.ToString();
        }

        /// <summary>
        /// 驗證decimal格式, decimal(12, 3) => DecimalRegexAttribute(12, 3, true)
        /// 如果不允許負號, decimal(12, 3) => DecimalRegexAttribute(12, 3, false)
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="storageBytes"></param>
        /// <param name="allowNegative"></param>
        public DecimalRegexAttribute(int precision, int storageBytes, bool allowNegative)
            :base(GeneratePattern(precision, storageBytes, allowNegative))
        {
            _precision = precision;
            _storageBytes = storageBytes;
            _allowNegative = allowNegative;

            if(_precision <= 0)
            {
                throw new Exception("precision不可少於等於0");
            }

            if (_precision < _storageBytes)
            {
                throw new Exception("precision不可小於torageBytes");
            }
        }

        /// <summary>
        /// 驗證decimal格式, decimal(12, 3) => DecimalRegexAttribute(12, 3)
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="storageBytes"></param>
        public DecimalRegexAttribute(int precision, int storageBytes)
            :this(precision, storageBytes, true)
        {

        }

        public override string FormatErrorMessage(string name)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                return ErrorMessage;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append($"{name}請輸入");

            if(_allowNegative)
            {
                sb.Append("[-]");
            }

            int intPart = _precision - _storageBytes;

            if(intPart > 0)
            {
                sb.Append(new string('#', intPart));
            }
            else
            {
                sb.Append("0");
            }

            if(_storageBytes > 0)
            {
                sb.Append(".").Append(new string('#', _storageBytes));
            }

            sb.Append("格式之數字");

            return sb.ToString();
        }

    }
}
