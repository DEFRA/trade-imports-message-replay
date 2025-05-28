using Refit;

namespace Defra.TradeImportsMessageReplay.MessageReplay.Services
{
    public interface IGatewayApi
    {
        [Post("/ITSW/CDS/SubmitImportDocumentCDSFacadeService")]
        Task SendClearanceRequest([Body] string xml);

        [Post("/ws/CDS/defra/alvsclearanceinbound/v1")]
        Task SendAlvsDecision([Body] string xml);

        [Post("/ITSW/CDS/NotifyFinalisedStateCDSFacadeService")]
        Task SendFinalisation([Body] string xml);
    }
}
