using DayEasy.Contracts;
using DayEasy.Core.Dependency;

namespace DayEasy.Paper.Services.Helper.Question
{
    /// <summary> 问题变化接收者 </summary>
    public class ChangeObserver
    {
        /// <summary> 更新事件 </summary>
        /// <param name="questionId"></param>
        /// <param name="paperId"></param>
        /// <param name="smallId"></param>
        public delegate void UpdateEventHandler(string questionId, string paperId, string smallId = null);

        public event UpdateEventHandler Update;
        private readonly IMarkingContract _markingContract;

        public ChangeObserver()
        {
            _markingContract = CurrentIocManager.Resolve<IMarkingContract>();
            Update += ChangeObserver_Update;
        }

        private void ChangeObserver_Update(string questionId, string paperId, string smallId = null)
        {
            //_markingContract.MkUpdateDetailByQuestionChange(questionId, paperId, smallId);
        }

        /// <summary>
        /// 异步通知
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="smallId"></param>
        /// <param name="paperId"></param>
        public void NotifyAsync(string questionId, string smallId = null, string paperId = "")
        {
            if (Update != null)
                Update(questionId, paperId, smallId);
            //Task.Run(() => Update(questionId, paperId));
        }
    }
}
