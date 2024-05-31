using Npgsql;
using System;
using System.Data.Common;

namespace MyDotNetAPP.Utils
{

    public class PostgreSQL : IDb
    {
        private string _connectionString;
        private NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;
        public PostgreSQL(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task Connect()
        {
            try
            {
                if (_connectionString == null)
                {
                    throw new DbException(DbException.Codes.ConnectionStringNotFound);
                }
                if (_connection == null)
                    _connection = new NpgsqlConnection(_connectionString);

                await _connection.OpenAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task BeginTransaction()
        {
            try
            {
                if (_connection == null)
                {
                    throw new DbException(DbException.Codes.ConnectionNotOpened);
                }
                if (_transaction != null)
                {
                    return;
                }
                _transaction = await _connection.BeginTransactionAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task CommitTransaction()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new DbException(DbException.Codes.TransactionNotBegun);
                }
                await _transaction.CommitAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task RollbackTransaction()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new DbException(DbException.Codes.TransactionNotBegun);
                }
                await _transaction.RollbackAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Dispose()
        {
            try
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public DbCommand GetCommand(string query)
        {
            DbCommand command = null;
            try
            {
                if (_connection == null)
                {
                    throw new DbException(DbException.Codes.ConnectionNotOpened);
                }
                if (_transaction == null)
                {
                    command = new NpgsqlCommand(query, _connection);
                }
                else
                {
                    command = new NpgsqlCommand(query, _connection, _transaction);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return command;

        }
        public DbCommand GetCommand()
        {
            DbCommand command = null;
            try
            {
                if (_connection == null)
                {
                    throw new DbException(DbException.Codes.ConnectionNotOpened);
                }
                if (_transaction == null)
                {
                    command = new NpgsqlCommand();
                    command.Connection = _connection;
                }
                else
                {
                    command = new NpgsqlCommand();
                    command.Connection = _connection;
                    command.Transaction = _transaction;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return command;

        }
        public DbParameter AddParameter(DbCommand command, string parameterName, DbTypes.Types type)
        {
            DbParameter parameter = null;
            try
            {
                int position = -1;
                NpgsqlParameter pgParameter = null;

                switch (type)
                {
                    case DbTypes.Types.String:
                        pgParameter = new NpgsqlParameter(parameterName, NpgsqlTypes.NpgsqlDbType.Varchar);
                        position = command.Parameters.Add(pgParameter);
                        break;
                    case DbTypes.Types.Boolean:
                        pgParameter = new NpgsqlParameter(parameterName, NpgsqlTypes.NpgsqlDbType.Boolean);
                        position = command.Parameters.Add(pgParameter);
                        break;
                    case DbTypes.Types.Integer:
                        pgParameter = new NpgsqlParameter(parameterName, NpgsqlTypes.NpgsqlDbType.Integer);
                        position = command.Parameters.Add(pgParameter);
                        break;
                    case DbTypes.Types.Long:
                        pgParameter = new NpgsqlParameter(parameterName, NpgsqlTypes.NpgsqlDbType.Bigint);
                        position = command.Parameters.Add(pgParameter);
                        break;
                    case DbTypes.Types.Json:
                        pgParameter = new NpgsqlParameter(parameterName, NpgsqlTypes.NpgsqlDbType.Json);
                        position = command.Parameters.Add(pgParameter);
                        break;
                    case DbTypes.Types.DateTime:
                        pgParameter = new NpgsqlParameter(parameterName, NpgsqlTypes.NpgsqlDbType.Timestamp);
                        position = command.Parameters.Add(pgParameter);
                        break;
                    case DbTypes.Types.Date:
                        pgParameter = new NpgsqlParameter(parameterName, NpgsqlTypes.NpgsqlDbType.Date);
                        position = command.Parameters.Add(pgParameter);
                        break;
                    case DbTypes.Types.Decimal:
                        pgParameter = new NpgsqlParameter(parameterName, NpgsqlTypes.NpgsqlDbType.Numeric);
                        position = command.Parameters.Add(pgParameter);
                        break;
                    case DbTypes.Types.ByteArray:
                        pgParameter = new NpgsqlParameter(parameterName, NpgsqlTypes.NpgsqlDbType.Bytea);
                        position = command.Parameters.Add(pgParameter);
                        break;
                    default:
                        pgParameter = new NpgsqlParameter(parameterName, null);
                        position = command.Parameters.Add(pgParameter);
                        break;
                }
                parameter = command.Parameters[position];
            }
            catch (Exception)
            {

                throw;
            }
            return parameter;

        }
        public async Task<int> ExecuteNonQuery(DbCommand command)
        {
            int affectedRowCount = 0;
            try
            {
                string queryWithParameters = command.CommandText;

                //foreach (NpgsqlParameter parameter in command.Parameters.Cast<NpgsqlParameter>())
                //{
                //    queryWithParameters = queryWithParameters.Replace("@"+parameter.ParameterName, parameter.Value.ToString());
                //}
                affectedRowCount = await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (command != null)
                {
                    command.Dispose();
                }
            }
            return affectedRowCount;
        }
        public async Task<DbDataReader> Execute(DbCommand command)
        {
            DbDataReader reader = null;
            try
            {
                reader = await command.ExecuteReaderAsync();

            }
            catch (Exception)
            {

                throw;
            }
            return reader;
        }
    }
}
