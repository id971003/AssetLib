using System;
using System.Collections.Generic;
using UnityEngine;

namespace dk.QA
{
    public abstract class QaCommandContainer
    {
        private List<QaCommandBase> _commands = new List<QaCommandBase>();

        public List<QaCommandBase> GetCommands()
        {
            if(_commands.Count == 0)
            {
                CreateQaCommands();
            }
            
            return _commands;
        }

        protected void CreateStringCommand(string key, Action action)
        {
            QaCommandBase command = new StringCommand(key, action);
            _commands.Add(command);
        }

        protected void CreateStringCommand(string key, string desc, Action action)
        {
            QaCommandBase command = new StringCommand(key, desc, action);
            _commands.Add(command);
        }

        protected void CreateStringCommand<T1>(string key, string desc, Action<T1> action)
        {
            QaCommandBase command = new StringCommand<T1>(key, desc, action);
            _commands.Add(command);
        }

        protected void CreateStringCommand<T1, T2>(string key, string desc, Action<T1, T2> action)
        {
            QaCommandBase command = new StringCommand<T1, T2>(key, desc, action);
            _commands.Add(command);
        }

        protected void CreateStringCommand<T1, T2, T3>(string key, string desc, Action<T1, T2, T3> action)
        {
            QaCommandBase command = new StringCommand<T1, T2, T3>(key, desc, action);
            _commands.Add(command);
        }

        protected void CreateStringCommand<T1, T2, T3, T4>(string key, string desc, Action<T1, T2, T3, T4> action)
        {
            QaCommandBase command = new StringCommand<T1, T2, T3, T4>(key, desc, action);
            _commands.Add(command);
        }

        protected void CreateKeyCodeCommand(KeyCode key, Action action)
        {
            QaCommandBase command = new KeyCodeCommand(key, action);
            _commands.Add(command);
        }

        protected void CreateKeyCodeCommand(KeyCode key, string desc, Action action)
        {
            QaCommandBase command = new KeyCodeCommand(key, desc, action);
            _commands.Add(command);
        }

        protected void CreateButtonCommand(string btnText, Action action)
        {
            QaCommandBase command = new ButtonCommand(btnText, action);
            _commands.Add(command);
        }

        protected abstract void CreateQaCommands();
    }
}
