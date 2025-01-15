using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BorisMobile.Helper
{
    public class SyncUpdateMessage : ValueChangedMessage<string>
    {
        public SyncUpdateMessage(string value) : base(value)
        {

        }
    }
}
