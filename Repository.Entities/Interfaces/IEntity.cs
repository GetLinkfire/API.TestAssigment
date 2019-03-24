namespace Repository.Entities.Interfaces
{
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
