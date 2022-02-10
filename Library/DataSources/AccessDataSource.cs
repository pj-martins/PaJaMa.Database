using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DataSources
{
	public class AccessOdbcDataSource : OdbcDataSource
	{
		public AccessOdbcDataSource(DatabaseConnection connection) : base(connection)
		{
		}

		private List<ColumnType> _columnTypes;
		public override List<ColumnType> ColumnTypes
		{
			get
			{
				if (_columnTypes == null)
				{
					_columnTypes = new List<ColumnType>();
                    _columnTypes.Add(new ColumnType("VARCHAR", DataType.VarChar, ""));
                    _columnTypes.Add(new ColumnType("INTEGER", DataType.Integer, "0"));
                    _columnTypes.Add(new ColumnType("COUNTER", DataType.Integer, "0"));
				}
				return _columnTypes;
			}
		}
	}
}
