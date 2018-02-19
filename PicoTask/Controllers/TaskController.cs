using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PicoTask.Data;
using PicoTask.Services;

namespace PicoTask.Controllers
{
    [Route("api/tasks")]
    public class TaskController : Controller
    {
        public readonly TaskService _taskService;

        public TaskController(TaskService taskService)
        {
            _taskService = taskService;
        }
        
        [HttpGet("")]
        public async Task<IActionResult> List(bool includeDone = false)
        {
            return Json(await _taskService.GetTasksAsync(includeDone));
        }
        
        [HttpPost("")]
        public async Task<IActionResult> Create(string rawTitle, string note)
        {
            return Json(await _taskService.CreateTaskAsync(rawTitle, note));
        }
        
        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> ToggleDone(Guid id)
        {
            return Json(await _taskService.ToggleDoneAsync(id));
        }
        
        [HttpPost("{id}/archive")]
        public async Task<IActionResult> Archive(Guid id)
        {
            return Json(await _taskService.ArchiveAsync(id));
        }
        
        [HttpPost("{id}/unarchive")]
        public async Task<IActionResult> Unarchive(Guid id)
        {
            return Json(await _taskService.UnarchiveAsync(id));
        }
        
        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(Guid id, string rawTitle, string note)
        {
            return Json(await _taskService.EditAsync(id, rawTitle, note));
        }
    }
}
