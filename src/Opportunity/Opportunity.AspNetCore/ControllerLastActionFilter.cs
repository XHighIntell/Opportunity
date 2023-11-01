using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Opportunity;

namespace Opportunity.AspNetCore;

public class ControllerLastActionFilter: IActionFilter, IOrderedFilter {
    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context) {
        var exception = context.Exception;
        var descriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;

        if (descriptor == null) return;
        if (descriptor.MethodInfo.GetCustomAttributes(typeof(ApiAttribute), false).Length == 0) return; // Method doesn't have [ApiAttribute]


        if (exception == null) {
            if (context.Result is ObjectResult o) {
                context.Result = new JsonResult(new {
                    //success = true,
                    //error = (object?)null,
                    result = o.Value,
                });
            }
            else {
                throw new NotImplementedException("API method must return object. Remove [ApiAttribute] from your method or please add your case here.");
            }
        }
        else {

            if (exception is HandledException) {
                context.Result = new JsonResult(new {
                    // success = false,
                    error = new {
                        message = exception.Message,
                        code = exception.HResult,
                    }
                }) {
                    StatusCode = 200,
                };
            }
            else {
                context.Result = new JsonResult(new {
                    // success = false,
                    error = new {
                        message = exception.Message,
                        stack = exception.StackTrace,
                    }
                }) {
                    StatusCode = 500,
                };
            }


            context.ExceptionHandled = true;
        }
    }
}
