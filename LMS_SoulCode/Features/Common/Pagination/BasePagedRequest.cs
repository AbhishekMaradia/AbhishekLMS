namespace LMS_SoulCode.Features.Common
{
    public class BasePagedRequest
    {
        public string? SearchTerm { get; set; }
        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = (value < 1) ? 1 : value;
        }

        private int _pageSize = 100;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > 50) ? 50 : value; // Max page size limit
        }
    }
}
