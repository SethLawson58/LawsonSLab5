using LawsonSLab5.Models;
using LawsonSLab5.Services;

namespace LawsonSLab5
{
    public class LibraryServiceTests
    {
        // Test 1 Read Books
        // Happy path: verify books load correctly from a CSV file.
        [Fact]
        public void ReadBooks_WithValidCsv_ReturnsAllBooks()
        {
            // Arrange
            var service = LibraryServiceFactory.Create(bookLines: new[]
            {
                "1,The Great Gatsby,F. Scott Fitzgerald,978-0743273565",
                "2,1984,George Orwell,978-0451524935"
            });

            // Act
            var books = service.GetBooks();

            //Assert
            Assert.Equal(99, books.Count);
            Assert.Equal("The Great Gatsby", books[0].Title);
            Assert.Equal("George Orwell", books[1].Author);
        }

        // Test 2 Add Book
        // Happy path: adding a book increases the count and assigns an ID.
        [Fact]
        public void AddBook_NewBook_IncreasesCountAndAssignsId()
        {
            // Arrange
            var service = LibraryServiceFactory.Create();
            var newBook = new Book { Title = "Dune", Author = "Frank Herbert", ISBN = "978-0441013593" };

            // Act
            service.AddBook(newBook);
            var books = service.GetBooks();

            // Assert
            Assert.Single(books);
            Assert.Equal(1, books[0].Id);
            Assert.Equal("Dune", books[0].Title);
        }

        // Test 3 Add Book
        // Edge case: new book ID should be one higher than the current maximum.
        [Fact]
        public void AddBook_WhenBooksExist_AssignsNextHighestId()
        {
            // Arrange
            var service = LibraryServiceFactory.Create(bookLines: new[]
            {
                "5,Existing Book,Some Author,000-000"
            });

            // Act
            service.AddBook(new Book { Title = "New Book", Author = "New Author", ISBN = "111-111" });

            // Assert
            Assert.Equal(6, service.GetBooks().Last().Id);
        }

        // Test 4 Edit Book
        // Happy path: editing a book updates its fields correctly.
        [Fact]
        public void EditBook_WithValidId_UpdatesBookFields()
        {
            // Arrange
            var service = LibraryServiceFactory.Create(bookLines: new[]
            {
                "1,Old Title,Old Author,000-000"
            });
            var updated = new Book { Id = 1, Title = "New Title", Author = "New Author", ISBN = "111-111" };

            // Act
            service.EditBook(updated);
            var book = service.GetBooks().First(b => b.Id == 1);

            // Assert
            Assert.Equal("New Title", book.Title);
            Assert.Equal("New Author", book.Author);
            Assert.Equal("111-111", book.ISBN);
        }

        // Test 5 Delete Book
        // Happy Path: deleting a book removes it from the list.
        [Fact]
        public void DeleteBook_WithValidId_RemovesBookFromList()
        {
            // Arrange
            var service = LibraryServiceFactory.Create(bookLines: new[]
            {
                "1,Book One,Author One,111",
                "2,Book Two,Author Two,222"
            });

            // Act
            service.DeleteBook(1);
            var books = service.GetBooks();

            // Assert
            Assert.Single(books);
            Assert.Equal(2, books[0].Id);
        }

        // Test 6 Delete Book
        // Edge case: deleting a book that does not exist should not crash the app.
        [Fact]
        public void DeleteBook_WithNonExistentId_DoesNotThrow()
        {
            // Arrange
            var service = LibraryServiceFactory.Create(bookLines: new[]
            {
                "1,Book One,Author One,111"
            });

            // Act
            var exception = Record.Exception(() => service.DeleteBook(999));
            
            // Assert
            Assert.Null(exception);
            Assert.Single(service.GetBooks());
        }

        // Test 7 Read Users
        // Happy path: users load correctly from a CSV file.
        [Fact]
        public void ReadUsers_WithValidCsv_ReturnsCorrectUsers()
        {
            // Arrange
            var service = LibraryServiceFactory.Create(userLines: new[]
            {
                "1,Alice Johnson,alice@example.com",
                "2,Bob Smith,bob@example.com"
            });

            // Act
            var users = service.GetUsers();

            // Assert
            Assert.Equal(2, users.Count);
            Assert.Equal("Alice Johnson", users[0].Name);
            Assert.Equal("bob@example.com", users[1].Email);
        }

        // Test 8 Borrow Book
        // Happy path: borrowing an available book succeeds and marks it unavailable.
        [Fact]
        public void BorrowBook_AvailableBook_SucceedsAndMarksUnavailable()
        {
            // Arrange
            var service = LibraryServiceFactory.Create(
                bookLines: new[] { "1,Some Book,Some Author,111" },
                userLines: new[] { "1,Alice,alice@example.com" });

            // Act
            var result = service.BorrowBook(userId: 1, bookId: 1);
            var book = service.GetBooks().First(b => b.Id == 1);

            // Assert
            Assert.True(result);
            Assert.False(book.IsAvailable);
            Assert.Single(service.GetBorrowedBooks());
        }

        // Test 9 Borrow Book
        // Edge case: borrowing a book that is already checked out should fail.
        [Fact]
        public void BorrowBook_AlreadyCheckedOut_ReturnsFalse()
        {
            // Arrange
            var service = LibraryServiceFactory.Create(
                bookLines: new[] { "1,Popular Book,Author,111" },
                userLines: new[]
                {
                    "1,Alice,alice@example.com",
                    "2,Bob,bob@example.com"
                });

            service.BorrowBook(userId: 1, bookId: 1);

            // Act
            var result = service.BorrowBook(userId: 2, bookId: 1);

            // Assert
            Assert.False(result);
            Assert.Single(service.GetBorrowedBooks());
        }

        // Test 10 Return Book
        // Happy path: returning a borrowed book makes it available again.
        [Fact]
        public void ReturnBook_BorrowedBook_MakesBookAvailableAgain()
        {
            // Arrange
            var service = LibraryServiceFactory.Create(
                bookLines: new[] { "1,Some Book,Some Author,111" },
                userLines: new[] { "1,Alice,alice@example.com" });

            service.BorrowBook(userId: 1, bookId: 1);

            // Act
            var result = service.ReturnBook(userId: 1, bookId: 1);
            var book = service.GetBooks().First(b => b.Id == 1);

            // Assert
            Assert.True(result);
            Assert.True(book.IsAvailable);
            Assert.Empty(service.GetBorrowedBooks());
        }
    }
}
