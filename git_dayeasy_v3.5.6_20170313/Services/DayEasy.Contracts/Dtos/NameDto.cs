using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos
{
    public class NameDto : NameDto<string>
    {
        public NameDto(string id, string name)
            : base(id, name)
        {
        }
    }

    public class NameDto<TId> : NameDto<TId, string>
    {
        public NameDto(TId id, string name)
            : base(id, name)
        {
        }
    }

    public class NameDto<TId, TName> : DDto
    {
        public NameDto(TId id, TName name)
        {
            Id = id;
            Name = name;
        }
        public TId Id { get; set; }
        public TName Name { get; set; }
    }
}
