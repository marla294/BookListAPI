﻿using System;
using System.Collections.Generic;
using BookList.Biz.Models;
using System.Linq;

namespace BookList.Biz.Database
{
    public static class BookFactory
    {
        // returns the id of the new book
        // if the book can't be created returns null
        public static string CreateNewBook(IDbConnection dbConnection, string title, string author)
        {
            string id;
            string slicedBookTitle = title;
            string slicedBookAuthor = author;

            // Shouldn't be creating books with the title or author blank
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
            {
                return null;
            }

            // if title is greater than 30 characters chop it
            if (title.Length > 30)
            {
                slicedBookTitle = title.Substring(0, 30);
            }

            // if author is greater than 30 characters chop it
            if (author.Length > 30)
            {
                slicedBookAuthor = author.Substring(0, 30);
            }

            dbConnection.Insert("books", new KeyValuePair<string, object>[] {
                                Pairing.Of("title", $"{slicedBookTitle}"),
                                Pairing.Of("author", $"{slicedBookAuthor}")
            }).Execute();

            id = dbConnection.Take("books").OrderBy("id", "desc").Limit(1).Execute()[0][0];

            Int32.TryParse(id, out int idConverted);

            return id;
        }

        public static List<Book> LoadAll(IDbConnection dbConnection)
        {
            var bookResultSet = dbConnection.Take("books").OrderBy("id").Execute();

            var books = new List<Book>();

            for (var i = 0; i < bookResultSet[0].Count; i++)
            {
                Book book = Int32.TryParse(bookResultSet[0][i], out int id)
                    ? new Book(id, bookResultSet[1][i], bookResultSet[2][i],
                               bookResultSet[3][i], bookResultSet[4][i])
                    : new Book();

                books.Add(book);
            }

            return books;
        }

        public static Book LoadSingle(IDbConnection dbConnection, int id) 
        {
            return LoadAll(dbConnection).FirstOrDefault<Book>(book => book.Id == id);
        }

        public static void DeleteBook(IDbConnection dbConnection, int id)
        {
            dbConnection.Delete("booklist").Where(Pairing.Of("book", id)).Execute();
            dbConnection.Delete("books").Where(Pairing.Of("id", id)).Execute();
        }
    }
}
