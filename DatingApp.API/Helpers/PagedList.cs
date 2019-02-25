using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Helpers
{
    // for pagination Section 14 Lecture 137
    // for the complete pagination workflow between SPA and API, check Important Points.txt
    public class PagedList<T> : List<T> // making this generic so that we can use for things other than users too
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public PagedList(List<T> items, int countOfItems, int pageNumber, int pageSize)
        {
            TotalCount = countOfItems;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(countOfItems / (double)pageSize);
            this.AddRange(items); // adding the items to the class (which is technically a list)
        }

        // this returns a new instance of the pagedlist
        // IQueryable: defers the execution of our request and allows us to define parts of our query
        // against our database in multiple steps and with deferred execution.
        // https://samueleresca.net/2015/03/the-difference-between-iqueryable-and-ienumerable/
        // https://stackoverflow.com/questions/1578778/using-iqueryable-with-linq - this link has better explanation
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            // source could be the list of users for eg:
            var count = await source.CountAsync(); // returns total no. of elements (before any pagination)
            /* Skip skips the number of elements to show the necessary elements in that particular page
            for eg: if we are in page 1, and page size is 5, then with the formula, we have to skip
            (1 - 1) * 5 = 0 elements. Take will then take the pageSize amount i.e. the first 5 elements in this case.
            If we are in page 2, then the formula will be (2 - 1) * 5 = 5. So that would skip the first 5 elements and take the next 5 elements.
            This goes on until we finish displaying all elements */
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize); // this would use the ctor above to assign the params to the class varaiables
            // which in turn will be sent to the SPA using the http headers
        }
    }
}