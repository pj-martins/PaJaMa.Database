using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.DataSources;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Studio.Classes
{
	public class IntellisenseHelper
	{
		public const string PATTERN = "(.* |^|\n)([A-Za-z\\.\\[\\]\"`]+)$";

		private DataSource _dataSource;
		public IntellisenseHelper(DataSource dataSource)
		{
			_dataSource = dataSource;
		}

		private List<IntellisenseMatch> getFromsAndJoinsMatches(string text, DbConnection connection)
		{
			var matches = new List<IntellisenseMatch>();
			var toCheck = text.Split(new string[] { " ", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < toCheck.Length - 1; i++)
			{
				if (toCheck[i].ToLower() == "from" || toCheck[i].ToLower() == "join")
				{
					var obj = toCheck[i + 1];
					var parts = obj.Split('.');
					var db = parts.Length > 1 ? _dataSource.Databases.FirstOrDefault(d => d.DatabaseName.ToLower() == parts[0].ToLower()) : _dataSource.CurrentDatabase;
					if (db.Schemas == null) _dataSource.PopulateSchemas(connection, db);
					var noSchema = db.Schemas.Count == 1 && string.IsNullOrEmpty(db.Schemas[0].SchemaName);
					var schema = db.Schemas[0];
					if (noSchema)
					{
						if (!schema.Tables.Any()) _dataSource.PopulateTables(connection, new Schema[] { schema }, false);
						var table = schema.Tables.FirstOrDefault(t => t.TableName.ToLower() == parts.Last().ToLower());
						if (table != null)
						{
							if (!table.Columns.Any()) _dataSource.PopulateColumnsForTable(connection, table);
							matches.AddRange(table.Columns.OrderBy(c => c.ColumnName).Select(c => new IntellisenseMatch(c.ColumnName, $"{table.TableName}.{c.ColumnName}")));
						}
					}
				}
			}
			return matches;
		}

		public List<IntellisenseMatch> GetIntellisenseMatches(string text, int position, DbConnection connection)
		{
			var startText = text.Substring(0, position);
			var partial = string.Empty;
			var match = Regex.Match(startText, PATTERN);
			partial = match.Groups[2].Value;
			string[] periodParts = null;
			if (partial.Length > 0)
			{
				periodParts = partial.Split('.');
				partial = periodParts.Last();
			}
			var matches = new List<IntellisenseMatch>();
			if (periodParts != null && periodParts.Length > 1)
			{
				partial = periodParts.Last();
				var selectedDb = _dataSource.Databases.FirstOrDefault(d => d.DatabaseName.ToLower() == periodParts[0].ToLower());
				if (selectedDb != null)
				{
					if (selectedDb.Schemas == null) _dataSource.PopulateSchemas(connection, selectedDb);
					var noSchema = selectedDb.Schemas.Count == 1 && string.IsNullOrEmpty(selectedDb.Schemas[0].SchemaName);
					if (periodParts.Length > 2 && !noSchema)
					{

					}
					else if (!noSchema)
					{
						matches.AddRange(selectedDb.Schemas.OrderBy(s => s.SchemaName).Select(s => new IntellisenseMatch(s.SchemaName, $"{s.Database.DatabaseName}.{s.Schema.SchemaName}")));
					}
					else
					{
						if (!selectedDb.Schemas[0].Tables.Any()) _dataSource.PopulateTables(connection, selectedDb.Schemas.ToArray(), false);
						matches.AddRange(selectedDb.Schemas[0].Tables.OrderBy(t => t.TableName).Select(t => new IntellisenseMatch(t.TableName,
							$"{t.Database.DatabaseName}.{(noSchema ? "" : $"{t.Schema.SchemaName}.")}{t.TableName}")));
					}
				}

				if (!_dataSource.CurrentDatabase.Schemas[0].Tables.Any()) _dataSource.PopulateTables(connection, new Schema[] { _dataSource.CurrentDatabase.Schemas[0] }, false);
				matches.AddRange(getFromsAndJoinsMatches(text, connection));
			}
			else
			{
				var items = _dataSource.GetReservedKeywords().OrderBy(k => k).Select(k => new IntellisenseMatch(k, k)).ToList();
				items.AddRange(_dataSource.Databases.OrderBy(d => d.DatabaseName).Select(d => new IntellisenseMatch(d.DatabaseName, d.DatabaseName)));
				matches.AddRange(items);
				if (_dataSource.CurrentDatabase.Schemas == null) _dataSource.PopulateSchemas(connection, _dataSource.CurrentDatabase);
				var noSchema = _dataSource.CurrentDatabase.Schemas.Count == 1 && string.IsNullOrEmpty(_dataSource.CurrentDatabase.Schemas[0].SchemaName);
				if (!noSchema)
				{
					matches.AddRange(_dataSource.CurrentDatabase.Schemas.OrderBy(s => s.SchemaName).Select(s => new IntellisenseMatch(s.SchemaName, $"{s.Database.DatabaseName}.{s.Schema.SchemaName}")));
				}
				else
				{
					if (!_dataSource.CurrentDatabase.Schemas[0].Tables.Any()) _dataSource.PopulateTables(connection, new Schema[] { _dataSource.CurrentDatabase.Schemas[0] }, false);
					matches.AddRange(_dataSource.CurrentDatabase.Schemas[0].Tables.OrderBy(t => t.TableName).Select(t => new IntellisenseMatch(t.TableName,
							$"{t.Database.DatabaseName}.{(noSchema ? "" : $"{t.Schema.SchemaName}.")}{t.TableName}")));
				}

				matches.AddRange(getFromsAndJoinsMatches(text, connection));
			}

			if (!string.IsNullOrEmpty(partial))
			{
				matches = matches.Where(m => m.ShortName.ToLower().StartsWith(partial.ToLower())).ToList();
			}

			return matches;
		}
	}

	public class IntellisenseMatch
	{
		public string ShortName { get; private set; }
		public string Description { get; private set; }
		public IntellisenseMatch(string shortName, string description)
		{
			this.ShortName = shortName;
			this.Description = description;
		}

		public override string ToString()
		{
			return this.Description;
		}
	}
}
