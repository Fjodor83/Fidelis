namespace Fidelity.Application.Common.Models;

/// <summary>
/// Generic result pattern - ISO 25000: Reliability
/// </summary>
public class Result
{
    public bool Succeeded { get; protected set; }
    public string[] Errors { get; protected set; } = Array.Empty<string>();
    public string? Message { get; protected set; }

    public static Result Success(string? message = null) => new() { Succeeded = true, Message = message };
    public static Result Failure(params string[] errors) => new() { Succeeded = false, Errors = errors };
    public static Result Failure(string error) => new() { Succeeded = false, Errors = new[] { error } };
}

/// <summary>
/// Generic result with data
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; private set; }

    public static Result<T> Success(T data, string? message = null) => new()
    {
        Succeeded = true,
        Data = data,
        Message = message
    };

    public new static Result<T> Failure(params string[] errors) => new()
    {
        Succeeded = false,
        Errors = errors
    };

    public new static Result<T> Failure(string error) => new()
    {
        Succeeded = false,
        Errors = new[] { error }
    };
}

/// <summary>
/// Paginated list for queries
/// </summary>
public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }
}
