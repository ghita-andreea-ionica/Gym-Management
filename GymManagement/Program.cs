using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GymManagement
{
    public abstract class User
    {
        private string _password;
        public string Username { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }

        protected User(string username, string password, string fullname)
        {
            Username = username;
            _password = HashPassword(password);
            FullName = fullname;
            CreatedAt = DateTime.Now;
        }

        protected User()
        {
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public bool VerifyPassword(string password)
        {
            return _password == HashPassword(password);
        }

        public (bool success, string message) ChangePassword(string oldPassword, string newPassword)
        {
            if (!VerifyPassword(oldPassword))
                return (false, "Parola veche incorectă!");

            _password = HashPassword(newPassword);
            return (true, "Parola a fost schimbată cu succes!");
        }

        public string GetPasswordHash() => _password;
        public void SetPasswordHash(string hash) => _password = hash;
        public abstract Dictionary<string, object> GetData();
        public abstract string GetUserType();
    }
} 