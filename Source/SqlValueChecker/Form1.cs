using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace SqlSyntaxChecker {
    public partial class Form1 : Form {
        private string noticeMessage = "";

        public Form1() {
            InitializeComponent();
        }

        private void btnCheck_Click(object sender, EventArgs e) {
            try {
                noticeMessage = "";

                using (var conn = new SqlConnection(string.Format(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString, cbxDatabaseName.SelectedItem))) {
                    conn.Open();

                    var insertPattern = new Regex(@"INSERT\s+INTO\s+(.+?)\s+\((.+)\)\s+VALUES\s+\((.+)\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var updatePattern = new Regex(@"UPDATE\s+(.+)\s+SET\s+(.+)\s+WHERE\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var currentSqlSyntaxIndex = 0;


                    var sqlSyntaxes = txtSqlSyntax.Text.Split(';');
                    for (currentSqlSyntaxIndex = 0; currentSqlSyntaxIndex < sqlSyntaxes.Length; currentSqlSyntaxIndex++) {
                        var sqlSyntax = sqlSyntaxes[currentSqlSyntaxIndex];
                        if (sqlSyntax != "") {
                            Match matchedResult;
                            var sqlSyntaxType = "";

                            var sqlSyntaxString = sqlSyntax.Trim();
                            if (sqlSyntaxString.StartsWith("INSERT", StringComparison.InvariantCultureIgnoreCase) == true) {
                                sqlSyntaxType = "INSERT";
                                matchedResult = insertPattern.Match(sqlSyntaxString);
                            }
                            else if (sqlSyntaxString.StartsWith("UPDATE", StringComparison.InvariantCultureIgnoreCase) == true) {
                                sqlSyntaxType = "UPDATE";
                                matchedResult = updatePattern.Match(sqlSyntaxString);
                            }
                            else {
                                throw new Exception("指令類型只限INSERT/UPDATE");
                            }

                            var matchedGroup1 = matchedResult.Groups[1].Value;
                            var tableNames = matchedGroup1.Split('.');
                            var schemaName = GetSchemaName(tableNames);
                            var tableName = GetTableName(tableNames);

                            var matchedGroup2 = matchedResult.Groups[2].Value;
                            var columnNames = ParseColumnNames(matchedGroup2);

                            var matchedGroup3 = "";
                            if (sqlSyntaxType == "INSERT") {
                                matchedGroup3 = matchedResult.Groups[3].Value;
                            }
                            else {
                                matchedGroup3 = matchedGroup2;
                            }

                            var fieldValues = ParseFieldValues(matchedGroup3);

                            for (var currentColumnIndex = 0; currentColumnIndex < columnNames.Count; currentColumnIndex++) {
                                var columnName = columnNames[currentColumnIndex];
                                var fieldValue = fieldValues[currentColumnIndex];

                                DataTable columnInformation = GetColumnInformation(schemaName, tableName, columnName, conn);
                                if (columnInformation == null) throw new Exception("第" + (currentSqlSyntaxIndex + 1).ToString() + "筆SQL指令,找不到欄位" + "[" + columnName + "]" + "資訊");

                                var columnDataType = GetColumnDataType(columnInformation.Rows[0][0]);
                                var maxColumnLength = GetMaxColumnLength(columnInformation.Rows[0][1]);
                                var isColumnNullabe = GetIsColumnNullabe(columnInformation.Rows[0][2]);

                                if (fieldValue == null && isColumnNullabe == false) {
                                    noticeMessage += "第" + (currentSqlSyntaxIndex + 1).ToString() + "筆SQL指令,欄位" + "[" + columnName + "]" + "不可為NULL" + Environment.NewLine; continue;
                                }

                                if (maxColumnLength != -1) {
                                    noticeMessage += GetNoticeMessage(columnName, fieldValue, columnDataType, maxColumnLength, (currentSqlSyntaxIndex + 1), conn);
                                }
                            }
                        }
                    }
                }

                MessageBox.Show(noticeMessage != "" ? noticeMessage : "檢查所有SQL指令欄位寫入值長度皆正常");
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private DataTable GetColumnInformation(string schemaName, string tableName, string columnName, SqlConnection conn) {
            var columnInformation = GetTableContent(String.Format(@"
                                    SELECT
                                        DATA_TYPE
                                        ,CHARACTER_MAXIMUM_LENGTH
                                        ,IS_NULLABLE
                                    FROM INFORMATION_SCHEMA.COLUMNS
                                    WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}';
                                    ", schemaName, tableName, columnName.Replace("'", "''")), conn);
            if (columnInformation.Rows.Count > 0) {
                return columnInformation;
            }
            else {
                throw new Exception("無法取得DB" + "[" + cbxDatabaseName.SelectedItem + "]" + ",SCHEMA" + "[" + schemaName + "]" + ",TABLE" + "[" + tableName + "]" + ",欄位" + "[" + columnName + "]資訊");
            }
        }

        private string GetNoticeMessage(string columnName, string fieldValue, string columnDataType, int maxColumnLength, int sqlSyntaxCount, SqlConnection conn) {
            try {
                var result = "";

                var checkDataLength = GetTableContent(String.Format(@"
            SELECT DATALENGTH(CAST('{0}' AS {1})) AS STRING_LENGTH;
            ", fieldValue, columnDataType), conn);

                var dataLegth = int.Parse(checkDataLength.Rows[0][0].ToString());
                if (columnDataType.Contains("char") == true || columnDataType.Contains("binary") == true) {
                    if (columnDataType.StartsWith("n") == true) {
                        dataLegth = dataLegth / 2;
                    }

                    if (dataLegth > maxColumnLength && maxColumnLength != 0) {
                        result += "第" + sqlSyntaxCount.ToString() + "筆SQL指令,欄位" + "[" + columnName + "]" + ",值" + "[" + GetShortText(fieldValue, 20) + "]" + "太長" + "(欄位長度[" + maxColumnLength + "],實際長度[" + dataLegth + "])" + Environment.NewLine;
                    }
                }

                if (columnDataType == "bit" || columnName.Contains("int") == true || columnName == "smallmoney") {
                    var parsedValue = decimal.Parse(fieldValue);

                    switch (columnDataType) {
                        case "bit":
                            if (parsedValue < 0 || parsedValue > 1) {
                                result += "第" + sqlSyntaxCount.ToString() + "筆SQL指令,欄位" + "[" + columnName + "]" + ",值" + "[" + GetShortText(fieldValue, 20) + "]" + "太長" + "(欄位上下限[0~1])" + Environment.NewLine;
                            }
                            break;
                        case "tinyint":
                            if (parsedValue < 0 || parsedValue > 255) {
                                result += "第" + sqlSyntaxCount.ToString() + "筆SQL指令,欄位" + "[" + columnName + "]" + ",值" + "[" + GetShortText(fieldValue, 20) + "]" + "太長" + "(欄位上下限[0~255])" + Environment.NewLine;
                            }
                            break;
                        case "smallint":
                            if (parsedValue < -32768 || parsedValue > 32767) {
                                result += "第" + sqlSyntaxCount.ToString() + "筆SQL指令,欄位" + "[" + columnName + "]" + ",值" + "[" + GetShortText(fieldValue, 20) + "]" + "太長" + "(欄位上下限[-32768~32767])" + Environment.NewLine;
                            }
                            break;
                        case "int":
                            if (parsedValue < -2147483648 || parsedValue > 2147483647) {
                                result += "第" + sqlSyntaxCount.ToString() + "筆SQL指令,欄位" + "[" + columnName + "]" + ",值" + "[" + GetShortText(fieldValue, 20) + "]" + "太長" + "(欄位上下限[-2147483648~2147483647])" + Environment.NewLine;
                            }
                            break;

                        case "smallmoney":
                            if (parsedValue < (decimal)-214748.3648 || parsedValue > (decimal)214748.3647) {
                                result += "第" + sqlSyntaxCount.ToString() + "筆SQL指令,欄位" + "[" + columnName + "]" + ",值" + "[" + GetShortText(fieldValue, 20) + "]" + "太長" + "(欄位上下限[-214748.3648~214748.3647])" + Environment.NewLine;
                            }
                            break;
                    }
                }

                return result;
            }
            catch (Exception ex) {
                throw new Exception("第" + sqlSyntaxCount.ToString() + "筆SQL指令,欄位" + "[" + columnName + "]" + "錯誤[" + ex.Message + "]");
            }
        }

        private string GetShortText(string text, int limitCount) {
            if (text.Length > limitCount) {
                return text.Substring(0, limitCount) + "...";
            }
            else {
                return text;
            }

        }

        private string GetColumnDataType(object fieldValue) {
            if (fieldValue.ToString().Contains("varchar") == true || fieldValue.ToString().Contains("varbinary") == true) {
                return fieldValue.ToString() + "(MAX)";
            }
            else if (fieldValue.ToString().Contains("char") == true || fieldValue.ToString().Contains("binary") == true) {
                return fieldValue.ToString() + "(8000)";
            }
            else {
                return fieldValue.ToString();
            }
        }

        private bool GetIsColumnNullabe(object fieldValue) {
            return fieldValue.ToString() == "YES" ? true : false;
        }

        private int GetMaxColumnLength(object fieldValue) {
            return fieldValue != DBNull.Value ? int.Parse(fieldValue.ToString()) : 0;
        }

        private List<string> ParseColumnNames(string columnNameString) {
            List<string> result = new List<string>();

            var sqlParser = new TSql120Parser(false);
            IList<ParseError> errors;

            using (var reader = new System.IO.StringReader(columnNameString)) {
                var queryTokens = sqlParser.GetTokenStream(reader, out errors);

                var tokens = queryTokens.Where(q => q.TokenType == TSqlTokenType.QuotedIdentifier || q.TokenType == TSqlTokenType.AsciiStringOrQuotedIdentifier || q.TokenType == TSqlTokenType.Identifier);

                foreach (var token in tokens) {
                    result.Add(GetColumnName(token.Text));
                }

                return result;
            }
        }

        private List<string> ParseFieldValues(string fieldValueString) {
            List<string> result = new List<string>();

            var sqlParser = new TSql120Parser(false);
            IList<ParseError> errors;

            using (var reader = new System.IO.StringReader(fieldValueString)) {
                var queryTokens = sqlParser.GetTokenStream(reader, out errors);

                var tokens = queryTokens.Where(q => q.TokenType == TSqlTokenType.AsciiStringLiteral || q.TokenType == TSqlTokenType.Null || q.TokenType == TSqlTokenType.Identifier || q.TokenType == TSqlTokenType.Integer || q.TokenType == TSqlTokenType.Numeric || q.TokenType == TSqlTokenType.UnicodeStringLiteral);

                foreach (var token in tokens) {
                    result.Add(GetFieldText(token.Text));
                }

                return result;
            }
        }

        private string GetFieldText(string fieldValue) {
            return fieldValue.ToUpper() != "NULL" ? RemoveFirstOrLastChar(fieldValue, both: "'") : null;
        }

        private string GetColumnName(string columnName) {
            return RemoveFirstOrLastChar(RemoveFirstOrLastChar(RemoveFirstOrLastChar(columnName.Trim(), both: @""""), first: "["), last: "]");
        }

        private string GetSchemaName(string[] tableNames) {
            switch (tableNames.Length) {
                case 4:
                    return GetColumnName(tableNames[2]);
                case 3:
                    return GetColumnName(tableNames[1]);
                case 2:
                    return GetColumnName(tableNames[0]);
                case 1:
                    return "dbo";
                default:
                    throw new Exception("Table名稱組成架構異常(應為[Server Name].[DataBase Name].[Schema].[Table Name])");
            }
        }

        private string GetTableName(string[] tableNames) {
            switch (tableNames.Length) {
                case 4:
                    return GetColumnName(tableNames[3]);
                case 3:
                    return GetColumnName(tableNames[2]);
                case 2:
                    return GetColumnName(tableNames[1]);
                case 1:
                    return GetColumnName(tableNames[0]);
                default:
                    throw new Exception("Table名稱組成架構異常(應為[Server Name].[DataBase Name].[Schema].[Table Name])");
            }
        }

        private string RemoveFirstOrLastChar(string text, string both = "", string first = "", string last = "") {
            if (both.Length > 1 || first.Length > 1 || last.Length > 1) throw new Exception("");

            if (both != "") {
                first = both;
                last = both;
            }

            var result = text;

            if (result.IndexOf(first) != -1 && first != "") {
                result = result.Substring(1, result.Length - 1);
            }

            if (result.LastIndexOf(last) != -1 && last != "") {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }

        private void Form1_Load(object sender, EventArgs e) {
            try {
                using (var conn = new SqlConnection(string.Format(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString, "master"))) {
                    conn.Open();

                    var tableList = GetTableContent("SELECT name FROM master.sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');", conn);

                    if (tableList != null) {
                        foreach (DataRow row in tableList.Rows) {
                            var tableName = row["name"].ToString();
                            cbxDatabaseName.Items.Add(tableName);
                        }

                        cbxDatabaseName.SelectedIndex = 0;
                    }
                    else {
                        throw new Exception("無法取得Database名稱列表");
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }

        }

        /// <summary>
        /// 取得Table內容
        /// </summary>
        /// <param name="sqlSyntax">SQL語法</param>
        /// <param name="conn">SQL連線</param>
        /// <returns>回傳結果</returns>
        private DataTable GetTableContent(string sqlSyntax, SqlConnection conn) {
            var dt = new DataTable();
            var command = new SqlCommand(sqlSyntax, conn);
            var reader = command.ExecuteReader();
            dt.Load(reader);

            return dt;
        }

        private void txtSqlSyntax_TextChanged(object sender, EventArgs e) {
            if (txtSqlSyntax.Text != "") {
                btnCheck.Enabled = true;
            }
            else {
                btnCheck.Enabled = false;
            }
        }

        private void txtSqlSyntax_DoubleClick(object sender, EventArgs e) {
            txtSqlSyntax.Text = "";
        }
    }
}