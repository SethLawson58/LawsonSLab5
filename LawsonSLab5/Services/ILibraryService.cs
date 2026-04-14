using LawsonSLab5.Models;

namespace LawsonSLab5.Services
{
    public interface ILibraryService
    {
        List<Book> GetBooks();
        void AddBook(Book book);
        void EditBook(Book book);
        void DeleteBook(int bookId);

        List<User> GetUsers();
        void AddUser(User user);
        void EditUser(User user);
        void DeleteUser(int userId);

        List<BorrowedBook> GetBorrowedBooks();
        bool BorrowBook(int userId, int bookId);
        bool ReturnBook(int userId, int bookId);
    }
}