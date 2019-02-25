using System;

namespace DatingApp.API.Models
{
    public class Photo
    {
        public int ID { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
        
        // this publicId is the one that Cloudinary returns
        public string PublicID { get; set; }
        public bool IsApproved { get; set; }

        // the following is for cascade delete 
        // (check the ExtendedUserClass migration - in the Up method under the Photos part, we have onDelete: ReferentialAction.Cascade), 
        // and to define the relationship between user and photo. This was also explained in the MVC course
        public User User { get; set; }
        public int UserID { get; set; }
    }
}