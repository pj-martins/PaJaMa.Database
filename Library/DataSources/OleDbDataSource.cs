﻿using PaJaMa.Database.Library.DatabaseObjects;
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
		public OleDbDataSource(DatabaseConnection connection) : base(connection)
		{
		}

		public override string DefaultSchemaName => throw new NotImplementedException();

		internal override string DatabaseSQL => "";
		internal override string ExtendedPropertySQL => "";
		internal override string CredentialSQL => "";
		internal override string DatabasePrincipalSQL => "";
		internal override string PermissionSQL => "";
		internal override string ServerLoginSQL => "";
		internal override string RoutineSynonymSQL => "";
		internal override string DefaultConstraintSQL => "";

		internal override string SchemaSQL => "";

		internal override string ViewSQL => throw new NotImplementedException();

		internal override string TableSQL => throw new NotImplementedException();

		internal override string ColumnSQL => throw new NotImplementedException();

		internal override string ForeignKeySQL => throw new NotImplementedException();

		internal override string KeyConstraintSQL => throw new NotImplementedException();

		internal override string IndexSQL => throw new NotImplementedException();

		internal override string TriggerSQL => throw new NotImplementedException();

		protected override Type connectionType => typeof(OleDbConnection);

		private List<ColumnType> _columnTypes;
		public override List<ColumnType> ColumnTypes
		{
			get
			{
				if (_columnTypes == null)
				{
					_columnTypes = new List<ColumnType>();
				}
				return _columnTypes;
			}
		}

		public override List<string> SurroundingCharacters => throw new NotImplementedException();

		internal override string CombinedSQL => throw new NotImplementedException();

		public override string GetConvertedObjectName(string objectName)
		{
			throw new NotImplementedException();
		}

		internal override string GetColumnAddAlterScript(Column column, Column targetColumn, string defaultValue, string postScript)
		{
			throw new NotImplementedException();
		}

		public override string GetPreTopN(int topN)
		{
			return topN <= 0 ? string.Empty : string.Format("TOP {0}", topN);
		}


	}
}
