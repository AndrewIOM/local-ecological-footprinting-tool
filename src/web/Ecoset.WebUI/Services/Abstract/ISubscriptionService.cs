using System;
using System.Collections.Generic;
using Ecoset.WebUI.Enums;
using Ecoset.WebUI.Models;

namespace Ecoset.WebUI.Services.Abstract
{
    public interface ISubscriptionService
    {
        Subscription GetActiveForUser(string userId);
        void Revoke(string subsciptionId);
    }
}