namespace LawsonSLab5.Models
{
    public class BorrowedBook
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowedDate { get; set; } = DateTime.Now;
    }
}