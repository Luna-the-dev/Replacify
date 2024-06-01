﻿using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TextReplace.Messages.Replace
{
    class IsReplaceFileUnsavedMsg(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
