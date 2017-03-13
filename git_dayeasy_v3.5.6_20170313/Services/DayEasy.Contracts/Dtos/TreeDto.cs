using System.Runtime.Serialization;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos
{
    [DataContract(Name = "tree")]
    public class TreeDto : DDto
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "parent")]
        public string ParentId { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "isParent")]
        public bool HasChildren { get; set; }
        [DataMember(Name = "li_attr")]
        public Attr LiAttr { get; set; }
    }

    [DataContract]
    public class Attr : DDto
    {
        [DataMember(Name = "sort")]
        public int Sort { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }
    }
}
