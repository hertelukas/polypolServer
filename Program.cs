using System;
using MongoDB.Driver;

namespace polypolServer
{
    class Program
    {
        static void Main(string[] args)
        {
            MongoClient dbClient = new MongoClient("mongodb+srv://admin:Fz05cKoP4PPx@polypol-i4wle.mongodb.net/test?retryWrites=true&w=majority");

            var dbList = dbClient.ListDatabaseNames().ToList();

            System.Console.WriteLine("The list of databases on this server is: ");
            foreach (var db in dbList)
            {
                System.Console.WriteLine(db);
            }
        }
    }
}
