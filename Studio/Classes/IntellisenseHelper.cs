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
		public const string PATTERN = "(.* |.*\t|^|\n|\\()([A-Za-z0-9\\.\\[\\]\"_`]+)$";

		private DataSource _dataSource;
		public IntellisenseHelper(DataSource dataSource)
		{
			_dataSource = dataSource;
		}

		private string stripSurroundingChars(string input)
		{
			string output = input;
			foreach (var surr in _dataSource.SurroundingCharacters)
			{
				output = output.Replace(surr, "");
			}
			return output;
		}

		private List<IntellisenseMatch> getFromsAndJoinsMatches(string text, DbConnection connection)
		{
			var matches = new List<IntellisenseMatch>();
			var toCheck = text.Split(new string[] { " ", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < toCheck.Length - 1; i++)
			{
				if (toCheck[i].ToLower() == "from" || toCheck[i].ToLower() == "join" || toCheck[i].ToLower() == "update"
					|| toCheck[i].ToLower() == "insert" || toCheck[i].ToLower() == "into")
				{
					var obj = toCheck[i + 1];
					var parts = obj.Split('.');
					Schema schema = null;
					Library.DatabaseObjects.Database db = null;
					if (parts.Length > 1) db = _dataSource.Databases.FirstOrDefault(d => d.DatabaseName.ToLower() == stripSurroundingChars(parts[0].ToLower()));
					if (db == null) db = _dataSource.CurrentDatabase;
					if (db != null)
					{
						if (db.Schemas == null) _dataSource.PopulateSchemas(connection, db);
						var noSchema = db.Schemas.Count == 1 && string.IsNullOrEmpty(db.Schemas[0].SchemaName);
						schema = db.Schemas[0];
						if (!noSchema)
						{
							if (parts.Length > 2) schema = db.Schemas.FirstOrDefault(s => s.SchemaName.ToLower() == (string.IsNullOrEmpty(parts[1]) ? 
								db.DataSource.DefaultSchemaName : stripSurroundingChars(parts[1])));
							if (schema == null && parts.Length > 1) schema = db.Schemas.FirstOrDefault(s => s.SchemaName.ToLower() == stripSurroundingChars(parts[0]));
							if (schema == null) schema = db.Schemas.FirstOrDefault(s => s.SchemaName.ToLower() == db.DataSource.DefaultSchemaName.ToLower());
						}
						if (!schema.Tables.Any()) _dataSource.PopulateTables(connection, new Schema[] { schema }, false);
						var table = schema.Tables.FirstOrDefault(t => t.TableName.ToLower() == stripSurroundingChars(parts.Last().ToLower()));
						if (table != null)
						{
							if (!table.Columns.Any()) _dataSource.PopulateChildColumns(connection, table);
							matches.AddRange(table.Columns.OrderBy(c => c.ColumnName).Select(c => new IntellisenseMatch(_dataSource.GetConvertedObjectName(c.ColumnName), $"{table.TableName}.{c.ColumnName}")));
						}
						// if (!schema.Views.Any()) _dataSource.PopulateViews()
						var view = schema.Views.FirstOrDefault(s => s.ViewName.ToLower() == stripSurroundingChars(parts.Last().ToLower()));
						if (view != null)
						{
							if (!view.Columns.Any()) _dataSource.PopulateChildColumns(connection, view);
							matches.AddRange(view.Columns.OrderBy(c => c.ColumnName).Select(c => new IntellisenseMatch(_dataSource.GetConvertedObjectName(c.ColumnName), $"{view.ViewName}.{c.ColumnName}")));
						}
					}
				}
			}
			return matches;
		}

		private List<TDatabaseObject> getSortObjects<TDatabaseObject>(List<TDatabaseObject> objs, string partial)
		{
			Func<object, string> getValToSort = (o) =>
			{
				if (o is Library.DatabaseObjects.Database d) return d.DatabaseName.ToLower();
				return (o as DatabaseObjectBase).ObjectName.ToLower();
			};
			var clones = objs.OrderBy(o =>
					{
						string shortName = getValToSort(o);
						//_dataSource.SurroundingCharacters.ForEach(s => shortName = shortName.Replace(s, ""));
						if (shortName == partial.ToLower()) return 0;
						return shortName.StartsWith(partial.ToLower()) ? 1 : 2;
					})
					.ThenBy(o => getValToSort(o));
			return clones.ToList();
		}

		public List<IntellisenseMatch> GetIntellisenseMatches(string text, int position, DbConnection connection)
		{
			var startText = text.Substring(0, position);
			var partial = string.Empty;
			var match = Regex.Match(startText, PATTERN, RegexOptions.RightToLeft);
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
				var selectedDb = _dataSource.Databases.FirstOrDefault(d => d.DatabaseName.ToLower() == stripSurroundingChars(periodParts[0].ToLower()));
				if (selectedDb != null)
				{
					if (selectedDb.Schemas == null) _dataSource.PopulateSchemas(connection, selectedDb);
					var noSchema = selectedDb.Schemas.Count == 1 && string.IsNullOrEmpty(selectedDb.Schemas[0].SchemaName);
					if (periodParts.Length > 2 && !noSchema)
					{
						var childSchema = selectedDb.Schemas.FirstOrDefault(s => s.SchemaName == (string.IsNullOrEmpty(periodParts[1]) ? 
							selectedDb.DataSource.DefaultSchemaName : stripSurroundingChars(periodParts[1])));
						if (childSchema != null)
						{
							matches.AddRange(getSortObjects<Table>(childSchema.Tables, partial).Select(t => new IntellisenseMatch(_dataSource.GetConvertedObjectName(t.TableName),
							$"{t.Database.DatabaseName}.{childSchema.SchemaName}.{t.TableName}")));
						}
					}
					else if (!noSchema)
					{
						matches.AddRange(getSortObjects<Schema>(selectedDb.Schemas, partial).Select(s => new IntellisenseMatch(_dataSource.GetConvertedObjectName(s.SchemaName), $"{s.Database.DatabaseName}.{s.SchemaName}")));
					}
					else
					{
						if (!selectedDb.Schemas[0].Tables.Any()) _dataSource.PopulateTables(connection, selectedDb.Schemas.ToArray(), false);
						matches.AddRange(getSortObjects<Table>(selectedDb.Schemas[0].Tables, partial).Select(t => new IntellisenseMatch(_dataSource.GetConvertedObjectName(t.TableName),
							$"{t.Database.DatabaseName}.{(noSchema ? "" : $"{t.Schema.SchemaName}.")}{t.TableName}")));
						if (selectedDb.DatabaseName.ToLower() == "information_schema")
						{
							matches.AddRange(getSortObjects<View>(_dataSource.GetSchemaViews(connection, selectedDb), partial).Select(t => new IntellisenseMatch(_dataSource.GetConvertedObjectName(t.ViewName),
							$"{t.Database.DatabaseName}.{(noSchema ? "" : $"{t.Schema.SchemaName}.")}{t.ViewName}")).Distinct());
						}
					}
				}
				var schema = _dataSource.CurrentDatabase.Schemas.FirstOrDefault(s => s.SchemaName.ToLower() == stripSurroundingChars(periodParts[0].ToLower()));
				if (schema != null)
				{
					matches.AddRange(getSortObjects<Table>(schema.Tables, partial).Select(t => new IntellisenseMatch(_dataSource.GetConvertedObjectName(t.TableName),
							$"{t.Database.DatabaseName}.{schema.SchemaName}.{t.TableName}")));
				}

				if (!_dataSource.CurrentDatabase.Schemas[0].Tables.Any()) _dataSource.PopulateTables(connection, new Schema[] { _dataSource.CurrentDatabase.Schemas[0] }, false);
				matches.AddRange(getFromsAndJoinsMatches(text, connection));
			}
			else
			{
				var items = _dataSource.GetReservedKeywords().OrderBy(k => k).Select(k => new IntellisenseMatch(k, k)).ToList();
				items.AddRange(getSortObjects<Library.DatabaseObjects.Database>(_dataSource.Databases, partial).Select(d => new IntellisenseMatch(_dataSource.GetConvertedObjectName(d.DatabaseName), d.DatabaseName)));
				matches.AddRange(items);
				if (_dataSource.CurrentDatabase.Schemas == null) _dataSource.PopulateSchemas(connection, _dataSource.CurrentDatabase);
				var noSchema = _dataSource.CurrentDatabase.Schemas.Count == 1 && string.IsNullOrEmpty(_dataSource.CurrentDatabase.Schemas[0].SchemaName);
				if (!noSchema)
				{
					matches.AddRange(getSortObjects<Schema>(_dataSource.CurrentDatabase.Schemas, partial).Select(s => new IntellisenseMatch(_dataSource.GetConvertedObjectName(s.SchemaName), $"{s.Database.DatabaseName}.{s.SchemaName}")));
					var defSchema = _dataSource.CurrentDatabase.Schemas.FirstOrDefault(s => s.SchemaName == _dataSource.DefaultSchemaName);
					// if (!defSchema.Tables.Any()) _dataSource.PopulateTables(connection, new Schema[] { defSchema }, false);
					matches.AddRange(getSortObjects<Table>(defSchema.Tables, partial).Select(t => new IntellisenseMatch(_dataSource.GetConvertedObjectName(t.TableName),
							$"{t.Database.DatabaseName}.{(noSchema ? "" : $"{t.Schema.SchemaName}.")}{t.TableName}")));
				}
				else
				{
					if (!_dataSource.CurrentDatabase.Schemas[0].Tables.Any()) _dataSource.PopulateTables(connection, new Schema[] { _dataSource.CurrentDatabase.Schemas[0] }, false);
					matches.AddRange(getSortObjects<Table>(_dataSource.CurrentDatabase.Schemas[0].Tables, partial).Select(t => new IntellisenseMatch(_dataSource.GetConvertedObjectName(t.TableName),
							$"{t.Database.DatabaseName}.{(noSchema ? "" : $"{t.Schema.SchemaName}.")}{t.TableName}")));
				}

				matches.AddRange(getFromsAndJoinsMatches(text, connection));
			}

			if (!string.IsNullOrEmpty(partial))
			{
				matches = matches
					.Where(m => m.ShortName.ToLower().Contains(partial.ToLower()))
					//.OrderBy(m =>
					//{
					//	string shortName = m.ShortName.ToLower();
					//	_dataSource.SurroundingCharacters.ForEach(s => shortName = shortName.Replace(s, ""));
					//	if (shortName == partial.ToLower()) return 0;
					//	return shortName.StartsWith(partial.ToLower()) ? 1 : 2;
					//})
					//.ThenBy(m => m.ShortName)
					.ToList();
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
