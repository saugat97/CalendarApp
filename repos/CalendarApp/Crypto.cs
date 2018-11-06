﻿using System;
using System.Text;

namespace CalendarApp
{
    public static class Crypto
    {
        public static string HashPassword(string value)
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(value))
                );
        }
    }
}