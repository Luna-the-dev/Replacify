﻿using CommunityToolkit.Mvvm.Messaging.Messages;
using TextReplace.MVVM.Model;

namespace TextReplace.Messages.Replace
{
    public class SelectedReplacePhraseMsg(ReplacePhrase value) : ValueChangedMessage<ReplacePhrase>(value)
    {
    }
}
