namespace Tema2.Requests;

public record RetrieveBooksRequest(
    int Page=1, 
    int PageSize=10, 
    string? Author=null,
    string? SortBy=null,
    string? SortDirection=null);