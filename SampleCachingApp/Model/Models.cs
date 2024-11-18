namespace SampleCachingApp.Model
{
    public enum FilterationType
    {
        Equals,
        GreaterThan,
        LessThan,
        Contains,
        StartsWith ,
        EndsWith ,
        In  
    }
    public class FilterCondition
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public FilterationType Comparison { get; set; }
    }

    public class RequestObject
    {
        public SortingParameters sortingParameters { get; set; } 
        public Paging paging { get; set; } 
        public List<Filters> filterCondition { get; set; }
    }
    public class SortingParameters
    {
        
        public string sortProperty { get; set; } = "id";
        public bool ascendingSort { get; set; } = true;
    }
    public class Paging
    {
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 5;
    }
    public class Filters
    {
        public string PropertyName { get; set; } = "";
        public string Value { get; set; } = "";
        public string Comparison { get; set; } = ""; 
    } 
}
