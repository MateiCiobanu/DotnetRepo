namespace Tema1;

public static class DisplayHelper
{
    /// <summary>
    /// Afișează informații despre un obiect, in functie de tipul obiectului.
    /// </summary>
    public static void DisplayInfo(object obj)
    {
        if (obj is Book book)
        {
            Console.WriteLine($"Carte: {book.Title} ({book.YearPublished})");
        }
        else if (obj is Borrower borrower)
        {
            Console.WriteLine($"Împrumutător: {borrower.Name}, a împrumutat {borrower.BorrowedBooks.Count} cărți");
        }
        else
        {
            Console.WriteLine("Unknown type");
        }
    }
    
    /// <summary>
    /// Afișează cărțile publicate după 2010 dintr-o listă de cărți.
    /// </summary>
    public static void DisplayBooksAfter2010(List<Book> books)
    {   
        //  salvez cartile filtrate dupa anul de publicare (publicate dupa 2010)  
        var recentBooks = books.Where(static b => b.YearPublished > 2010);

        Console.WriteLine("\nCărți publicate după 2010:");
        foreach (var book in recentBooks)
        {
            Console.WriteLine($"- {book.Title} ({book.YearPublished})");
        }
    }
}