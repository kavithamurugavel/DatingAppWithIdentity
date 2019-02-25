namespace DatingApp.API.Models
{
    // Basically creating a table of Like with Likee and Liker IDs, which in turn
    // need foreign key references to the User table to get the IDs of the liker and likee.
    // Hence the User Liker and User Likee variables below
    public class Like
    {
        public int LikerID { get; set; } // id of user liking another user
        public int LikeeID { get; set; } // id of user being liked by another user
        
        // similar to how User is defined in the Photo class (This was also explained in the MVC course).
        // Similar to Photos, we have a relationship between the user and the likers/likees. This is required because liker/likee ids are
        // basically user ids from the User table (similar to the UserID column in Photos table)
        // It is all then connected to the User with the Liker and Likee ICollections in User.cs.
        // Also, the Users below will then be defined as Foreign keys (so that the liker and likee ids are 
        // obtained from the User table) in DataContext OnModelCreating override.
        public User Liker { get; set; }
        public User Likee { get; set; }

    }
}