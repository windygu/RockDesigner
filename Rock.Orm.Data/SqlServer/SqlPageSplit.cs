using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Rock.Orm.Data;
using Rock.Orm.Common;

namespace Rock.Orm.Data.SqlServer
{
	/// <summary>
	/// Sql Page Splitter
	/// </summary>
    [Obsolete]
	public class SqlPageSplit : PageSplit
	{
		#region Private Members

        /// <summary>
        /// Formats the inside params.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns></returns>
        private static string FormatInsideParams(string where)
        {
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"(@[\w\d_]+)");
            return r.Replace(where, "$1Inside", int.MaxValue);
        }

        private string ConstructAndCacheSplitableSelectStatement(string sql, string keyColumn)
        {
            string columnList = string.Empty;
            string fromTable = string.Empty;
            string whereClause = string.Empty;
            string orderByClause = string.Empty;

            //parse input select statement
            string str = sql.Substring(6).TrimStart(new char[] { ' ' });

            int fromBegin = str.ToLower().IndexOf(" from ");
            columnList = str.Substring(0, fromBegin);
            str = str.Substring(fromBegin).TrimStart(new char[] { ' ' });

            if (str.ToLower().IndexOf(" where ") > 0)
            {
                int whereBegin = str.ToLower().IndexOf(" where ");
                fromTable = str.Substring(0, whereBegin);
                str = str.Substring(whereBegin).TrimStart(new char[] { ' ' });

                if (str.ToLower().IndexOf(" order by ") > 0)
                {
                    int orderBegin = str.ToLower().IndexOf(" order by ");
                    whereClause = str.Substring(0, orderBegin);
                    str = str.Substring(orderBegin).TrimStart(new char[] { ' ' });

                    orderByClause = str.TrimEnd(new char[] { ' ', ';' });
                }
                else
                {
                    whereClause = str.TrimEnd(new char[] { ' ', ';' });
                }
            }
            else
            {
                if (str.ToLower().IndexOf(" order by ") > 0)
                {
                    int orderBegin = str.ToLower().IndexOf(" order by ");
                    fromTable = str.Substring(0, orderBegin);
                    str = str.Substring(orderBegin).TrimStart(new char[] { ' ' });

                    orderByClause = str.TrimEnd(new char[] { ' ', ';' });
                }
                else
                {
                    fromTable = str.TrimEnd(new char[] { ' ', ';' });
                }
            }

            if (whereClause != string.Empty)
            {
                _Where = whereClause.Substring(6);
            }
            if (orderByClause != string.Empty)
            {
                _OrderBy = orderByClause.Substring(8);
            }

            string retStr = string.Format("SELECT TOP #PageSize# {0} {1} {2} {3}",
                columnList, fromTable,
                (whereClause.ToLower().StartsWith("where") ? whereClause + " AND " : " WHERE ") +
                string.Format("( [{0}] NOT IN ( SELECT TOP #BeforeCount# [{0}] {1} {2} {3}) )",
                keyColumn, fromTable, FormatInsideParams(whereClause), orderByClause), orderByClause)
                .Replace("#PageSize#", "{0}").Replace("#BeforeCount#", "{1}");

            _SplitableStatementCache.Add(sql, retStr);
            return retStr;
        }

		#endregion

        #region Impl PageSplit Members

        /// <summary>
        /// construct an page splitable select statement from a simple select statement like 
        /// " select [columns] from [table_name] sql [condition] order by [order_list] "
        /// </summary>
        protected override string ConstructPageSplitableSelectStatement(string sql, string keyColumn)
        {
            Check.Require(sql != null && keyColumn != null);

            string retStr;

            if (!_SplitableStatementCache.TryGetValue(sql,out retStr))
            {
                lock (_SplitableStatementCache)
                {
                    if (!_SplitableStatementCache.TryGetValue(sql, out retStr))
                    {
                        retStr = ConstructAndCacheSplitableSelectStatement(sql, keyColumn);
                    }
                }
            }

            return retStr;
        }

        /// <summary>
        /// ConstructPageSplitableSelectStatementForFirstPage
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="keyColumn">keyColumn</param>
        /// <returns></returns>
        protected override string ConstructPageSplitableSelectStatementForFirstPage(string sql, string keyColumn)
        {
            return "SELECT TOP {0}" + sql.Substring(6);
        }

        /// <summary>
        /// ConstructSelectCountStatement
        /// </summary>
        /// <param name="sql">sql</param>
        /// <returns></returns>
        protected override string ConstructSelectCountStatement(string sql)
        {
            string columnList = string.Empty;
            string fromTable = string.Empty;
            string whereClause = string.Empty;
            string orderByClause = string.Empty;

            //parse input select statement
            string str = sql.Substring(6).TrimStart(new char[] { ' ' });

            int fromBegin = str.ToLower().IndexOf(" from ");
            columnList = str.Substring(0, fromBegin);
            str = str.Substring(fromBegin).TrimStart(new char[] { ' ' });

            if (str.ToLower().IndexOf(" where ") > 0)
            {
                int whereBegin = str.ToLower().IndexOf(" where ");
                fromTable = str.Substring(0, whereBegin);
                str = str.Substring(whereBegin).TrimStart(new char[] { ' ' });

                if (str.ToLower().IndexOf(" order by ") > 0)
                {
                    int orderBegin = str.ToLower().IndexOf(" order by ");
                    whereClause = str.Substring(0, orderBegin);
                    str = str.Substring(orderBegin).TrimStart(new char[] { ' ' });

                    orderByClause = str.TrimEnd(new char[] { ' ', ';' });
                }
                else
                {
                    whereClause = str.TrimEnd(new char[] { ' ', ';' });
                }
            }
            else
            {
                if (str.ToLower().IndexOf(" order by ") > 0)
                {
                    int orderBegin = str.ToLower().IndexOf(" order by ");
                    fromTable = str.Substring(0, orderBegin);
                    str = str.Substring(orderBegin).TrimStart(new char[] { ' ' });

                    orderByClause = str.TrimEnd(new char[] { ' ', ';' });
                }
                else
                {
                    fromTable = str.TrimEnd(new char[] { ' ', ';' });
                }
            }

            string retStr = string.Format("SELECT count(*) {0} {1}", fromTable,
                (whereClause.ToLower().StartsWith("where") ? whereClause : ""));

            return retStr;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlPageSplit"/> class.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="selectStatement">The select statement.</param>
        /// <param name="keyColumn">The DEFAULT_KEY column.</param>
        /// <param name="paramValues">The param values.</param>
        public SqlPageSplit(Database db, string selectStatement, string keyColumn, params object[] paramValues)
            : base(db, selectStatement, keyColumn)
		{
			if (paramValues != null && paramValues.Length > 0)
			{
				this.paramValues = new object[paramValues.Length * 2];
				int i = 0;
				foreach (object item in paramValues)
				{
					this.paramValues[i++] = item;
				}
				foreach (object item in paramValues)
				{
					this.paramValues[i++] = item;
				}
			}
		}

		#endregion
	}
}
