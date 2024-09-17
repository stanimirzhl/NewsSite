
namespace NewsSite.Models.Pagination
{
    public class PagingModel<T>:List<T>
    {
        public int PageIndex { get; set; }

        public int TotalPages { get; set; }

        public PagingModel(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count/(double)pageSize);
            this.AddRange(items);
        }
        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;

        public static PagingModel<T> Create(List<T> source, int pageIndex, int pageSize)
        {
            int count = source.Count;
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return new PagingModel<T>(items, count, pageIndex, pageSize);
        }
    }
}
