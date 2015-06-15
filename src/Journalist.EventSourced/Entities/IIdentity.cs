namespace Journalist.EventSourced.Entities
{
    public interface IIdentity
    {
        string GetTag();

        string GetValue();
    }
}