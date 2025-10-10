namespace Tema1;

public record Borrower(
    int Id,
    string Name,
    List<Book> BorrowedBooks
);