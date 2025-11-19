namespace Tema3.Application.Requests;

public class BatchCreateProductRequest
{
    public List<CreateProductProfileRequest> Products { get; set; } = new();
    public bool EnableParallelProcessing { get; set; } = true;
    public int MaxDegreeOfParallelism { get; set; } = 4;
}

