using BlazorWebSocket.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace BlazorWebSocket.Client.Pages
{
    public partial class Login : ComponentBase
    {
        [Inject] NavigationManager mNavigationManager { get; set; }
        [Inject] IDialogService mDialogService { get; set; }

        SharpBlaze.Shared.LoginInput mModel = new();
        string mMessage = "Just enter anything in the password textbox";

        async Task OnCreateAccountClick()
        {
            await mDialogService.ShowMessageBox("Create Account", "This function will not be implemented. Just enter anything in the password textbox.");
        }

        private void OnValidSubmit(EditContext context)
        {
            mMessage = "Logging in. Please wait.";

            // BEGIN: initialize WebSocket client connection here
            WebSocketClientConnection.NavigationManager = mNavigationManager;
            WebSocketClientConnection.Username = mModel.Username;
            // END: initialize WebSocket client connection here

            // connect and listen
            WebSocketClientConnection.AddListener(async x =>
            {
                WebSocketClientConnection.ClearListeners();
                var pMessage = Message.FromString(x);
                if (pMessage.MessageType == Message.eType.LoginSuccess)
                    mNavigationManager.NavigateTo("#");
                else
                    mMessage = pMessage.Text;
                StateHasChanged(); // required to refresh the text of MudAlert
            });
        }

        private async Task OnClickForgotPassword()
        {
            await mDialogService.ShowMessageBox("Forgot Password", "Just enter anything in the password textbox.");
        }

    }
}
