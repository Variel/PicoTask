using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PicoTask.Models.Request;
using PicoTask.Models.Response;
using PicoTask.Services;

namespace PicoTask.Controllers
{
    [Route("kapi")]
    public class KakaoApiController : Controller
    {
        private readonly TaskService _taskService;

        public KakaoApiController(TaskService taskService)
        {
            _taskService = taskService;
        }
        
        [HttpGet("keyboard")]
        public async Task<IActionResult> Keyboard()
        {
            return Json(new Keyboard
            {
                type = KeyboardType.Buttons,
                buttons = new []
                {
                    "추가하기",
                    "목록 보기",
                }
            });
        }

        [HttpPost("message")]
        public async Task<IActionResult> Message(ChatRequest model)
        {
            
        }
    }
}
