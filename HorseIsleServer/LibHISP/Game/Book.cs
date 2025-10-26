using System.Collections.Generic;
using System.Linq;

namespace HISP.Game.Services
{
    public class Book
    {
        private static List<Book> libaryBooks = new List<Book>();
        public static Book[] LibaryBooks
        {
            get
            {
                return libaryBooks.ToArray();
            }
        }

        public int Id;
        public string Title;
        public string Author;
        public string Text;
        public Book(int id, string title, string author, string text)
        {
            Id = id;
            Title = title;
            Author = author;
            Text = text;
            libaryBooks.Add(this);
        }

        public static bool BookExists(int id)
        {
            return LibaryBooks.Any(o => o.Id == id);
        }
        public static Book GetBookById(int id)
        {
            return LibaryBooks.First(o => o.Id == id);
        }
    }
}
