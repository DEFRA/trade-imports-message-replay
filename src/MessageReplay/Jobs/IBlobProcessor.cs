using Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Jobs
{
    public interface IBlobProcessor
    {
        bool CanProcess(string queue);

        Task Process(BlobItem item);
    }
}
