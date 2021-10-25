using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

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
            try
            {
                GetBookById(id);
                return true;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }
        }
        public static Book GetBookById(int id)
        {
            foreach(Book libaryBook in LibaryBooks)
            {
                if (libaryBook.Id == id)
                    return libaryBook;
            }
            throw new KeyNotFoundException("no book with id: " + id.ToString() + " found.");
        }
    }
}
