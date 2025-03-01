using System;
using System.Collections.Concurrent;

namespace MyECommerce.Services
{
    public class OtpService
    {
        private static ConcurrentDictionary<string, (string Otp, DateTime Expiry)> otpStorage = new();
        private static readonly Random _random = new();

        public string GenerateOtp(string key)
        {
            string otp = _random.Next(100000, 999999).ToString();
            otpStorage[key] = (otp, DateTime.UtcNow.AddMinutes(5));
            return otp;
        }

        public bool VerifyOtp(string key, string otp)
        {
            if (otpStorage.TryGetValue(key, out var otpEntry))
            {
                if (otpEntry.Otp == otp && otpEntry.Expiry > DateTime.UtcNow)
                {
                    otpStorage.TryRemove(key, out _);
                    return true;
                }
            }
            return false;
        }
    }
}
