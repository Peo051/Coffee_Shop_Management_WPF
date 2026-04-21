using System;
using System.Text.RegularExpressions;

namespace CoffeeShop.Wpf.Helpers
{
    /// <summary>
    /// Lớp hỗ trợ validation dữ liệu đầu vào
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Kiểm tra số điện thoại có hợp lệ không
        /// Quy tắc: Phải có đúng 10 chữ số và bắt đầu bằng số 0
        /// </summary>
        /// <param name="phone">Số điện thoại cần kiểm tra</param>
        /// <returns>Thông báo lỗi nếu không hợp lệ, null nếu hợp lệ</returns>
        public static string? ValidatePhoneNumber(string? phone)
        {
            // Cho phép null hoặc empty (trường không bắt buộc)
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            // Loại bỏ khoảng trắng
            phone = phone.Trim();

            // Kiểm tra độ dài
            if (phone.Length != 10)
            {
                return "Số điện thoại phải có đúng 10 chữ số";
            }

            // Kiểm tra bắt đầu bằng số 0
            if (!phone.StartsWith("0"))
            {
                return "Số điện thoại phải bắt đầu bằng số 0";
            }

            // Kiểm tra chỉ chứa chữ số
            if (!Regex.IsMatch(phone, @"^\d{10}$"))
            {
                return "Số điện thoại chỉ được chứa chữ số";
            }

            return null; // Hợp lệ
        }

        /// <summary>
        /// Kiểm tra trường bắt buộc có được nhập hay không
        /// </summary>
        /// <param name="value">Giá trị cần kiểm tra</param>
        /// <param name="fieldName">Tên trường (để hiển thị trong thông báo lỗi)</param>
        /// <returns>Thông báo lỗi nếu không hợp lệ, null nếu hợp lệ</returns>
        public static string? ValidateRequired(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return $"{fieldName} không được để trống";
            }

            return null; // Hợp lệ
        }

        /// <summary>
        /// Kiểm tra số nguyên có phải là số dương hay không
        /// </summary>
        /// <param name="value">Giá trị cần kiểm tra</param>
        /// <param name="fieldName">Tên trường (để hiển thị trong thông báo lỗi)</param>
        /// <returns>Thông báo lỗi nếu không hợp lệ, null nếu hợp lệ</returns>
        public static string? ValidatePositiveNumber(int value, string fieldName)
        {
            if (value <= 0)
            {
                return $"{fieldName} phải lớn hơn 0";
            }

            return null; // Hợp lệ
        }

        /// <summary>
        /// Kiểm tra số thập phân có phải là số dương hay không
        /// </summary>
        /// <param name="value">Giá trị cần kiểm tra</param>
        /// <param name="fieldName">Tên trường (để hiển thị trong thông báo lỗi)</param>
        /// <returns>Thông báo lỗi nếu không hợp lệ, null nếu hợp lệ</returns>
        public static string? ValidatePositiveNumber(decimal value, string fieldName)
        {
            if (value <= 0)
            {
                return $"{fieldName} phải lớn hơn 0";
            }

            return null; // Hợp lệ
        }

        /// <summary>
        /// Kiểm tra khoảng thời gian có hợp lệ không (từ ngày <= đến ngày)
        /// </summary>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <returns>Thông báo lỗi nếu không hợp lệ, null nếu hợp lệ</returns>
        public static string? ValidateDateRange(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
            {
                return "Từ ngày phải nhỏ hơn hoặc bằng Đến ngày";
            }

            return null; // Hợp lệ
        }

        /// <summary>
        /// Kiểm tra độ dài tối đa của chuỗi
        /// </summary>
        /// <param name="value">Giá trị cần kiểm tra</param>
        /// <param name="fieldName">Tên trường</param>
        /// <param name="maxLength">Độ dài tối đa</param>
        /// <returns>Thông báo lỗi nếu không hợp lệ, null nếu hợp lệ</returns>
        public static string? ValidateMaxLength(string? value, string fieldName, int maxLength)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            {
                return $"{fieldName} không được vượt quá {maxLength} ký tự";
            }

            return null; // Hợp lệ
        }
    }
}
