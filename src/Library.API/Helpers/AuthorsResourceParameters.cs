using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.API.Helpers
{
    public class AuthorsResourceParameters
    {
        // We want to limit user input page size from a query string. If user input page size is greater than our 
        // defined limit then use our defined default limit.
        const int maxPageSize = 20;

        private int _pageSize = 10;

        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

        public int PageNumber { get; set; } = 1;

        public string Genre { get; set; }

        public string SearchQuery { get; set; }


    }
}
