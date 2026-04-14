using LawsonSLab5.Models;
using LawsonSLab5.Services;
using Microsoft.AspNetCore.Hosting;
using Moq;


namespace LawsonSLab5
{
    public static class LibraryServiceFactory
    {
        public static LibraryService Create(
            string[]? bookLines = null,
            string[]? userLines = null)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var dataDir = Path.Combine(tempDir, "Data");
            Directory.CreateDirectory(dataDir);

            File.WriteAllLines(
                Path.Combine(dataDir, "Books.csv"),
                bookLines ?? Array.Empty<string>());

            File.WriteAllLines(
                Path.Combine(dataDir, "Users.csv"),
                userLines ?? Array.Empty<string>());

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(equals => equals.ContentRootPath).Returns(tempDir);

            return new LibraryService(mockEnv.Object);
        }
    }
}
