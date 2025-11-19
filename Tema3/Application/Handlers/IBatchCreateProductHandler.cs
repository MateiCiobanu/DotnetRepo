using Tema3.Application.Dtos;
using Tema3.Application.Requests;

namespace Tema3.Application.Handlers;

public interface IBatchCreateProductHandler
{
    Task<BatchProductResultDto> HandleBatchAsync(BatchCreateProductRequest request, CancellationToken ct);
}

