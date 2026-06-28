using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace JobBoard.API.Hubs
{
    /// <summary>
    /// Real-time bildiriş hub-ı. Hər istifadəçi öz "user-{id}" qrupuna qoşulur,
    /// admin istifadəçilər əlavə olaraq "admins" qrupuna qoşulur.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        public static string UserGroup(int userId) => $"user-{userId}";
        public const string AdminsGroup = "admins";

        public override async Task OnConnectedAsync()
        {
            var userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdValue, out var userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

                var role = Context.User?.FindFirstValue(ClaimTypes.Role);
                if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
                    await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);
            }

            await base.OnConnectedAsync();
        }
    }
}
