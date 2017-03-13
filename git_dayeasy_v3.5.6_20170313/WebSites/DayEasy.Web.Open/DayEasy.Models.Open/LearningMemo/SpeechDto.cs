namespace DayEasy.Models.Open.LearningMemo
{
    public class SpeechDto : DDto
    {
        /// <summary> 语音链接 </summary>
        public string Url { get; set; }

        /// <summary> 时长(秒) </summary>
        public int Seconds { get; set; }
    }
}
