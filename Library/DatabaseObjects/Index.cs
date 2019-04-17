using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
    public class Index : DatabaseObjectBase
    {
		public Index(Database database) : base(database)
		{
		}

		public Table Table { get; set; }
        public string IndexName { get; set; }
		public string TableName { get; set; }
        public string IndexType { get; set; }
        public bool IsUnique { get; set; }

        public bool HasBeenDropped { get; set; }

        public List<IndexColumn> IndexColumns { get; set; }

        public override string ObjectName
        {
            get { return IndexName; }
        }

		internal override void setObjectProperties(DbDataReader reader)
		{

			var schema = Database.Schemas.First(s => s.SchemaName == reader["SchemaName"].ToString());
			var table = schema.Tables.FirstOrDefault(t => t.TableName == TableName);
			if (table == null) return;
			var index = table.Indexes.FirstOrDefault(i => i.IndexName == IndexName && i.Table.TableName == TableName && i.Table.Schema.SchemaName
				== schema.SchemaName);
			if (index == null)
			{
				index = this;
				index.IndexColumns = new List<IndexColumn>();

				// stale index?
				index.Table = table;
				table.Indexes.Add(index);
			}
			var indexCol = reader.ToObject<IndexColumn>();
			index.IndexColumns.Add(indexCol);
		}
    }

    public class IndexColumn
    {
        public string ColumnName { get; set; }
        public bool Descending { get; set; }
        [Ignore]
        public Int64 Descending2
        {
            get { return Descending ? 1 : 0; }
            set { Descending = value == 1; }
        }
        public int Ordinal { get; set; }
        public Int64 Ordinal2
        {
            get { return Ordinal; }
            set { Ordinal = Convert.ToInt32(value); }
        }
    }
}
