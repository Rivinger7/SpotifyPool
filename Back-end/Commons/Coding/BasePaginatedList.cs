namespace Utility.Coding
{
    public class BasePaginatedList<TDocument>
    {
        public IReadOnlyCollection<TDocument> Items { get; private set; }

        // Thuộc tính để lưu trữ tổng số phần tử
        public int TotalItems { get; private set; }

        // Thuộc tính để lưu trữ số trang hiện tại
        public int CurrentPage { get; private set; }

        // Thuộc tính để lưu trữ tổng số trang
        public int TotalPages { get; private set; }

        // Thuộc tính để lưu trữ số phần tử trên mỗi trang
        public int PageSize { get; private set; }

        /// <summary>
        /// Constructor để khởi tạo danh sách phân trang
        /// </summary>
        /// <param name="items">Câu query trả về danh sách tất cả những thứ cần show ra</param>
        /// <param name="itemsAmount">Thuộc tính lưu trữ tổng số phần tử</param>
        /// <param name="currentPageIndex">Thuộc tính lưu trữ số trang hiện tại</param>
        /// <param name="itemsPerPage">Thuộc tính lưu trữ số phần tử trên mỗi trang</param>
        public BasePaginatedList(IReadOnlyCollection<TDocument> items, int itemsAmount, int currentPageIndex, int itemsPerPage)
        {
            TotalItems = itemsAmount;
            CurrentPage = currentPageIndex;
            PageSize = itemsPerPage;
            TotalPages = (int)Math.Ceiling(itemsAmount / (double)itemsPerPage);
            Items = items;
        }

        // Phương thức để kiểm tra nếu có trang trước đó
        public bool HasPreviousPage => CurrentPage > 1;

        // Phương thức để kiểm tra nếu có trang kế tiếp
        public bool HasNextPage => CurrentPage < TotalPages;

    }
}
