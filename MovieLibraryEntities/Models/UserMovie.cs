namespace MovieLibraryEntities.Models
{
    public class UserMovie
    {
        public long Id { get; set; }
        public long Rating { get; set; }
        public DateTime RatedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Movie Movie { get; set; }

    }
}
