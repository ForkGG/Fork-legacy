using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Fork.Annotations;

namespace Fork.Logic.Model.ServerConsole;

public class ConsoleMessage : INotifyPropertyChanged
{
    public enum MessageLevel
    {
        INFO,
        WARN,
        ERROR,
        SUCCESS
    }

    public ConsoleMessage(string content)
    {
        Content = content;
        Level = CategorizeContent(content);
        CreationTime = DateTime.Now;
    }

    public ConsoleMessage(string content, MessageLevel level)
    {
        Content = content;
        Level = level;
        CreationTime = DateTime.Now;
    }

    public string Content { get; }
    public MessageLevel Level { get; }
    public DateTime CreationTime { get; }

    /// <summary>
    ///     SubContent are messages with a very similar content as this message.
    ///     This is used instead of a new message to reduce application lag when console is spammed
    ///     (shout-outs to the Chunky plugin ^^)
    /// </summary>
    public int SubContents { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    private MessageLevel CategorizeContent(string content)
    {
        if (content.Contains("ERROR") || content.Contains("Exception") || content.Trim().StartsWith("at "))
        {
            return MessageLevel.ERROR;
        }

        if (content.Contains("WARN"))
        {
            return MessageLevel.WARN;
        }

        return MessageLevel.INFO;
    }

    public override string ToString()
    {
        if (SubContents == 0)
        {
            return Content;
        }

        return Content + "\n    + " + SubContents + " suppressed messages";
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}