using System;
using System.Collections.Generic;
using System.Net;
using LWS_Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LWS_Gateway.Controllers
{
    public class SuperControllerBase: ControllerBase
    {

        protected IActionResult HandleCase(Dictionary<ResultCode, Func<IActionResult>> caseHandler, ResultCode actual)
        {
            if (caseHandler.ContainsKey(actual))
            {
                return caseHandler[actual].Invoke();
            }

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}