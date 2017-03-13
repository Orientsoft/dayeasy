
using System;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    public class JointExceptionDto : DDto
    {
        public string Id { get; set; }
        public long TeacherId { get; set; }
        public string TeacherCode { get; set; }
        public string Teacher { get; set; }
        public string PictureId { get; set; }
        public string Student { get; set; }
        public string Message { get; set; }
        public DateTime CreationTime { get; set; }
        public byte Status { get; set; }
        public byte? ExceptionType { get; set; }
        public string ExceptionTypeTitle { get; set; }
        public DateTime? SolveTime { get; set; }
    }
}
