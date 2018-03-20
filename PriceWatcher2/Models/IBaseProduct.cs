namespace PriceWatcher2.Models
{
    public interface IBaseProduct : IBaseObject
    {
        string ExternalId { get; set; }
    }
}
