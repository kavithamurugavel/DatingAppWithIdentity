using System;

namespace DatingApp.API.DTOs
{
    public class MessageToReturnDTO
    {
        public int ID { get; set; }
        public int SenderID { get; set; }
        // Even though we have SenderKnownAs instead of KnownAs and SenderPhotoUrl instead of PhotoUrl
        // AutoMapper is smart enough to map User's KnownAs and Photo url to these properties.
        // Same with the recipient properties right below
        public string SenderKnownAs { get; set; }
        public string SenderPhotoUrl { get; set; }
        public int RecipientID { get; set; }
        public string RecipientKnownAs { get; set; }
        public string RecipientPhotoUrl { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
    }
}