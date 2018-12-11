﻿using System;
using NUnit.Framework;
using BookList.Biz.Database;

namespace BookList.Tests.Biz.Database
{
    [TestFixture]
    public class BookCRUDOperations
    {
        [Test]
        public void TestCreateNewBook()
        {
            var db = new PostgreSQLConnection();

            if (Int32.TryParse(BookFactory.CreateNewBook(db, "test book", "test author"), out int id))
            {
                var testBook = BookFactory.LoadSingle(db, id);

                Assert.IsNotNull(testBook);
                Assert.AreEqual("test book", testBook.Title);
                Assert.AreEqual("test author", testBook.Author);

                //ListFactory.DeleteList(db, id);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestLoadAll()
        {
            var testBookList = BookFactory.LoadAll(new PostgreSQLConnection());
            var testBook = testBookList.Find(book => book.Id == 2);

            Assert.IsNotNull(testBookList);
            Assert.IsNotNull(testBook);
            //Assert.AreEqual(3, testBookList.Count);
            Assert.AreEqual("all the light we cannot see", testBook.Title);
        }
    }
}
