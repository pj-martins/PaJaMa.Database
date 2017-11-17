using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects.DataSources
{
	public class MySqlDataSource : DataSource
	{
		public MySqlDataSource(string connectionString) : base(connectionString)
		{
		}

		public override string DefaultSchemaName => throw new NotImplementedException();

		protected override Type connectionType => typeof(MySqlConnection);

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

		internal override List<ColumnType> ColumnTypes => throw new NotImplementedException();

		public override string GetConvertedObjectName(string objectName)
		{
			throw new NotImplementedException();
		}
	}
}
