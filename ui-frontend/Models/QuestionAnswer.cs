namespace ui_frontend.Models
{
    public class QuestionAnswer
    {
        public string? Question { get; set; }
        public string? Answer { get; set; }

        public bool? AnswerHidden { get; set; }

        public List<Citation> Citations
        {
            get
            {
                return _citations;
            }
        }

        private List<Citation> _citations { get; set; } = new List<Citation>();

        public QuestionAnswer(string question, string answer)
        {
            Question = question;
            Answer = answer;
        }

        public void AddCitation(Citation citation) => _citations.Add(citation);
    }
}
