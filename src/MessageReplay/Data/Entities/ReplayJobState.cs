namespace Defra.TradeImportsMessageReplay.MessageReplay.Data.Entities;

public class ReplayJobState : IDataEntity
{
    public required string Id { get; set; }
    public string ETag { get; set; } = null!;
    public required string BlobName { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    public void OnSave() { }
}
