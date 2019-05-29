using System;

namespace ChatApp.Core
{
    public static class HubMethodExtensions
    {
        public static string GetHubMethodName(this HubMessages.HubMethod self) => Enum.GetName(self.GetType(), self);
    }
}