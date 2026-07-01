namespace ColmenaEmpresa.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int?    StatusCode { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public bool Is404 => StatusCode == 404;
}
