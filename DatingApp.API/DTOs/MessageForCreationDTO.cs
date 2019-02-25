using System;

namespace DatingApp.API.DTOs
{
    public class MessageForCreationDTO
    {
        public int SenderID { get; set; }
        public int RecipientID { get; set; }
        public DateTime MessageSent { get; set; }
        public string content { get; set; }
        public MessageForCreationDTO()
        {
            MessageSent = DateTime.Now;
        }
    }
}