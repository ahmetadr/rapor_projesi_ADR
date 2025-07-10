// KorRaporOnline.API/Models/Common/PaginatedResponse.cs

using System.Collections.Generic;

namespace KorRaporOnline.API.Models.Common
{
    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}