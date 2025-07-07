using System;
using UnityEngine;

namespace dk.QA
{
    public abstract class QaCommandBase 
    {
        public string Description { get; protected set; }

        public abstract void Execute();
    }

    public class ButtonCommand : QaCommandBase
    {
        public string btnText;
        public Action action;

        public ButtonCommand(string btnText, Action action)
        {
            base.Description = btnText;
            this.btnText = btnText;
            this.action = action;
        }

        public override void Execute()
        {
            action();
        }
    }

    public class KeyCodeCommand : QaCommandBase
    {
        public KeyCode Key;
        public Action action;

        public KeyCodeCommand(KeyCode key, Action action)
        {
            this.Description = action.Method.Name;
            this.Key = key;
            this.action = action;
        }

        public KeyCodeCommand(KeyCode key, string desc, Action action)
        {
            base.Description = desc;
            this.Key = key;
            this.action = action;
        }

        public override void Execute()
        {
            action();
        }
    }

    public abstract class StringCommandBase : QaCommandBase
    {
        public string Key;
    }

    public class StringCommand : StringCommandBase
    {
        public Action action;

        public StringCommand(string key, Action action)
        {
            this.Description = action.Method.Name.Length > 0 ? action.Method.Name : key.Replace("/", "");
            base.Key = key;
            this.action = action;
        }

        public StringCommand(string key, string desc, Action action)
        {
            base.Description = desc;
            base.Key = key;
            this.action = action;
        }

        public override void Execute()
        {
            action();
        }
    }

    public class StringCommand<T1> : StringCommandBase
    {
        public Action<T1> action;

        public StringCommand(string key, Action<T1> action)
        {
            this.Description = action.Method.Name.Length > 0 ? action.Method.Name : key.Replace("/", "");
            base.Key = key;
            this.action = action;
        }

        public StringCommand(string key, string desc, Action<T1> action)
        {
            base.Description = desc;
            base.Key = key;
            this.action = action;
        }

        public override void Execute()
        {
            Debug.LogWarning("Should call Execute with parameters");
        }

        public void Execute(T1 param1)
        {
            action(param1);
        }
    }

    public class StringCommand<T1, T2> : StringCommandBase
    {
        public Action<T1, T2> action;

        public StringCommand(string key, Action<T1, T2> action)
        {
            this.Description = action.Method.Name.Length > 0 ? action.Method.Name : key.Replace("/", "");
            base.Key = key;
            this.action = action;
        }

        public StringCommand(string key, string desc, Action<T1, T2> action)
        {
            base.Description = desc;
            base.Key = key;
            this.action = action;
        }

        public override void Execute()
        {
            Debug.LogWarning("Should call Execute with parameters");
        }

        public void Execute(T1 param1, T2 param2)
        {
            action(param1, param2);
        }
    }

    public class StringCommand<T1, T2, T3> : StringCommandBase
    {
        public Action<T1, T2, T3> action;
        
        public StringCommand(string key, Action<T1, T2, T3> action)
        {
            this.Description = action.Method.Name.Length > 0 ? action.Method.Name : key.Replace("/", "");
            base.Key = key;
            this.action = action;
        }

        public StringCommand(string key, string desc, Action<T1, T2, T3> action)
        {
            base.Description = desc;
            base.Key = key;
            this.action = action;
        }

        public override void Execute()
        {
            Debug.LogWarning("Should call Execute with parameters");
        }

        public void Execute(T1 param1, T2 param2, T3 param3)
        {
            action(param1, param2, param3);
        }
    }

    public class StringCommand<T1, T2, T3, T4> : StringCommandBase
    {
        public Action<T1, T2, T3, T4> action;
        
        public StringCommand(string key, Action<T1, T2, T3, T4> action)
        {
            this.Description = action.Method.Name.Length > 0 ? action.Method.Name : key.Replace("/", "");
            base.Key = key;
            this.action = action;
        }

        public StringCommand(string key, string desc, Action<T1, T2, T3, T4> action)
        {
            base.Description = desc;
            base.Key = key;
            this.action = action;
        }

        public override void Execute()
        {
        }

        public void Execute(T1 param1, T2 param2, T3 param3, T4 param4)
        {
            action(param1, param2, param3, param4);
        }
    }
}