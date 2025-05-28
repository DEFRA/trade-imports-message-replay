using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs
{
    public interface IBlobProcessor
    {
        bool CanProcess(BlobItem item);

        Task Process(BlobItem item);
    }
}
