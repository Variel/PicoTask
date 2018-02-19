using System;
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
        private readonly CategoryService _categoryService;
        private static readonly Keyboard DefaultKeyboard = new Keyboard
        {
            type = KeyboardType.Buttons,
            buttons = new[]
            {
                "*추가하기*",
                "*목록 보기*",
                "*카테고리 보기*"
            }
        };

        public KakaoApiController(TaskService taskService, CategoryService categoryService)
        {
            _taskService = taskService;
            _categoryService = categoryService;
        }
        
        [HttpGet("keyboard")]
        public async Task<IActionResult> Keyboard()
        {
            return Json(DefaultKeyboard);
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
            else if (model.content == "*목록 보기*")
            {
                var tasks = (await _taskService.GetTasksAsync()).ToArray();
                return Json(new MessageResponse
                {
                    keyboard = new Keyboard
                    {
                        type = KeyboardType.Buttons,
                        buttons = tasks.Select(t => t.Title + "\n" + "[" + t.Id.ToString().ToLower() + "]").Concat(new [] {"*처음으로*"}).ToArray()
                    },
                    message = new Message
                    {
                        text = String.Join("\n", tasks.Select(t => String.Join("", t.Categories.Select(c => "[" + c.Category.FullName + "]")) + " " + t.Title).ToArray())
                    }
                });
            }
            else if (model.content == "*처음으로*")
            {
                return Json(new MessageResponse
                {
                    keyboard = DefaultKeyboard,
                    message = new Message
                    {
                        text = "명령을 눌러주세요"
                    }
                });
            }
            else if (model.content == "*카테고리 보기*")
            {
                var categories = await _categoryService.GetCategoriesAsync();
                
                return Json(new MessageResponse
                {
                    keyboard = new Keyboard
                    {
                        type = KeyboardType.Buttons,
                        buttons = categories.Select(c => $"{c.FullName}\n[c-{c.Id}]").Concat(new [] { "*처음으로*" }).ToArray()
                    },
                    message = new Message
                    {
                        text = String.Join("\n", categories.Select(c => $"[{c.FullName}] {c.Tasks.Count}개"))
                    }
                });
            }
            else if (model.content.StartsWith("*완료처리*"))
            {
                var idPattern = @"\[(?<id>\w{8}-\w{4}-\w{4}-\w{4}-\w{12})\]";
                var idMatch = Regex.Match(model.content, idPattern);

                if (idMatch.Success)
                {
                    var id = Guid.Parse(idMatch.Groups["id"].Value);

                    await _taskService.ToggleDoneAsync(id);

                    return Json(new MessageResponse
                    {
                        keyboard = DefaultKeyboard,
                        message = new Message
                        {
                            text = "명령을 눌러주세요"
                        }
                    });
                }
            }
            else if (model.content.StartsWith("*삭제하기*"))
            {
                var idPattern = @"\[(?<id>\w{8}-\w{4}-\w{4}-\w{4}-\w{12})\]";
                var idMatch = Regex.Match(model.content, idPattern);

                if (idMatch.Success)
                {
                    var id = Guid.Parse(idMatch.Groups["id"].Value);

                    await _taskService.ArchiveAsync(id);

                    return Json(new MessageResponse
                    {
                        keyboard = DefaultKeyboard,
                        message = new Message
                        {
                            text = "명령을 눌러주세요"
                        }
                    });
                }
            }
            else
            {
                var idPattern = @"\[(?<id>\w{8}-\w{4}-\w{4}-\w{4}-\w{12})\]";
                var catIdPattern = @"\[c-(?<id>\w{8}-\w{4}-\w{4}-\w{4}-\w{12})\]";

                var idMatch = Regex.Match(model.content, idPattern);
                var catIdMatch = Regex.Match(model.content, catIdPattern);

                if (idMatch.Success)
                {
                    var id = Guid.Parse(idMatch.Groups["id"].Value);
                    var task = await _taskService.FindTaskAsync(id);

                    return Json(new MessageResponse
                    {
                        keyboard = new Keyboard
                        {
                            type = KeyboardType.Buttons,
                            buttons = new[]
                            {
                                $"*완료처리*\n[{id}]",
                                $"*삭제하기*\n[{id}]",
                                "*목록 보기*",
                                "*처음으로*"
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
                else if (catIdMatch.Success)
                {
                    var id = Guid.Parse(catIdMatch.Groups["id"].Value);
                    var category = await _categoryService.FindCategoryAsync(id);
                    var tasks = category.Tasks.Select(t => t.Task);

                    return Json(new MessageResponse
                    {
                        keyboard = new Keyboard
                        {
                            type = KeyboardType.Buttons,
                            buttons = tasks.Select(t => t.Title + "\n" + "[" + t.Id.ToString().ToLower() + "]").Concat(new[] { "*처음으로*" }).ToArray()
                        },
                        message = new Message
                        {
                            text = String.Join("\n", tasks.Select(t => String.Join("", t.Categories.Select(c => "[" + c.Category.FullName + "]")) + " " + t.Title).ToArray())
                        }
                    });
                }

                if (!model.content.StartsWith("*"))
                {
                    await _taskService.CreateTaskAsync(model.content, null);

                    return Json(new MessageResponse
                    {
                        keyboard = DefaultKeyboard,
                        message = new Message { text = "추가되었습니다" }
                    });
                }
            }

            return Json(new MessageResponse
            {
                keyboard = DefaultKeyboard,
                message = new Message
                {
                    text = "명령을 눌러주세요"
                }
            });
        }
    }
}
