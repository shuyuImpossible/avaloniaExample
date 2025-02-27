using avaloniaExample.Services;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace avaloniaExample.Messages;

public class LoginSuccessMessage(AuthenticationResult result) : ValueChangedMessage<AuthenticationResult>(result);
