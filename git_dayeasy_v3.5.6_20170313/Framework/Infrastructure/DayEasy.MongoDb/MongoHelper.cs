

using MongoDB.Driver.Builders;

namespace DayEasy.MongoDb
{
    public static class MongoHelper
    {
        /// <summary> 添加数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        public static void Insert<T>(T model)
        {
            new MongoManager().Collection<T>().Insert(model);
        }

        /// <summary> 删除数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        public static void Delete<T>(string id)
        {
            new MongoManager().Collection<T>().Remove(Query.EQ("_id", id));
        }
    }
}
