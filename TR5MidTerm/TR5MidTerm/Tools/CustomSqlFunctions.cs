using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TR5MidTerm.Models
{
    /// <summary>
    ///   因為有些C#函式不能被轉換為SQL, 所以手工處理
    /// </summary>
    public static class CustomSqlFunctions
    {
        /// <summary>
        ///   轉換解密後byte陣列為字串
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static string ConvertDecryptedByteArrayToString(byte[] datas) => throw new NotSupportedException();

        /// <summary>
        ///   轉換加密欄位為字串
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static string DecryptToString(byte[] datas) => throw new NotSupportedException();

        /// <summary>
        ///    將資料庫代碼與資料庫名稱串接為「{資料庫代號}_{資料庫名稱}」
        ///    因為ef直接拼接代號及名稱在值為null時, 會被自動轉為空字串, 最後顯示「_」, 所以自行刻函式處理
        ///    當「資料庫名稱」為null時, 只顯示「資料庫代號」
        /// </summary>
        /// <param name="code">資料庫代號</param>
        /// <param name="name">資料庫名稱，當「資料庫名稱」為null時, 只顯示「資料庫代號」</param>
        /// <returns></returns>
        public static string ConcatCodeAndName(string code, string name) => throw new NotSupportedException();

        /// <summary>
        /// 是否(字串A)>(字串B)
        /// </summary>
        /// <param name="fieldA"></param>
        /// <param name="fieldB"></param>
        /// <returns></returns>
        public static bool GreaterThan(string fieldA, string fieldB) => throw new NotSupportedException();

        /// <summary>
        /// 是否(字串A)<(字串B)
        /// </summary>
        /// <param name="fieldA"></param>
        /// <param name="fieldB"></param>
        /// <returns></returns>
        public static bool LessThan(string fieldA, string fieldB) => throw new NotSupportedException();

        /// <summary>
        /// 是否(字串A)>=(字串B)
        /// </summary>
        /// <param name="fieldA"></param>
        /// <param name="fieldB"></param>
        /// <returns></returns>
        public static bool GreaterThanOrEqual(string fieldA, string fieldB) => throw new NotSupportedException();

        /// <summary>
        /// 是否(字串A)<=(字串B)
        /// </summary>
        /// <param name="fieldA"></param>
        /// <param name="fieldB"></param>
        /// <returns></returns>
        public static bool LessThanOrEqual(string fieldA, string fieldB) => throw new NotSupportedException();

        /// <summary>
        /// 在特定位置插入<br>，讓很長的字串可以換行。
        /// 註:如果要輸入超過1000個字，請修改subStr2中SUBSTR的第三個參數。
        /// </summary>
        /// <param name="s">原字串</param>
        /// <param name="index">要插入的位置</param>
        /// <returns></returns>
        public static string AddHtmlLineBreakAt(this string s, int index) => throw new NotSupportedException();

        /// <summary>
        /// untested
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatTimeSpanToHHmm(this TimeSpan time) => throw new NotSupportedException();

        /// <summary>
        /// untested
        /// TimeSpan? == null ? null : TimeSpan.ToString("HH:mm")
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatNullableTimeSpanToHHmm(this TimeSpan? time) => throw new NotSupportedException();

        public static void Register(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFunction(typeof(CustomSqlFunctions).GetMethod(nameof(ConvertDecryptedByteArrayToString))).HasTranslation(ConvertToStringImpl);

            modelBuilder.HasDbFunction(typeof(CustomSqlFunctions).GetMethod(nameof(DecryptToString))).HasTranslation(DecryptToStringImpl);

            modelBuilder.HasDbFunction(typeof(CustomSqlFunctions).GetMethod(nameof(ConcatCodeAndName))).HasTranslation(ConcatCodeAndNameImpl);

            modelBuilder.HasDbFunction(typeof(CustomSqlFunctions).GetMethod(nameof(GreaterThan))).HasTranslation(GreaterThanImpl);

            modelBuilder.HasDbFunction(typeof(CustomSqlFunctions).GetMethod(nameof(LessThan))).HasTranslation(LessThanImpl);

            modelBuilder.HasDbFunction(typeof(CustomSqlFunctions).GetMethod(nameof(GreaterThanOrEqual))).HasTranslation(GreaterThanOrEqualImpl);

            modelBuilder.HasDbFunction(typeof(CustomSqlFunctions).GetMethod(nameof(LessThanOrEqual))).HasTranslation(LessThanOrEqualImpl);

            modelBuilder.HasDbFunction(typeof(CustomSqlFunctions).GetMethod(nameof(AddHtmlLineBreakAt))).HasTranslation(LineBreakAtImpl);

            modelBuilder.HasDbFunction(typeof(CustomSqlFunctions).GetMethod(nameof(FormatTimeSpanToHHmm))).HasTranslation(FormatNullableTimeSpanToHHmmImpl);
            modelBuilder.HasDbFunction(typeof(CustomSqlFunctions).GetMethod(nameof(FormatNullableTimeSpanToHHmm))).HasTranslation(FormatNullableTimeSpanToHHmmImpl);
        }

        /// <summary>
        ///   轉換byte陣列為字串, 用於將解密後資料轉換
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static SqlExpression ConvertToStringImpl(IReadOnlyCollection<SqlExpression> args)
        {
            /// CONVERT(NVARCHAR(MAX), ...) 
            /// 如果寫CAST要進行字串串接, 但傳入值有可能是SqlFunctionExpression, 無法串接
            return new SqlFunctionExpression(
                        "CONVERT",
                        args.Prepend(new SqlFragmentExpression($"NVARCHAR(MAX)")),
                        nullable: true, /// 回傳是否可為null
                        argumentsPropagateNullability: new[] { false, true }, /// 參數為null即回傳null
                        type: typeof(string), ///回傳型別
                        typeMapping: null);
        }

        /// <summary>
        ///   轉換加密欄位為字串, 用於將解密後資料轉換
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static SqlExpression DecryptToStringImpl(IReadOnlyCollection<SqlExpression> args)
        {
            /// CONVERT(Nvarchar(max), DecryptByKey(args))
            /// 不要用Cast + SqlFragmentExpression去拼字串, 有可能出現各種奇怪的bug
            var decryptExpression = new SqlFunctionExpression(
                        "DecryptByKey",
                        args,
                        nullable: true, /// 回傳是否可為null
                        argumentsPropagateNullability: new[] { true }, /// 參數為null即回傳null
                        type: typeof(byte[]), ///回傳型別
                        typeMapping: RelationalTypeMapping.NullMapping);

            return new SqlFunctionExpression(
                        "CONVERT",
                        new List<SqlExpression>() { new SqlFragmentExpression($"NVARCHAR(MAX)"), decryptExpression },
                        nullable: true, /// 回傳是否可為null
                        argumentsPropagateNullability: new[] { false, true }, /// 參數為null即回傳null
                        type: typeof(string), ///回傳型別
                        typeMapping: null);
        }

        private static SqlExpression ConcatCodeAndNameImpl(IReadOnlyCollection<SqlExpression> args)
        {
            SqlExpression codeField = args.First();
            SqlExpression nameField = args.Skip(1).First();

            var nullConstant = new SqlConstantExpression(
                Expression.Constant(null),
                RelationalTypeMapping.NullMapping);
            var testCondition = new SqlBinaryExpression(
                ExpressionType.Equal,
                nameField, nullConstant,
                typeof(bool),
                RelationalTypeMapping.NullMapping);
            var speratorConstant = new SqlConstantExpression(
                Expression.Constant("_"),
                nameField.TypeMapping); /// SqlServerString
            var concatReulst = new SqlBinaryExpression(
                ExpressionType.Add,
                new SqlBinaryExpression(
                    ExpressionType.Add,
                    codeField,
                    speratorConstant,
                    typeof(string),
                    nameField.TypeMapping), /// SqlServerString
                nameField,
                typeof(string),
                nameField.TypeMapping); /// SqlServerString

            return new CaseExpression(
                new List<CaseWhenClause> { new CaseWhenClause(testCondition, codeField) },
                concatReulst);
        }

        private static SqlExpression GreaterThanImpl(IReadOnlyCollection<SqlExpression> args)
        {
            SqlExpression filedA = args.First();
            SqlExpression filedB = args.Skip(1).First();

            var testCondition = new SqlBinaryExpression(
                ExpressionType.GreaterThan,
                filedA, filedB,
                typeof(bool),
                RelationalTypeMapping.NullMapping);

            return testCondition;
        }

        private static SqlExpression LessThanImpl(IReadOnlyCollection<SqlExpression> args)
        {
            SqlExpression filedA = args.First();
            SqlExpression filedB = args.Skip(1).First();

            var testCondition = new SqlBinaryExpression(
                ExpressionType.LessThan,
                filedA, filedB,
                typeof(bool),
                RelationalTypeMapping.NullMapping);

            return testCondition;
        }

        private static SqlExpression GreaterThanOrEqualImpl(IReadOnlyCollection<SqlExpression> args)
        {
            SqlExpression filedA = args.First();
            SqlExpression filedB = args.Skip(1).First();

            var testCondition = new SqlBinaryExpression(
                ExpressionType.GreaterThanOrEqual,
                filedA, filedB,
                typeof(bool),
                RelationalTypeMapping.NullMapping);

            return testCondition;
        }

        private static SqlExpression LessThanOrEqualImpl(IReadOnlyCollection<SqlExpression> args)
        {
            SqlExpression filedA = args.First();
            SqlExpression filedB = args.Skip(1).First();

            var testCondition = new SqlBinaryExpression(
                ExpressionType.LessThanOrEqual,
                filedA, filedB,
                typeof(bool),
                RelationalTypeMapping.NullMapping);

            return testCondition;
        }

        private static SqlExpression LineBreakAtImpl(IReadOnlyCollection<SqlExpression> args)
        {
            /*  CASE
                WHEN LEN(text) < index THEN text
                ELSE CONCAT(
                         SUBSTRING(text, 1, index),
                         '<br>',
                         SUBSTRING(text, index+1 ,1000)
             ) 
            */
            SqlExpression rawString = args.First();
            SqlExpression targetIndex = args.Skip(1).First();

            int index = (int)(targetIndex as SqlConstantExpression).Value;

            var subStr1 = new SqlFunctionExpression(
                      functionName: "SUBSTRING",
                         new[]
                         {
                             rawString,
                             new SqlConstantExpression(Expression.Constant(1), RelationalTypeMapping.NullMapping),
                             targetIndex,
                         },
                         nullable: true, /// 回傳是否可為null
                        argumentsPropagateNullability: new[] { true, true }, /// 參數為null即回傳null
                        type: typeof(string), ///回傳型別
                        typeMapping: RelationalTypeMapping.NullMapping);

            var subStr2 = new SqlFunctionExpression(
                      functionName: "SUBSTRING",
                         new[]
                         {
                            rawString,
                            new SqlConstantExpression(Expression.Constant(index+1), RelationalTypeMapping.NullMapping),
                            new SqlConstantExpression(Expression.Constant(1000), //要算長度有點麻煩，乾脆直接給1000
                            RelationalTypeMapping.NullMapping),
                         },
                          nullable: true, /// 回傳是否可為null
                        argumentsPropagateNullability: new[] { true, true }, /// 參數為null即回傳null
                        type: typeof(string), ///回傳型別
                        typeMapping: RelationalTypeMapping.NullMapping);

            var concatExp = new SqlFunctionExpression(
                      functionName: "CONCAT",
                         new SqlExpression[]
                          {
                                 subStr1,
                                 new SqlConstantExpression(Expression.Constant("\'<br>\'"), RelationalTypeMapping.NullMapping),
                                 subStr2
                         },
                          nullable: true, /// 回傳是否可為null
                        argumentsPropagateNullability: new[] { true, true }, /// 參數為null即回傳null
                        type: typeof(string), ///回傳型別
                        typeMapping: RelationalTypeMapping.NullMapping);

            var lenExp = new SqlFunctionExpression(
                        functionName: "LEN",
                        new[] { rawString },
                        nullable: true, /// 回傳是否可為null
                        argumentsPropagateNullability: new[] { true, true }, /// 參數為null即回傳null
                        type: typeof(string), ///回傳型別
                        typeMapping: RelationalTypeMapping.NullMapping);

            var testCondition = new SqlBinaryExpression(
                   ExpressionType.LessThan,
                   lenExp,
                   new SqlConstantExpression(Expression.Constant(index), RelationalTypeMapping.NullMapping),
                   typeof(bool),
                   RelationalTypeMapping.NullMapping);

            return new CaseExpression(
                new List<CaseWhenClause> { new CaseWhenClause(testCondition, rawString) },
                concatExp);
        }

        private static SqlExpression FormatNullableTimeSpanToHHmmImpl(IReadOnlyCollection<SqlExpression> args)
        {
            var intTypeMapping = new IntTypeMapping("int", DbType.Int32);

            // 取得第一個參數，這應該是一個TimeSpan或TimeSpan?的值
            SqlExpression timeSpanField = args.First();

            // 使用SQL的CONVERT函數來格式化時間
            var formattedTimeSpan = new SqlFunctionExpression(
                "CONVERT",
                new List<SqlExpression>
                {
                    new SqlFragmentExpression("VARCHAR(5)"),
                    timeSpanField,
                    new SqlConstantExpression(Expression.Constant(108),  intTypeMapping)
                },
                nullable: true,
                argumentsPropagateNullability: new[] { false, true, false },
                type: typeof(string), ///回傳型別
                typeMapping: null);

            return formattedTimeSpan;
        }
    }
}


