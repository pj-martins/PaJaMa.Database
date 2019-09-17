using PaJaMa.Common;
using PaJaMa.Database.DataGenerate.Content;
using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Workspaces.Generate
{
	public class ColumnWorkspace
	{
		public Column Column { get; private set; }
		public ContentBase Content { get; set; }

		public ColumnWorkspace(Column column)
		{
			Column = column;

			if (Column.IsIdentity || !string.IsNullOrEmpty(Column.Formula))
				return;

			var fks = from s in Column.Parent.Schema.Database.Schemas
					  from t in s.Tables
					  from fk in t.ForeignKeys
					  where fk.ChildTable.TableName == Column.Parent.ObjectName
					   && fk.Columns.Any(c => c.ChildColumn.ColumnName == Column.ColumnName)
					  select fk;

			SqlDbType type = SqlDbType.UniqueIdentifier;
			Enum.TryParse(column.ColumnType.TypeName, true, out type);

			if (fks.Any())
			{
				var fk = fks.First();
				Content = new ForeignKeyContent(fk, type);
			}
			else
			{
				var clrType = PaJaMa.Common.DataHelper.GetClrType(type);

				if (Column.Parent is Table && (Column.Parent as Table).KeyConstraints.Any(k => k.Columns.Any(c => c.ColumnName == Column.ColumnName)) && clrType.IsNumericType())
					Content = new KeyContent((Column.Parent as Table).KeyConstraints.First(k => k.Columns.Any(c => c.ColumnName == Column.ColumnName)), type);
				else if (type == SqlDbType.Timestamp)
					return;
				else if (clrType.Equals(typeof(DateTime)) || clrType.Equals(typeof(DateTime?)))
					Content = new DateTimeContent(type);
				else if (clrType.Equals(typeof(DateTimeOffset)) || clrType.Equals(typeof(DateTimeOffset?)))
					Content = new DateTimeContent(type);
				else if (clrType.Equals(typeof(String)))
					Content = new StringContent(type);
				else if (clrType.Equals(typeof(byte[])))
					Content = new StringContent(type);
				else if (clrType.IsNumericType())
					Content = new NumericContent(type);
				else if (clrType.Equals(typeof(Guid)) || clrType.Equals(typeof(Guid?)))
					Content = new GuidContent(type);
				else if (clrType.Equals(typeof(bool)) || clrType.Equals(typeof(bool?)))
					Content = new BoolContent(type);
				else
					Content = new NoContent(type);
			}
		}
	}
}
