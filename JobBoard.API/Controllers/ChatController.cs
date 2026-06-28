using JobBoard.Core.DTOs.Chat;
using JobBoard.Core.DTOs.Common;
using JobBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobBoard.API.Controllers
{
    [ApiController]
    [Route("api/chats")]
    [Authorize]
    [Produces("application/json")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService) => _chatService = chatService;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        [Authorize(Roles = "employer")]
        public async Task<IActionResult> Start([FromBody] StartChatDto dto)
        {
            var result = await _chatService.StartConversationAsync(UserId, dto.ApplicationId);
            return Ok(ApiResponse<ChatConversationDto>.Ok(result, "Söhbət açıldı."));
        }

        [HttpGet]
        public async Task<IActionResult> GetMy()
        {
            var result = await _chatService.GetConversationsAsync(UserId);
            return Ok(ApiResponse<IEnumerable<ChatConversationDto>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _chatService.GetConversationAsync(UserId, id);
            return Ok(ApiResponse<ChatConversationDetailDto>.Ok(result));
        }

        [HttpPost("{id:int}/messages")]
        public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageDto dto)
        {
            var result = await _chatService.SendMessageAsync(UserId, id, dto.Content);
            return Ok(ApiResponse<ChatMessageDto>.Ok(result));
        }

        [HttpPatch("{id:int}/close")]
        [Authorize(Roles = "employer")]
        public async Task<IActionResult> Close(int id)
        {
            await _chatService.CloseConversationAsync(UserId, id);
            return Ok(ApiResponse.Ok("Söhbət bağlandı."));
        }
    }
}
