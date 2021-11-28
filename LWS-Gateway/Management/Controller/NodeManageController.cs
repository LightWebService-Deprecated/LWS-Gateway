using System.Threading.Tasks;
using LWS_Gateway.Management.Model.Request;
using LWS_Gateway.Management.Service;
using Microsoft.AspNetCore.Mvc;

namespace LWS_Gateway.Management.Controller
{
    [ApiController]
    [Route("/api/manage/node")]
    public class NodeManageController: ControllerBase
    {
        private readonly NodeManageService _nodeManageService;

        public NodeManageController(NodeManageService service)
        {
            _nodeManageService = service;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterNewNode(NewNodeRequest newNodeRequest)
        {
            await _nodeManageService.RegisterNewNodeInternal(newNodeRequest);

            return Ok();
        }
    }
}