using BlazorWebSocket.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace BlazorWebSocket.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebSocketsController : ControllerBase
    {
        static ConcurrentDictionary<string, WebSocket> mUsers = new();

        [Route("Get")]
        public async Task<ActionResult> Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string pUsername;
                Microsoft.Extensions.Primitives.StringValues pValues;
                var pWebSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                Request.Query.TryGetValue("Username", out pValues);
                if (pValues.Count == 0)
                {
                    await SendMessage(Message.ToString(Message.eType.Error, "Username is required in the query string."), null, pWebSocket);
                    await pWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    return BadRequest();
                }
                if (pValues.Count != 1)
                {
                    await SendMessage(Message.ToString(Message.eType.Error, "Username query string must contain only 1 item."), null, pWebSocket);
                    await pWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    return BadRequest();
                }
                pUsername = pValues[0];
                if (string.IsNullOrWhiteSpace(pUsername))
                {
                    await SendMessage(Message.ToString(Message.eType.Error, "Username must not be blank."), null, pWebSocket);
                    await pWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    return BadRequest();
                }
                pUsername = pUsername.Trim();

                if (mUsers.ContainsKey(pUsername.ToLower()))
                {
                    await SendMessage(Message.ToString(Message.eType.Error, "Username, " + pUsername + ", already exist. Please choose a unique client name."), null, pWebSocket);
                    await pWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    return BadRequest();
                }
                mUsers.TryAdd(pUsername.ToLower(), pWebSocket);

                var pReceiveMessageTask = ReceiveMessage(pWebSocket);
                _ = SendMessage(Message.ToString(Message.eType.System, "User, " + pUsername + ", is online"), pUsername);
                _ = SendMessage(Message.ToString(Message.eType.LoginSuccess, ""), null, pWebSocket);
                await pReceiveMessageTask;
                return Ok();
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                return BadRequest("This action is for websockets only.");
            }
        }

        async Task ReceiveMessage(WebSocket vWebSocket)
        {
            var pBytes = new byte[1024 * 4];
            WebSocketReceiveResult pResult;
            while (true)
            {
                pResult = await vWebSocket.ReceiveAsync(new ArraySegment<byte>(pBytes), CancellationToken.None);
                if (pResult.CloseStatus.HasValue)
                    break;
                var pText = Encoding.UTF8.GetString(pBytes, 0, pResult.Count);

                // todo: analyze the received message here and do your own logic

                // this line will broadcast the received message to other listeners. 
                // for your own project, this line is optional.
                await SendMessage(pText, null);
            }
            await vWebSocket.CloseAsync(pResult.CloseStatus.Value, pResult.CloseStatusDescription, CancellationToken.None);
            string pUsername = null;
            var pToRemove = mUsers.Where(x => x.Value == vWebSocket);
            if (pToRemove.Count() > 0)
            {
                pUsername = pToRemove.First().Key;
                WebSocket pDummy;
                mUsers.TryRemove(pUsername, out pDummy);
            }

            if (!string.IsNullOrWhiteSpace(pUsername))
                await SendMessage(Message.ToString(Message.eType.System, "User, " + pUsername + ", has disconnected."), null);
        }

        static async Task SendMessage(string vMessage, string vSenderUsername, WebSocket vDestinationWebSocket = null)
        {
            var pBytes = Encoding.UTF8.GetBytes(vMessage);
            var pArraySegment = new ArraySegment<byte>(pBytes, 0, pBytes.Length);

            if (vDestinationWebSocket == null)
                foreach (var pUser in mUsers)
                {
                    if ((vSenderUsername == null)
                        || (vSenderUsername != pUser.Key))
                    {
                        await pUser.Value.SendAsync(pArraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            else
                await vDestinationWebSocket.SendAsync(pArraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
