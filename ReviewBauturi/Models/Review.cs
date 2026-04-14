namespace ReviewBauturi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int BeverageId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}