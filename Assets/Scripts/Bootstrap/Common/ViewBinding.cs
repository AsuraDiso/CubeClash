using System;

namespace Bootstrap.Common
{
    public static class ViewBinding
    {
        public static void Rebind<T>(ref T current, T next, Action<T> subscribe, Action<T> unsubscribe)
            where T : class
        {
            if (current != null)
            {
                unsubscribe(current);
            }

            current = next;

            if (current != null)
            {
                subscribe(current);
            }
        }

        public static void Unbind<T>(ref T current, Action<T> unsubscribe)
            where T : class
        {
            if (current == null)
            {
                return;
            }

            unsubscribe(current);
            current = null;
        }

        public static void Switch<T>(
            ref T current,
            T next,
            Action<T> subscribe,
            Action<T> unsubscribe)
            where T : class
        {
            if (current == next)
            {
                return;
            }

            Rebind(ref current, next, subscribe, unsubscribe);
        }
    }
}
