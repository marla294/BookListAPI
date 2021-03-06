﻿using System.Web.Http;
using System.Web.Http.Cors;
using System.Collections.Generic;
using BookList.Biz.Models;
using BookList.Biz.Database;

public class ListName {
    public string Name { get; set; }
    public string UserToken { get; set; }
}

namespace BookList.Service.Controllers
{
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    public class ListsController : ApiController
    {
        PostgreSQLConnection Db { get; set; }

        public ListsController()
        {
            Db = new PostgreSQLConnection();
        }

        public List<List> Get(string id)
        {
            // id is the userToken, have to convert to userId
            var user = UserFactory.LoadSingleByToken(id);

            return user == null ? null : ListFactory.LoadByUserId(Db, user.Id);
        }

        public void Put(int id, [FromBody]ListName value)
        {
            ListFactory.UpdateListName(Db, id, value.Name);
        }

        // returns the id of the new list as a string
        public int? Post([FromBody]ListName value)
        {
            return ListFactory.CreateNewList(Db, value.UserToken, value.Name);
        }

        public void Delete(int id) 
        {
            ListFactory.DeleteList(Db, id);
        }
    }
}
