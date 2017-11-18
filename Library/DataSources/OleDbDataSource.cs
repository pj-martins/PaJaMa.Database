using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DataSources
{
	public class OleDbDataSource : DataSource
	{
		public OleDbDataSource(string connectionString) : base(connectionString)
		{
		}

		public override string DefaultSchemaName => throw new NotImplementedException();

		internal override string SchemaSQL => throw new NotImplementedException();

		internal override string ViewSQL => throw new NotImplementedException();

		internal override string TableSQL => throw new NotImplementedException();

		internal override string ColumnSQL => throw new NotImplementedException();

		internal override string ForeignKeySQL => throw new NotImplementedException();

		internal override string KeyConstraintSQL => throw new NotImplementedException();

		internal override string IndexSQL => throw new NotImplementedException();

		internal override string DefaultConstraintSQL => throw new NotImplementedException();

		internal override string TriggerSQL => throw new NotImplementedException();

		internal override string DatabaseSQL => throw new NotImplementedException();

		protected override Type connectionType => typeof(OleDbConnection);

		internal override List<ColumnType> ColumnTypes => throw new NotImplementedException();

		public override string GetConvertedObjectName(string objectName)
		{
			throw new NotImplementedException();
		}

		internal override string GetColumnAddAlterScript(Column column, bool add, string defaultValue, string postScript)
		{
			throw new NotImplementedException();
		}

		public override string GetPreTopN(int topN)
		{
			return topN <= 0 ? string.Empty : string.Format("TOP {0}", topN);
		}


	}
}
