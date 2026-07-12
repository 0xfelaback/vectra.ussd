public record MenuSession : SessionBase
{
    public int currentPage { get; set; } // 1-indexed-based counting
}
