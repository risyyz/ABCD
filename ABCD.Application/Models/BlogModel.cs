namespace ABCD.Application.Models {
    public record BlogModel(
        int BlogId,
        string Name,
        string Description,
        IReadOnlyCollection<string> Domains

        //add last updated by, last updated timestamp
    );
}
