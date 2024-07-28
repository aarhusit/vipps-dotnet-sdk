using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;
using Vipps.net.Infrastructure;
using Vipps.net.Models.Recurring;

namespace Vipps.net.Services;

public interface IVippsRecurringService
{
    Task<ICollection<AgreementResponseV3>> ListAgreementsV3Async  (AgreementStatus? status = null, long? createdAfter = null, Guid? continuationToken = null, CancellationToken cancellationToken = default);
    Task<DraftAgreementResponseV3> DraftAgreementV3Async          (DraftAgreementV3 body, CancellationToken cancellationToken = default);
    //Task<AgreementResponseV3> FetchAgreementV3Async             (string agreementId, string content_Type = null, CancellationToken cancellationToken = default);
    //Task UpdateAgreementPatchV3Async                            (string agreementId, PatchAgreementV3 body, string content_Type = null, CancellationToken cancellationToken = default);
    //Task<object> AcceptUsingPATCHV3Async                        (string agreementId, ForceAcceptAgreementV3 body, string content_Type = null, CancellationToken cancellationToken = default);
    //Task<ICollection<ChargeResponseV3>> ListChargesV3Async      (string agreementId, Guid? continuation_Token = null, string content_Type = null, ChargeStatus? status = null, CancellationToken cancellationToken = default);
    //Task<ChargeReference> CreateChargeV3Async                   (string agreementId, CreateChargeV3 body, string content_Type = null, CancellationToken cancellationToken = default);
    //Task<ChargeResponseV3> FetchChargeV3Async                   (string agreementId, string chargeId, string content_Type = null, CancellationToken cancellationToken = default);
    //Task CancelChargeV3Async                                    (string agreementId, string chargeId, string content_Type = null, CancellationToken cancellationToken = default);
    //Task<ChargeResponseV3> FetchChargeByIdV3Async               (string chargeId, string content_Type = null, CancellationToken cancellationToken = default);
    //Task CaptureChargeV3Async                                   (string agreementId, string chargeId, CaptureRequestV3 body, string content_Type = null, CancellationToken cancellationToken = default);
    //Task RefundChargeV3Async                                    (string agreementId, string chargeId, RefundRequest body, string content_Type = null, CancellationToken cancellationToken = default);


    //Task<CreatePaymentResponse> CreatePayment(Vipps.net.Models.Recurring.ag createPaymentRequest, CancellationToken cancellationToken = default);
}

internal sealed class VippsRecurringService(RecurringServiceClient recurringServiceClient) : IVippsRecurringService
{
    public async Task<ICollection<AgreementResponseV3>> ListAgreementsV3Async(AgreementStatus? status = null, long? createdAfter = null, Guid? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var requestPath = $"/recurring/v3/agreements?status={status}&createdAfter={createdAfter}";

        var headers = continuationToken.HasValue? new Dictionary<string, string>{{ "Continuation-Token", continuationToken.Value.ToString("N") } } : null;
        
        var agreements = await recurringServiceClient.ExecuteRequestWithHeader<ICollection<AgreementResponseV3>>(
            requestPath,
            HttpMethod.Get,
            headers,
            cancellationToken
        );

        return agreements;
    }

    public async Task<DraftAgreementResponseV3> DraftAgreementV3Async(DraftAgreementV3 body, CancellationToken cancellationToken = default)
    {
        const string requestPath = "/recurring/v3/agreements";

        var agreement = await recurringServiceClient.ExecuteRequest<DraftAgreementV3, DraftAgreementResponseV3>(
            requestPath,
            HttpMethod.Post,
            body,
            cancellationToken);

        return agreement;
    }
}
