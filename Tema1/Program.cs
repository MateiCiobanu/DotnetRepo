using Tema1;

var book1 = new Book("1984", "George Orwell", 1949);
var book2 = new Book("To Kill a Mockingbird", "Harper Lee", 1960);
var book3 = new Book("The Great Gatsby", "F. Scott Fitzgerald", 1925);

var borrower1 = new Borrower(1, "Matei", new List<Book> { book1, book2 });

var borrower2 = borrower1 with
{
    BorrowedBooks = new List<Book>(borrower1.BorrowedBooks)
    {
        book3
    }
};

Console.WriteLine($"{borrower2.Name} a împrumutat {borrower2.BorrowedBooks.Count} cărți: {string.Join(", ", borrower2.BorrowedBooks.Select(b => b.Title))}");



// Top-level statements
var userGivenBooks = new List<Book>();

while (true)
{
    Console.Write("Introdu titlul unei cărți (sau apasă Enter ca să oprești): ");
    var title = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(title))
        break;
    
    Console.Write("Introdu autorul cărții: ");
    var author = Console.ReadLine() ?? "Necunoscut";

    Console.Write("Introdu anul în care a fost publicată cartea: ");
    if (!int.TryParse(Console.ReadLine(), out int year))
    {
        Console.WriteLine("An invalid, cartea nu a fost adăugată.\n");
        continue;
    }

    userGivenBooks.Add(new Book(title, author, year));
    Console.WriteLine("Cartea a fost adăugată!\n");
}

Console.WriteLine("\nCărțile introduse sunt:");
foreach (var book in userGivenBooks)
{
    Console.WriteLine($"- {book.Title} ({book.YearPublished}) — {book.Author}");
}

// ----------------------------------------------------------------------

Console.WriteLine("\nTest DisplayInfo:");
DisplayHelper.DisplayInfo(book1);
DisplayHelper.DisplayInfo(borrower2);
DisplayHelper.DisplayInfo(12345);


//----------------------------------------------------------------------
DisplayHelper.DisplayBooksAfter2010(userGivenBooks);