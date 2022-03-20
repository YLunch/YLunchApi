using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace YLunchApi.Domain.CommonAggregate.Dto;

public class ErrorDto
{
    public string Title { get; set; } = null!;
    public int StatusCode { get; set; }
    public Errors Errors { get; set; } = null!;

    // because of deserialization usage
    [ExcludeFromCodeCoverage]
    public ErrorDto()
    {
    }

    public ErrorDto(HttpStatusCode status, string error)
    {
        Title = status.ToString();
        StatusCode = (int)status;
        Errors = new Errors(error);
    }
}

public class Errors
{
    public List<string> Reasons { get; set; } = null!;

    // because of deserialization usage
    [ExcludeFromCodeCoverage]
    public Errors()
    {
    }

    public Errors(string error)
    {
        Reasons = new List<string> { error };
    }
}
