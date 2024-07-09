using System.Net;
using ProjectApi.ViewModels.Base;
using ProjectUtilities.Utilities.Exceptions;
using FluentValidation;

namespace ProjectApi.Middlewares
{
    public class ExceptionsHandlingMiddleware
    {
        private readonly ILogger<ExceptionsHandlingMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ExceptionsHandlingMiddleware(RequestDelegate next, ILogger<ExceptionsHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                var logMessage = $"Exception occurred ({httpContext.Request.Method})'{httpContext.Request.Path}'";
                var logLevel = ex is IUserFriendlyException or ValidationException ? LogLevel.Warning : LogLevel.Error;

                _logger.Log(logLevel, ex, logMessage);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ErrorResponseViewModel errorResponseViewModel;
            context.Response.ContentType = "application/json";

            switch (exception)
            {
                // case ForbiddenException e:
                //     context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                //     errorResponseViewModel = new ErrorResponseViewModel
                //     {
                //         Message = e.Message
                //     };
                //     break;
                case IExtendedUserFriendlyException e:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponseViewModel = new ExtendedErrorsResponseViewModel(exception.Message,
                        e.Errors);

                    break;
                case IUserFriendlyException e:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponseViewModel = new ErrorResponseViewModel(exception.Message);

                    break;
                case ValidationException e:
                    context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    errorResponseViewModel = new ExtendedErrorsResponseViewModel("Validation problems",
                        e.Errors.Select(error => error.ErrorMessage).ToList());

                    break;
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponseViewModel = new ErrorResponseViewModel("Oops! Something Went Wrong...");

                    break;
            }

            return context.Response.WriteAsync(errorResponseViewModel.ToString());
        }
    }
}