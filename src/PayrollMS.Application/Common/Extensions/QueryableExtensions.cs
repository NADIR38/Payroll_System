using System.Linq.Expressions;

namespace PayrollMS.Application.Common.Extensions;

/// <summary>
/// Database-agnostic queryable extension methods for the Application layer.
/// Decouples Application layer from EF Core or any specific ORM package.
/// Injected/initialized by Infrastructure at startup.
/// </summary>
public static class QueryableExtensions
{
    private static Func<object, CancellationToken, Task<System.Collections.IList>>? _toListAsync;
    private static Func<object, object?, CancellationToken, Task<object?>>? _firstOrDefaultAsync;
    private static Func<object, object?, CancellationToken, Task<bool>>? _anyAsync;
    private static Func<object, object>? _asNoTracking;
    private static Func<object, object, object>? _include;

    public static void Configure(
        Func<object, CancellationToken, Task<System.Collections.IList>> toListAsync,
        Func<object, object?, CancellationToken, Task<object?>> firstOrDefaultAsync,
        Func<object, object?, CancellationToken, Task<bool>> anyAsync,
        Func<object, object> asNoTracking,
        Func<object, object, object> include)
    {
        _toListAsync = toListAsync;
        _firstOrDefaultAsync = firstOrDefaultAsync;
        _anyAsync = anyAsync;
        _asNoTracking = asNoTracking;
        _include = include;
    }

    public static async Task<List<T>> ToListAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default)
    {
        if (_toListAsync == null)
            return source.ToList();

        var result = await _toListAsync(source, cancellationToken);
        return result.Cast<T>().ToList();
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default)
    {
        if (_firstOrDefaultAsync == null)
            return source.FirstOrDefault();

        var result = await _firstOrDefaultAsync(source, null, cancellationToken);
        if (result == null) return default;
        return (T)result;
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (_firstOrDefaultAsync == null)
            return source.FirstOrDefault(predicate);

        var result = await _firstOrDefaultAsync(source, predicate, cancellationToken);
        if (result == null) return default;
        return (T)result;
    }

    public static async Task<bool> AnyAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default)
    {
        if (_anyAsync == null)
            return source.Any();

        return await _anyAsync(source, null, cancellationToken);
    }

    public static async Task<bool> AnyAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (_anyAsync == null)
            return source.Any(predicate);

        return await _anyAsync(source, predicate, cancellationToken);
    }

    public static IQueryable<T> AsNoTracking<T>(this IQueryable<T> source)
    {
        if (_asNoTracking == null)
            return source;

        return (IQueryable<T>)_asNoTracking(source);
    }

    public static IQueryable<T> Include<T, TProperty>(
        this IQueryable<T> source,
        Expression<Func<T, TProperty>> navigationPropertyPath)
    {
        if (_include == null)
            return source;

        return (IQueryable<T>)_include(source, navigationPropertyPath);
    }
}
