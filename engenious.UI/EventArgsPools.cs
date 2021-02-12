using System.Collections.Generic;
using engenious.Input;

namespace engenious.UI
{
    internal abstract class EventArgsPool<T> where T : EventArgs,new()
    {
        private readonly Stack<T> _freeList = new Stack<T>(16);
        private readonly object _lockObj = new object();

        public T Take()
        {
            if (_freeList.Count <= 0) return new T();
            lock (_lockObj)
            {
                if (_freeList.Count > 0)
                {
                    return _freeList.Pop();
                }
            }

            return new T();
        }
        public void Release(T arr)
        {
            if (arr == null) return;
            ResetVariable(arr);

            lock (_lockObj)
            {
                _freeList.Push(arr);
            }
        }

        protected abstract void ResetVariable(T arr);
    }
    internal class EventArgsPool : EventArgsPool<EventArgs>
    {
        private static EventArgsPool _instance;
        public static EventArgsPool Instance => _instance ??= new EventArgsPool();

        protected override void ResetVariable(EventArgs arr)
        {
            arr.Handled = false;
        }
    }
    internal class DragEventArgsPool : EventArgsPool<DragEventArgs>
    {
        private static DragEventArgsPool _instance;
        public static DragEventArgsPool Instance => _instance ??= new DragEventArgsPool();

        protected override void ResetVariable(DragEventArgs arr)
        {
            arr.Handled = false;
            arr.Sender = null;
            arr.Icon = null;
            arr.IconSize = Point.Zero;
            arr.Content = null;
        }
    }
    
    internal class MouseEventArgsPool : EventArgsPool<MouseEventArgs>
    {
        private static MouseEventArgsPool _instance;
        public static MouseEventArgsPool Instance => _instance ??= new MouseEventArgsPool();

        protected override void ResetVariable(MouseEventArgs arr)
        {
            arr.Handled = false;
            arr.MouseMode = MouseMode.Captured;
            arr.Bubbled = false;
            arr.GlobalPosition = Point.Zero;
            arr.LocalPosition = Point.Zero;
        }
    }
    
    internal class KeyEventArgsPool : EventArgsPool<KeyEventArgs>
    {
        private static KeyEventArgsPool _instance;
        public static KeyEventArgsPool Instance => _instance ??= new KeyEventArgsPool();

        protected override void ResetVariable(KeyEventArgs arr)
        {
            arr.Key = Keys.Unknown;
            arr.Alt = false;
            arr.Shift = false;
            arr.Ctrl = false;
            arr.Handled = false;
        }
    }
}