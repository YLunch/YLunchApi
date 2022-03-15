using System.Net;

namespace YLunchApi.Domain.CommonAggregate.Dto;

public class ErrorDto
{
    public string Title { get; set; }
    public int Status { get; set; }
    public List<string> Errors { get; set; }

    public ErrorDto(HttpStatusCode status, List<string> errors)
    {
        Title = status.ToString();
        Status = (int) status;
        Errors = errors;
    }

    public ErrorDto(HttpStatusCode status, string error)
    {
        Title = status.ToString();
        Status = (int) status;
        Errors = new List<string> { error };
    }
}
