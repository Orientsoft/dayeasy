using MongoDB.Bson;

namespace DayEasy.Web.File.Domain
{
    public class FileDocument
    {
        public string Id { get; set; }
        public BsonValue File { get; set; }
    }
}