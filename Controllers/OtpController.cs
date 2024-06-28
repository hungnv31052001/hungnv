using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobWebApplicationvip.Controllers
{
    public class OtpController : Controller
    {
        private readonly ISmsService _smsService;
        private static readonly Dictionary<string, string> otpStore = new Dictionary<string, string>();

        public OtpController(ISmsService smsService)
        {
            _smsService = smsService;
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp([FromBody] OtpRequest request)
        {
            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                return BadRequest("Phone number is required.");
            }

            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu OTP vào bộ nhớ tạm (hoặc cơ sở dữ liệu) với thời gian hết hạn
            otpStore[request.PhoneNumber] = otp;

            await _smsService.SendSmsAsync(request.PhoneNumber, $"Your OTP code is {otp}");

            return Ok("OTP sent successfully");
        }

        [HttpPost]
        public IActionResult VerifyOtp([FromBody] OtpVerifyRequest request)
        {
            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Otp))
            {
                return BadRequest("Phone number and OTP are required.");
            }

            if (otpStore.ContainsKey(request.PhoneNumber) && otpStore[request.PhoneNumber] == request.Otp)
            {
                otpStore.Remove(request.PhoneNumber); // Xóa OTP sau khi xác minh thành công
                return Ok("OTP verified successfully");
            }

            return BadRequest("Invalid OTP");
        }

        public class OtpRequest
        {
            public string PhoneNumber { get; set; }
        }

        public class OtpVerifyRequest
        {
            public string PhoneNumber { get; set; }
            public string Otp { get; set; }
        }
    }
}
