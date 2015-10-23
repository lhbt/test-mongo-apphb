using System;
using System.Configuration;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace test_mongo
{
    using Nancy;

    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = parameters =>
            {
                var connectionstring = ConfigurationManager.AppSettings.Get("MONGOLAB_URI");
                var url = new MongoUrl(connectionstring);
                var client = new MongoClient(url);
                var server = client.GetServer();
                var database = server.GetDatabase(url.DatabaseName);

                var collection = database.GetCollection<Thingy>("Thingies");

                // insert object
                collection.Insert(new Thingy { Name = "foo" });

                // fetch all objects
                var thingies = collection.FindAll();

                return thingies.First().Name;
            };
        }
    }

    public class Thingy
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }

    public class DatabaseFacade : IDatabaseFacade
    {
        public DatabaseFacade(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var server = client.GetServer();

            Database = server.GetDatabase(databaseName);

            BsonSerializer.RegisterIdGenerator(typeof(Guid), GuidGenerator.Instance);
        }

        private MongoDatabase Database { get; set; }

        public MongoCollection<T> GetCollection<T>(string collectionName)
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>();
            }

            return Database.GetCollection<T>(collectionName);
        }
    }

    public interface IDatabaseFacade
    {
        MongoCollection<T> GetCollection<T>(string collectionName);
    }
}