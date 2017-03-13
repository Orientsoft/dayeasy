using System;

namespace DayEasy.Contracts.Dtos.Group
{
    /// <summary>
    /// 批量导入教师DTO
    /// </summary>
    [Serializable]
    public class BatchTeacherDto
    {
        public long Id { get; set; }
        public string SubjectName { get; set; }
        public int? SubjectId { get; set; }
    }
}
