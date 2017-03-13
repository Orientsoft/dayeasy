using System.Collections.Generic;
using System.Threading.Tasks;
using DayEasy.Utility;
using SolrNet;

namespace DayEasy.Assistant.Solr
{
    public abstract class SolrManager<T>
        where T : SolrEntity
    {
        protected static TV BaseInstance<TV>()
            where TV : SolrManager<T>, new()
        {
            if (Singleton<TV>.Instance != null)
                return Singleton<TV>.Instance;
            SolrHelper.Instance.InitSolr<T>();
            Singleton<TV>.Instance = new TV();
            return Singleton<TV>.Instance;
        }

        protected ISolrOperations<T> Solr
        {
            get { return SolrHelper.Instance.Solr<T>(); }
        }

        protected ResponseHeader Add(T entity)
        {
            return Solr.Add(entity);
        }

        protected ResponseHeader AddRange(IEnumerable<T> entities)
        {
            return Solr.AddRange(entities);
        }

        /// <summary>
        /// 刷新Solr库
        /// </summary>
        /// <returns></returns>
        public ResponseHeader Optimize()
        {
            return SolrHelper.Instance.Commit<T>();
        }

        /// <summary>
        /// 清空题库索引
        /// </summary>
        public ResponseHeader Clear()
        {
            return Solr.Delete(SolrQuery.All);
        }

        /// <summary>
        /// 删除某个题目
        /// </summary>
        /// <param name="id"></param>
        public ResponseHeader Delete(string id)
        {
            return Solr.Delete(id);
        }

        public void DeleteAsync(string id)
        {
            var task = new Task(() =>
            {
                Solr.Delete(id);
                Optimize();
            });
            task.Start();
        }
        public void DeleteAsync(IEnumerable<string> ids)
        {
            var task = new Task(() =>
            {
                foreach (var id in ids)
                {
                    Solr.Delete(id);
                }
                Optimize();
            });
            task.Start();
        }

        public abstract ResponseHeader Update(string id);

        public void UpdateAsync(string id)
        {
            var task = new Task(() =>
            {
                Update(id);
                Optimize();
            });
            task.Start();
        }

        public void UpdateAsync(IEnumerable<string> ids)
        {
            var task = new Task(() =>
            {
                foreach (var id in ids)
                {
                    Update(id);
                }
                Optimize();
            });
            task.Start();
        }
    }
}
