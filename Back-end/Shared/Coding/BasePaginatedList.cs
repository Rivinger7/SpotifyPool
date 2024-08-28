namespace Utility.Coding
{
    public class BasePaginatedList<T>
    {
        public IReadOnlyCollection<T> Items { get; private set; }

        // Thuộc tính để lưu trữ tổng số phần tử
        public int TotalItems { get; private set; }

        // Thuộc tính để lưu trữ số trang hiện tại
        public int CurrentPage { get; private set; }

        // Thuộc tính để lưu trữ tổng số trang
        public int TotalPages { get; private set; }

        // Thuộc tính để lưu trữ số phần tử trên mỗi trang
        public int PageSize { get; private set; }

        // Constructor để khởi tạo danh sách phân trang
        public BasePaginatedList(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
        {
            TotalItems = count;
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }

        // Phương thức để kiểm tra nếu có trang trước đó
        public bool HasPreviousPage => CurrentPage > 1;

        // Phương thức để kiểm tra nếu có trang kế tiếp
        public bool HasNextPage => CurrentPage < TotalPages;

    }
}
