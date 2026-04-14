using LawsonSLab5.Models;

namespace LawsonSLab5.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly string _booksPath;
        private readonly string _usersPath;

        private List<Book> _books = new();
        private List<User> _users = new();
        private List<BorrowedBook> _borrowedBooks = new();

        public LibraryService(IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "Data");

            _booksPath = Path.Combine(dataDir, "Books.csv");
            _usersPath = Path.Combine(dataDir, "Users.csv");

            ReadBooks();
            ReadUsers();
        }

        private void ReadBooks()
        {
            _books.Clear();
            var lines = File.ReadAllLines(_booksPath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = SplitCsvLine(line);
                if (parts.Length < 4) continue;

                if (!int.TryParse(parts[0].Trim(), out int id)) continue;

                _books.Add(new Book
                {
                    Id = id,
                    Title = parts[1].Trim(),
                    Author = parts[2].Trim(),
                    ISBN = parts[3].Trim(),
                    IsAvailable = true
                });
            }
        }

        private void WriteBooks()
        {
            var lines = _books.Select(b =>
            {
                var title = b.Title.Contains(',') ? $"\"{b.Title}\"" : b.Title;
                return $"{b.Id},{title},{b.Author},{b.ISBN}";
            });
            File.WriteAllLines(_booksPath, lines);
        }

        public List<Book> GetBooks() => _books;

        public void AddBook(Book book)
        {
            book.Id = _books.Count > 0 ? _books.Max(b => b.Id) + 1 : 1;
            book.IsAvailable = true;
            _books.Add(book);
            WriteBooks();
        }

        public void EditBook(Book book)
        {
            var existing = _books.FirstOrDefault(b => b.Id == book.Id);
            if (existing == null) return;
            existing.Title = book.Title;
            existing.Author = book.Author;
            existing.ISBN = book.ISBN;
            WriteBooks();
        }

        public void DeleteBook(int bookId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book == null) return;
            _books.Remove(book);
            _borrowedBooks.RemoveAll(br => br.BookId == bookId);
            WriteBooks();
        }


        private void ReadUsers()
        {
            _users.Clear();
            var lines = File.ReadAllLines(_usersPath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                if (!int.TryParse(parts[0].Trim(), out int id)) continue;

                _users.Add(new User
                {
                    Id = id,
                    Name = parts[1].Trim(),
                    Email = parts[2].Trim()
                });
            }
        }

        private void WriteUsers()
        {
            var lines = _users.Select(u => $"{u.Id},{u.Name},{u.Email}");
            File.WriteAllLines(_usersPath, lines);
        }

        public List<User> GetUsers() => _users;

        public void AddUser(User user)
        {
            user.Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;
            _users.Add(user);
            WriteUsers();
        }

        public void EditUser(User user)
        {
            var existing = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existing == null) return;
            existing.Name = user.Name;
            existing.Email = user.Email;
            WriteUsers();
        }

        public void DeleteUser(int userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return;
            _users.Remove(user);
            _borrowedBooks.RemoveAll(br => br.UserId == userId);
            WriteUsers();
        }


        public List<BorrowedBook> GetBorrowedBooks() => _borrowedBooks;

        public bool BorrowBook(int userId, int bookId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book == null || !book.IsAvailable) return false;

            var alreadyBorrowed = _borrowedBooks
                .Any(br => br.UserId == userId && br.BookId == bookId);
            if (alreadyBorrowed) return false;

            book.IsAvailable = false;
            _borrowedBooks.Add(new BorrowedBook
            {
                UserId = userId,
                BookId = bookId,
                BorrowedDate = DateTime.Now
            });
            return true;
        }

        public bool ReturnBook(int userId, int bookId)
        {
            var record = _borrowedBooks
                .FirstOrDefault(br => br.UserId == userId && br.BookId == bookId);
            if (record == null) return false;

            _borrowedBooks.Remove(record);

            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book != null) book.IsAvailable = true;

            return true;
        }


        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}
