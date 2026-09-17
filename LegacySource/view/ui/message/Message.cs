using System;
using System.Runtime.Serialization;

namespace View.UI.Message
{
    [Serializable]
    public abstract class Message : ISerializable
    {
        protected const int WIDTH = 600;
        protected const int HEIGHT = 600;
        private static readonly long serialVersionUID = 1L;
        protected bool isRead = false;
        protected transient RENDEROBJ section;
        protected double currentSecond = -1;
        private readonly string title;
        protected readonly string key;

        public Message(ICharSequence title)
        {
            if (title == null)
                throw new Exception("");

            this.title = title.ToString();

            string k = "";
            foreach (var e in new Exception().StackTrace)
            {
                k += e.ToString().GetHashCode();
            }

            key = k;
        }

        protected abstract RENDEROBJ MakeSection();

        protected RENDEROBJ Section()
        {
            return section;
        }

        public bool Send()
        {
            currentSecond = TIME.CurrentSecond();
            return VIEW.Messages().Add(this);
        }

        protected string Title()
        {
            return title;
        }

        protected void Close()
        {
            VIEW.Messages().Hide();
        }
    }

    public interface ICharSequence
    {
        string ToString();
    }

    public class TIME
    {
        public static double CurrentSecond()
        {
            // Implement this method to get the current second
            throw new NotImplementedException();
        }
    }

    public class VIEW
    {
        public static Messages Messages()
        {
            // Implement this method to get the messages instance
            throw new NotImplementedException();
        }
    }

    public class Messages
    {
        public bool Add(Message message)
        {
            // Implement this method to add a message
            throw new NotImplementedException();
        }

        public void Hide()
        {
            // Implement this method to hide messages
            throw new NotImplementedException();
        }
    }
}