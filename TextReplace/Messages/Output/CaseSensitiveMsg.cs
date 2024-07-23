﻿using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Output
{
    public class CaseSensitiveMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
