﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                    "*추가하기*",
                    "*목록보기*"
                }
            });
        }

        [HttpPost("message")]
        public async Task<IActionResult> Message([FromBody] ChatRequest model)
        {
            if (model.content == "*추가하기*")
            {
                return Json(new MessageResponse
                {
                    keyboard = Models.Response.Keyboard.TextKeyboard,
                    message = new Message
                    {
                        text = "내용을 적어주세요"
                    }
                });
            } 
            else if (model.content == "*목록보기*")
            {
                var tasks = (await _taskService.GetTasksAsync()).ToArray();
                return Json(new MessageResponse
                {
                    keyboard = new Keyboard
                    {
                        type = KeyboardType.Buttons,
                        buttons = tasks.Select(t => t.Title + "\n" + "[" + t.Id.ToString().ToLower() + "] ").ToArray()
                    },
                    message = new Message
                    {
                        text = String.Join("\n", tasks.Select(t => String.Join(" ", t.Categories.Select(c => "[" + c.Category.FullName + "]")) + t.Title).ToArray())
                    }
                });
            }
            else
            {
                var idPattern = @"\[(?<id>\w{8}-\w{4}-\w{4}-\w{4}-\w{12})\]";
                var idMatch = Regex.Match(model.content, idPattern);

                if (idMatch.Success)
                {
                    var id = Guid.Parse(idMatch.Groups["id"].Value);
                    var task = await _taskService.GetTaskAsync(id);

                    return Json(new MessageResponse
                    {
                        keyboard = new Keyboard
                        {
                            type = KeyboardType.Buttons,
                            buttons = new[]
                            {
                                "*수정하기*",
                                "*완료처리*",
                                "*삭제하기*"
                            }
                        },
                        message = new Message
                        {
                            text = $"{task.Title}\n" +
                                   (task.Deadline != null ? task.Deadline?.ToString("yy.MM.dd HH:mm") + "까지\n" : "") + 
                                   (!String.IsNullOrWhiteSpace(task.Place) ? $"{task.Place}에서" : "")
                        }
                    });
                }

                if (!model.content.StartsWith("*"))
                {
                    await _taskService.CreateTaskAsync(model.content, null);

                    return Json(new MessageResponse
                    {
                        keyboard = new Keyboard
                        {
                            type = KeyboardType.Buttons,
                            buttons = new[]
                            {
                            "*추가하기*",
                            "*목록보기*"
                        }
                        },
                        message = new Message { text = "추가되었습니다" }
                    });
                }
            }

            return Json(new MessageResponse
            {
                keyboard = Models.Response.Keyboard.TextKeyboard,
                message = new Message
                {
                    text = "확인"
                }
            });
        }
    }
}
