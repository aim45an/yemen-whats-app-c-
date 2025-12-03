using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YemenWhatsApp.Models
{
    public class ApiResponse<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("data")]
        public T? Data { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ApiResponse(bool success = true, string message = "")
        {
            Success = success;
            Message = message;
        }

        public static ApiResponse<T> CreateSuccess(T data, string message = "‰Ã«Õ")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.Now
            };
        }

        public static ApiResponse<T> CreateError(string error, string message = "Œÿ√")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = error,
                Timestamp = DateTime.Now
            };
        }
    }

    public class AuthResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;

        [JsonProperty("user")]
        public User? User { get; set; }

        [JsonProperty("expiresIn")]
        public int ExpiresIn { get; set; } = 86400; // 24 ”«⁄…

        public AuthResponse(bool success = false, string message = "")
        {
            Success = success;
            Message = message;
        }
    }

    public class UserListResponse
    {
        [JsonProperty("users")]
        public List<User> Users { get; set; } = new List<User>();

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("onlineCount")]
        public int OnlineCount { get; set; }
    }

    public class MessageListResponse
    {
        [JsonProperty("messages")]
        public List<Message> Messages { get; set; } = new List<Message>();

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }
    }
}