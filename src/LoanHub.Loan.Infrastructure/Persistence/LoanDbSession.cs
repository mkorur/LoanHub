using Npgsql;

namespace LoanHub.Loan.Infrastructure.Persistence;

public interface ILoanDbSession
{
    (NpgsqlConnection Connection, NpgsqlTransaction Transaction) RequireActiveTransaction();
}

internal sealed class LoanUnitOfWork : ILoanDbSession, Application.Abstractions.ILoanUnitOfWork
{
    private readonly NpgsqlDataSource _dataSource;
    private NpgsqlConnection? _connection;
    private NpgsqlTransaction? _transaction;
    private bool _committed;

    public LoanUnitOfWork(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Transaction already started.");

        _connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        _transaction = await _connection.BeginTransactionAsync(cancellationToken);
        _committed = false;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction.");

        await _transaction.CommitAsync(cancellationToken);
        _committed = true;
        await ResetAsync();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync(cancellationToken);
        await ResetAsync();
    }

    public (NpgsqlConnection Connection, NpgsqlTransaction Transaction) RequireActiveTransaction()
    {
        if (_connection is null || _transaction is null)
            throw new InvalidOperationException("Call BeginTransactionAsync before using the unit of work.");

        return (_connection, _transaction);
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null && !_committed)
            await _transaction.RollbackAsync(CancellationToken.None);

        await ResetAsync();
    }

    private async Task ResetAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        _committed = false;
    }
}
