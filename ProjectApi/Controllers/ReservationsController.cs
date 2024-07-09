using ProjectApi.Application.Core.Reservations.Dtos;
using ProjectApi.Application.Core.Reservations.RequestHandlers.Interfaces;
using ProjectApi.Attributes;
using ProjectApi.Controllers.Base;
using ProjectApi.Core.Base.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ProjectApi.Controllers;

[ApplicationAuthorization]
public class ReservationsController : BaseV1ApiController
{
    private readonly ICreateReservationRequestHandler _createReservationRequestHandler;
    private readonly IReadReservationRequestHandler _readReservationRequestHandler;
    private readonly IReadByIdReservationRequestHandler _readByIdReservationRequestHandler;
    private readonly IDeleteReservationRequestHandler _deleteReservationRequestHandler;
    private readonly IWithdrawRequestHandler _withdrawRequestHandler;
    private readonly IPayReservationRequestHandler _payReservationRequestHandler;
    private readonly ICancelReservationRequestHandler _cancelReservationRequestHandler;
    private readonly IDiscardDraftAndStartOverRequestHandler _discardDraftAndStartOverRequestHandler;

    public ReservationsController(ICreateReservationRequestHandler createReservationRequestHandler,
        IReadReservationRequestHandler readReservationRequestHandler,
        IReadByIdReservationRequestHandler readByIdReservationRequestHandler,
        IDeleteReservationRequestHandler deleteReservationRequestHandler,
        IWithdrawRequestHandler withdrawRequestHandler, IPayReservationRequestHandler payReservationRequestHandler,
        ICancelReservationRequestHandler cancelReservationRequestHandler,
        IDiscardDraftAndStartOverRequestHandler discardDraftAndStartOverRequestHandler)
    {
        _createReservationRequestHandler = createReservationRequestHandler;
        _readReservationRequestHandler = readReservationRequestHandler;
        _readByIdReservationRequestHandler = readByIdReservationRequestHandler;
        _deleteReservationRequestHandler = deleteReservationRequestHandler;
        _withdrawRequestHandler = withdrawRequestHandler;
        _payReservationRequestHandler = payReservationRequestHandler;
        _cancelReservationRequestHandler = cancelReservationRequestHandler;
        _discardDraftAndStartOverRequestHandler = discardDraftAndStartOverRequestHandler;
    }

    [HttpPost]
    public async Task<ReservationDto> Create([FromBody] CreateReservationRequestDto request)
    {
        return await _createReservationRequestHandler.Handle(request);
    }

    [HttpGet]
    public async Task<PaginatedListResponseDto<ReservationDto>> Read([FromQuery] ReadReservationRequestDto request)
    {
        return await _readReservationRequestHandler.Handle(request);
    }

    [HttpGet("{id:guid}")]
    public async Task<ReservationDto> ReadById([FromRoute] Guid id)
    {
        return await _readByIdReservationRequestHandler.Handle(id);
    }

    [HttpDelete("{id:guid}")]
    public async Task Delete([FromRoute] Guid id)
    {
        await _deleteReservationRequestHandler.Handle(id);
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<WithdrawRequestHandlerResponseDto> Withdraw([FromRoute] Guid id)
    {
        return await _withdrawRequestHandler.Handle(id);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<PayReservationRequestHandlerResponseDto> Pay([FromRoute] Guid id,
        [FromBody] PayReservationRequestHandlerRequestDto request)
    {
        return await _payReservationRequestHandler.Handle(id, request);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ReservationCancelationDto> Cancel([FromRoute] Guid id)
    {
        return await _cancelReservationRequestHandler.Handle(id);
    }

    [HttpPost("{id:guid}/start-over")]
    public async Task StartOver([FromRoute] Guid id)
    {
        await _discardDraftAndStartOverRequestHandler.Handle(id);
    }
}