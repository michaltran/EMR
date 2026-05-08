using System.Security.Cryptography;

namespace EMR.Signing;

/// <summary>
/// Sinh OTP theo RFC 6238 (TOTP HMAC-SHA1, 6 digits, 30s window).
/// VNPT SmartCA cấp TOTP key cho user dạng Base64( hex_string ),
/// vd: "NkY5QTM2RkQzRENGMDRDQUE1QUZGNzc1Qzk2NDdDOEE=" → hex "6F9A36FD3DCF04CAA5AFF775C9647C8A" → 16 bytes.
/// </summary>
public static class TotpGenerator
{
    /// <summary>
    /// Generate TOTP code for current UTC time (default: 6 digits, 30s window, SHA1).
    /// </summary>
    public static string Generate(string vnptKeyBase64, int digits = 6, int periodSeconds = 30)
    {
        var secret = DecodeVnptKey(vnptKeyBase64);
        var counter = (long)Math.Floor(DateTimeOffset.UtcNow.ToUnixTimeSeconds() / (double)periodSeconds);
        return ComputeOtp(secret, counter, digits);
    }

    /// <summary>
    /// Decode key VNPT trả: "NkY5QTM2RkQzRENGMDRDQUE1QUZGNzc1Qzk2NDdDOEE="
    /// → Base64 decode = "6F9A36FD3DCF04CAA5AFF775C9647C8A" (32 hex chars)
    /// → Hex decode = 16 bytes binary secret.
    /// </summary>
    public static byte[] DecodeVnptKey(string keyBase64)
    {
        var step1 = Convert.FromBase64String(keyBase64.Trim());
        var hexString = System.Text.Encoding.ASCII.GetString(step1);
        return Convert.FromHexString(hexString);
    }

    private static string ComputeOtp(byte[] secret, long counter, int digits)
    {
        // Counter as big-endian 8-byte
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);

        using var hmac = new HMACSHA1(secret);
        var hash = hmac.ComputeHash(counterBytes);

        // Dynamic truncation
        var offset = hash[^1] & 0x0F;
        var binary =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);

        var modulo = (int)Math.Pow(10, digits);
        return (binary % modulo).ToString().PadLeft(digits, '0');
    }
}
