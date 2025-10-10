namespace Tema1;

// clasa Librarian care nu permite schimbarea valorilor unei proprietăți după inițializare deoarece proprietățile folosesc init-only setters
public class Librarian
{
    public string Name { get; init; }
    public string Email { get; init; }
    public string LibrarySection { get; init; }
}