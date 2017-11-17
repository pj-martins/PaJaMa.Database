using PaJaMa.Common;
using PaJaMa.Database.Library.Helpers;
using PaJaMa.Database.Library.Synchronization;
using PaJaMa.Database.Library.Workspaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public class Table : DatabaseObjectWithExtendedProperties
	{
		//private DataTable _removedKeys;

		public string TableName { get; set; }

		[Ignore]
		public List<Column> Columns { get; private set; }

		[Ignore]
		public List<Index> Indexes { get; private set; }

		[Ignore]
		public List<KeyConstraint> KeyConstraints { get; private set; }

		[Ignore]
		public List<ForeignKey> ForeignKeys { get; private set; }

		[Ignore]
		public List<DefaultConstraint> DefaultConstraints { get; private set; }

		[Ignore]
		public List<Trigger> Triggers { get; private set; }

		public override string ObjectName
		{
			get { return TableName; }
		}

		public Table(Database database) : base(database)
		{
			Columns = new List<Column>();
			Indexes = new List<Index>();
			KeyConstraints = new List<KeyConstraint>();
			ForeignKeys = new List<ForeignKey>();
			DefaultConstraints = new List<DefaultConstraint>();
			Triggers = new List<Trigger>();
		}

		internal override void setObjectProperties(DbDataReader reader)
		{
			if (reader["Definition"] != DBNull.Value)
				this.Definition = reader["Definition"].ToString();
			this.Schema = ParentDatabase.Schemas.First(s => s.SchemaName == reader["SchemaName"].ToString());
			if (ParentDatabase.ExtendedProperties != null)
				this.ExtendedProperties = ParentDatabase.ExtendedProperties.Where(ep => ep.Level1Object == this.TableName && ep.ObjectSchema == this.Schema.SchemaName &&
				string.IsNullOrEmpty(ep.Level2Object)).ToList();
			this.Schema.Tables.Add(this);
		}

		private List<ForeignKey> getForeignKeys()
		{
			var fks = this.ForeignKeys.ToList();
			fks.AddRange(from s in this.Schema.ParentDatabase.Schemas
						 from t in s.Tables
						 where t.TableName != this.TableName
						 from fk in t.ForeignKeys
						 where fk.ParentTable.TableName == this.TableName
						 select fk);

			return fks;
		}

		public void ResetIndexes()
		{
			foreach (var ix in Indexes)
			{
				ix.HasBeenDropped = false;
			}
		}

		public void RemoveIndexes(IDbCommand cmd)
		{
			foreach (var ix in Indexes)
			{
				cmd.CommandText = new IndexSynchronization(ParentDatabase, ix).GetRawDropText();
				cmd.ExecuteNonQuery();
				ix.HasBeenDropped = true;
			}
		}

		public void AddIndexes(IDbCommand cmd)
		{
			foreach (var ix in Indexes)
			{
				if (!ix.HasBeenDropped)
					continue;

				var items = new IndexSynchronization(ParentDatabase, ix).GetCreateItems();
				foreach (var item in items)
				{
					foreach (var script in item.Scripts.Where(s => s.Value.Length > 0).OrderBy(s => (int)s.Key))
					{
						cmd.CommandText = script.Value.ToString();
						cmd.CommandTimeout = 1200;
						cmd.ExecuteNonQuery();
					}
				}
				ix.HasBeenDropped = false;
			}
		}

		public void ResetForeignKeys()
		{
			foreach (var fk in getForeignKeys())
			{
				fk.HasBeenDropped = false;
			}
		}

		public void RemoveForeignKeys(IDbCommand cmd)
		{
			foreach (var fk in getForeignKeys())
			{
				if (fk.HasBeenDropped)
					continue;

				cmd.CommandText = new ForeignKeySynchronization(ParentDatabase, fk).GetRawDropText();
				cmd.ExecuteNonQuery();
				fk.HasBeenDropped = true;
			}
		}

		public void AddForeignKeys(IDbCommand cmd)
		{
			foreach (var fk in getForeignKeys())
			{
				if (!fk.HasBeenDropped)
					continue;

				var items = new ForeignKeySynchronization(ParentDatabase, fk).GetCreateItems();
				foreach (var item in items)
				{
					foreach (var script in item.Scripts.Where(s => s.Value.Length > 0).OrderBy(s => (int)s.Key))
					{
						cmd.CommandText = script.Value.ToString();
						cmd.CommandTimeout = 1200;
						cmd.ExecuteNonQuery();
					}
				}
				fk.HasBeenDropped = false;
			}
		}

		public void TruncateDelete(Database targetDatabase, IDbCommand cmd, bool truncate)
		{
			if (truncate)
			{
				cmd.CommandText = string.Format("truncate table {0}.{1}", Schema.GetQueryObjectName(targetDatabase.DataSource), GetQueryObjectName(targetDatabase.DataSource));
				cmd.ExecuteNonQuery();
			}
			else
			{
				cmd.CommandText = string.Format("delete from {0}.{1}", Schema.GetQueryObjectName(targetDatabase.DataSource), GetQueryObjectName(targetDatabase.DataSource));
				cmd.ExecuteNonQuery();
			}
		}

		public override string ToString()
		{
			return Schema == null || string.IsNullOrEmpty(Schema.SchemaName) ? TableName : Schema.SchemaName + "." + TableName;
		}
	}
}
