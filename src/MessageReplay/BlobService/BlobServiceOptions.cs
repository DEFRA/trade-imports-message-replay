using System.ComponentModel.DataAnnotations;

namespace Defra.TradeImportsMessageReplay.MessageReplay.BlobService;

public class BlobServiceOptions
{
    public const string SectionName = nameof(BlobServiceOptions);

    [Required]
    public string CredentialType { get; set; } = "ConfidentialClientApplicationTokenCredential"; //StorageSharedKeyCredential or ConfidentialClientApplicationTokenCredential

    [Required]
    public string DmpBlobContainer { get; set; } = null!;

    [Required]
    public string DmpBlobUri { get; set; } = null!;

    [Required]
    public string AzureClientId { get; set; } = null!;

    public string AzureTenantId { get; set; } = null!;

    [Required]
    public string AzureClientSecret { get; set; } = null!;

    public int Retries { get; set; } = 3;

    public int Timeout { get; set; } = 10;
}
