namespace backend.Models
{
    public class Friendship
    {
        public int Id { get; set; }

        public int RequesterId { get; set; }
        public User Requester { get; set; } = null!;

        public int AddresseeId { get; set; }
        public User Addressee { get; set; } = null!;

        public bool IsConfirmed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}