namespace Defra.TradeImportsMessageReplay.MessageReplay.Data.Entities;

public interface IDataEntity
{
    public string Id { get; set; }
    public string ETag { get; set; }

    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    void OnSave();
}
