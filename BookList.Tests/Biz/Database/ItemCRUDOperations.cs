﻿using System;
using NUnit.Framework;
using BookList.Biz.Database;

namespace BookList.Tests.Biz.Database
{
    [TestFixture]
    public class ItemCRUDOperations
    {
        PostgreSQLConnection Db { get; set; }
        int BookId { get; set; }
        int ListId { get; set; }
        int UserId { get; set; }
        string UserToken { get; set; }

        public ItemCRUDOperations()
        {
            Db = new PostgreSQLConnection();

            var userToken = UserFactory.CreateNewUser(Db, "testuser", "testuseritem", "password");

            if (userToken != null)
            {
                UserId = UserFactory.LoadSingleByToken(userToken).Id;
                UserToken = userToken;
            }

            var bookId = BookFactory.CreateNewBook(Db, "test book", "test author");
            var listId = ListFactory.CreateNewList(Db, UserToken, "test list");

            BookId = (int)bookId;
            ListId = (int)listId;
        }

        ~ItemCRUDOperations()
        {
            BookFactory.DeleteBook(Db, BookId);
            ListFactory.DeleteList(Db, ListId);
            UserFactory.DeleteUser(Db, UserToken);
        }

        [Test]
        public void TestCreateNewItem()
        {
            if (Int32.TryParse(ItemFactory.CreateNewItem(Db, BookId, ListId), out int id))
            {
                var testItem = ItemFactory.LoadSingle(Db, id);

                Assert.IsNotNull(testItem);
                Assert.AreEqual(BookId, testItem.Book.Id);
                Assert.AreEqual(ListId, testItem.ListId);

                ItemFactory.DeleteItem(Db, id);
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestLoadAll()
        {
            var testItemsList = ItemFactory.LoadAll(Db);
            //var testItem = testItemsList.Find(item => item.Id == 1);

            Assert.IsNotNull(testItemsList);
            //Assert.IsNotNull(testItem);
            //Assert.AreEqual(2, testItem.Book.Id);
        }

        [Test]
        public void TestDeleteItem()
        {
            if (Int32.TryParse(ItemFactory.CreateNewItem(Db, BookId, ListId), out int id))
            {
                ItemFactory.DeleteItem(Db, id);

                var testItem = ItemFactory.LoadSingle(Db, id);

                Assert.IsNull(testItem);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
