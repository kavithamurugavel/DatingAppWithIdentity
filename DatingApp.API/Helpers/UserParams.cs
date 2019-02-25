namespace DatingApp.API.Helpers
{
    // class to help the client send up information about pagination in the url's query string to the API
    // so that they can request page number or page size. This would help API to send back the requested
    // information to the client through the pagination header (Section 14, Lecture 138)
    public class UserParams
    {
        private const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1; // default value, unless requested otherwise we always return the first page
        private int pageSize = 10;

        // client can customize what they want to return from the API
        public int PageSize
        {
            get { return pageSize;}
            set { pageSize = (value > maxPageSize) ? maxPageSize : value;}
        }
        public int UserID { get; set; }
        public string Gender { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 99;
        public string OrderBy { get; set; }
        public bool Likees { get; set; } = false;
        public bool Likers { get; set; } = false;
        
    }
}