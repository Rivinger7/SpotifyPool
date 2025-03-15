namespace BusinessLogicLayer.ModelView.Service_Model_Views.Admin.Response
{
	public class PagedResult<T>
	{
		public MetaData Meta { get; set; }
		public IEnumerable<T> Data { get; set; }

		public PagedResult(IEnumerable<T> data, int pageNumber, int pageSize, long totalCount)
		{
			Data = data;
			Meta = new MetaData
			{
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = totalCount,
				TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
			};
		}
	}

	public class MetaData
	{
		public int PageNumber { get; set; }		//Số trang

		public int PageSize { get; set; }		//Kích cỡ trang

		public long TotalCount { get; set; }	//Tổng tài khoản bao nhiêu

		public int TotalPages { get; set; }		//Tổng trang
	}
}
