using System.Collections.Generic;
using DayEasy.Contracts.Models;

namespace DayEasy.Paper.Services.Model
{
    public class SavePaper
    {
        public TP_Paper Paper { get; set; }
        public List<TP_PaperSection> PaperSections { get; set; }
        public List<TP_PaperContent> PaperContents { get; set; }
        public List<TP_SmallQScore> SmallQScores { get; set; }
    }
}
