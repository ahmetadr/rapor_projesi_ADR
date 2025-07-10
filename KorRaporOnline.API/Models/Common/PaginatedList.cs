// KorRaporOnline.API/Models/Common/PaginatedList.cs
using System.Collections.Generic;

namespace KorRaporOnline.API.Models.Common
{
    public class PaginatedResult<T>  // PaginatedList yerine PaginatedResult kullanacağız çünkü List ile karışabilir
    {
        public IEnumerable<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}