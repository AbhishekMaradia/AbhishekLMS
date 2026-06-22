namespace LMS_SoulCode.Features.Common
{
    public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;

        public static PagedApiResponse<T> Success(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords, string message = "Success")
        {
            var adjustedPageSize = pageSize > 0 ? pageSize : 10;
            var totalPages = (int)Math.Ceiling((double)totalRecords / adjustedPageSize);

            return new PagedApiResponse<T>
            {
                Code = StatusCodes.Success,
                IsSuccess = true,
                Message = message,
                Data = data ?? new List<T>(),
                PageNumber = pageNumber,
                PageSize = adjustedPageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }

        public static new PagedApiResponse<T> Fail(string message, int code)
        {
            return new PagedApiResponse<T>
            {
                Code = code,
                IsSuccess = false,
                Message = message,
                Data = new List<T>(),
                PageNumber = 0,
                PageSize = 0,
                TotalRecords = 0,
                TotalPages = 0
            };
        }
    }
}
