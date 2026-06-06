using System;
using AvaloniaAppCrypto.Interfaces;

namespace AvaloniaAppCrypto.Services
{
    public class ToastService : IToastService
    {
        public event Action<string, ToastType, int>? OnToast;

        public void Show(string message, ToastType type = ToastType.Info, int durationMs = 4000)
        {
            OnToast?.Invoke(message, type, durationMs);
        }
    }
}
