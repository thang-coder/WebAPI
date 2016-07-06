using Microsoft.IdentityModel.Tokens;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace WebAPI.SignatureTypes
{
    public class HmacSignatureFactory
    {
        // The hash function gets replaced every time the secret string changes.
        private static HMACSHA256 hashFunction;

        private static string secret;

        // The signing and verifying key gets replaced every time the secret string changes.
        private static SymmetricSecurityKey signingAndVerifyingKey;

        // Changing the secret string will reset the hash function.
        public static string Secret
        {
            get
            {
                return secret;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException();
                }

                secret = value;
                hashFunction = new HMACSHA256(Encoding.UTF8.GetBytes(value));
                signingAndVerifyingKey = new SymmetricSecurityKey(hashFunction.Key);
            }
        }

        // Define which properties of a JWT are required and how to validate the token's signature
        public static TokenValidationParameters ValidationParameters { get; internal set; }

        // static constructor
        static HmacSignatureFactory()
        {
            // Define which properties of a JWT are required and how to validate the token's signature
            ValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                SignatureValidator = VerifySignature
            };
        }

        public static string CreateSignature(JwtSecurityToken jwt)
        {
            if (jwt == null)
            {
                throw new ArgumentNullException(nameof(jwt));
            }
            var protectedContent = Encoding.ASCII.GetBytes($"{jwt.EncodedHeader}.{jwt.EncodedPayload}");
            var hash = hashFunction.ComputeHash(protectedContent);
            return Base64UrlEncoder.Encode(hash);
        }

        public static SecurityToken VerifySignature(string token, TokenValidationParameters validationParameters)
        {
            JwtSecurityToken jwt = null;
            string computedSignature = null;
            try
            {
                jwt = new JwtSecurityToken(token);
                computedSignature = CreateSignature(jwt);
            }
            catch (Exception error)
            {
                Trace.TraceError($"Cannot verify signature. Error: {error.ToString()}");
            }

            if (string.Compare(computedSignature, jwt.RawSignature) == 0)
            {
                return jwt;
            }

            return null;
        }
    }
}