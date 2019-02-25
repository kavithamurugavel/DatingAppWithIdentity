using System;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.DTOs
{
    public class PhotoForCreationDTO
    {
        public string Url { get; set; }
        public IFormFile File { get; set; } // IFormFile represents a file sent with the HttpRequest.
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public string PublicID { get; set; }

        public PhotoForCreationDTO()
        {
            DateAdded = DateTime.Now;
        }
    }
}