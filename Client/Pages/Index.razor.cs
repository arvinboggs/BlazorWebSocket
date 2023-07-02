using BlazorWebSocket.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Newtonsoft.Json;

namespace BlazorWebSocket.Client.Pages
{
    public partial class Index : ComponentBase
    {
        string mMessage;
        string mLog;
        MudTextField<string> mTxtMessage;

        [Inject] NavigationManager mNavigationManager { get; set; }

        protected override void OnInitialized()
        {
            if (string.IsNullOrWhiteSpace(WebSocketClientConnection.Username))
            {
                mNavigationManager.NavigateTo("/login");
                return;
            }

            try
            {
                WebSocketClientConnection.AddListener(x =>
                {
                    ProcessMessage(x);
                });
                mLog += "Welcome, " + WebSocketClientConnection.Username + "\n";
                mLog += "To test this web app, open this page using another browser.\n";
            }
            catch (Exception ex)
            {
                mLog += ex.Message + "\n";
            }
            base.OnInitialized();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                mTxtMessage.FocusAsync();

            base.OnAfterRender(firstRender);
        }

        void ProcessMessage(String vText)
        {
            var pMessage = Message.FromString(vText);
            if (pMessage == null)
                return;

            switch (pMessage.MessageType)
            {
                case Message.eType.Text:
                    mLog += pMessage.Sender + ": " + pMessage.Text.Trim() + "\n";
                    break;

                case Message.eType.System:
                    mLog += "System: " + pMessage.Text.Trim() + "\n";
                    break;

                case Message.eType.Error:
                    mLog += "Error: " + pMessage.Text.Trim() + "\n";
                    break;
            }
            StateHasChanged();
        }

        void butSend_Click()
        {
            if (string.IsNullOrWhiteSpace(mMessage))
                return;
            var pMessage = Message.ToString(Message.eType.Text, WebSocketClientConnection.Username, mMessage);
            WebSocketClientConnection.SendStringAsync(pMessage);
            mMessage = "";
        }

        void txtMessage_OnKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
                butSend_Click();
        }
    }
}
