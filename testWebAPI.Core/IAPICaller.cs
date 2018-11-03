namespace testWebAPI.Core
{
    public interface IAPICaller
    {
        string Module { get; set; }
        string Name { get; }
        int ArgumentCount { get; }

        Result Do(params string[] args);
    }
}
