﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Paradigm.ORM.Data.Cassandra.Converters;
using Paradigm.ORM.Data.Database.Schema.Structure;

namespace Paradigm.ORM.Data.Cassandra.Schema
{
    public partial class CqlSchemaProvider 
    {
        #region Public Methods

        /// <summary>
        /// Gets the schema of database tables specifying the database, and allowing to filter which tables to return.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <param name="filter">An array of table names you want to retrieve.</param>
        /// <returns>
        /// A list of table schemas.
        /// </returns>
        public async Task<List<ITable>> GetTablesAsync(string database, params string[] filter)
        {
            return (await this.TableQuery.ExecuteAsync(this.GetTableWhere(database, TableType, filter)))
                .Where(x => x.Type == "Standard" && (filter == null || filter.Length == 0 || filter.Contains(x.Name)))
                .Cast<ITable>().ToList();
        }

        /// <summary>
        /// Gets the schema of database views specifying the database, and allowing to filter which views to return.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <param name="filter">An array of view names you want to retrieve.</param>
        /// <returns>
        /// A list of view schemas.
        /// </returns>
        public async Task<List<IView>> GetViewsAsync(string database, params string[] filter)
        {
            return (await this.ViewQuery.ExecuteAsync(this.GetTableWhere(database, ViewType, filter)))
                .Where(x => x.Type == "View" && (filter == null || filter.Length == 0 || filter.Contains(x.Name)))
                .Cast<IView>().ToList();
        }

        /// <summary>
        /// Gets the schema of stored procedures specifying the database, and allowing to filter which stored procedures to return.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <param name="filter">An array of stored procedure names you want to retrieve.</param>
        /// <returns>
        /// A list of stored procedure schemas.
        /// </returns>
        public Task<List<IStoredProcedure>> GetStoredProceduresAsync(string database, params string[] filter)
        {
            return Task.FromResult(new List<IStoredProcedure>());
        }

        /// <summary>
        /// Gets the schema of all the columns of a table.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <param name="tableName">The table name.</param>
        /// <returns>
        /// A list of column schemas.
        /// </returns>
        public async Task<List<IColumn>> GetColumnsAsync(string database, string tableName)
        {
            var columns = (await this.ColumnQuery.ExecuteAsync($"\"keyspace_name\"='{database}' AND \"columnfamily_name\"='{tableName}'")).ToList();

            foreach (var column in columns)
            {
                column.DataType = CqlDbStringTypeConverter.ValidatorToDbType(column.DataType);
            }

            return columns.Cast<IColumn>().ToList();
        }

        /// <summary>
        /// Gets the schema of all the contraints of a table.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <param name="tableName">The table name.</param>
        /// <returns>
        /// A list of constraint schemas.
        /// </returns>
        public async Task<List<IConstraint>> GetConstraintsAsync(string database, string tableName)
        {
            var constraints = (await this.ConstraintQuery
                .ExecuteAsync($"\"keyspace_name\"='{database}' AND \"columnfamily_name\"='{tableName}'"))
                .Where(x => x.ColumnType == "partition_key")
                .ToList();

            foreach (var constraint in constraints)
            {
                constraint.Type = ConstraintType.PrimaryKey;
                constraint.FromColumnName = constraint.Name;
            }

            return constraints.Cast<IConstraint>().ToList();
        }

        /// <summary>
        /// Gets the schema of all the parameters of a stored procedure.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <param name="routineName">The routine name.</param>
        /// <returns>
        /// A list of parameter schemas.
        /// </returns>
        public Task<List<IParameter>> GetParametersAsync(string database, string routineName)
        {
            return Task.FromResult(new List<IParameter>());
        }

        #endregion
    }
}