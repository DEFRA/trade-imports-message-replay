using System.Diagnostics;

namespace Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

[DebuggerDisplay("{Name}")]
public class BlobItem
{
    public string Name { get; set; } = default!;
    public BinaryData Content { get; set; } = default!;
}
