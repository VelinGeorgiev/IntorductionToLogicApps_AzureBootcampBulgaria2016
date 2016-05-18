namespace DataConvertor.Models
{
    public class FormattedData
    {
        public FormattedData(string name, int score, int totalScore)
        {
            Name = name;
            Score = score;
            TotalScore = totalScore;
        }

        public string Name { get; set; }
        public int Score { get; set; }
        public int TotalScore { get; set; }
    }
}