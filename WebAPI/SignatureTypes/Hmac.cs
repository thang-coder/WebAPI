﻿using Microsoft.IdentityModel.Tokens;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace WebAPI.SignatureTypes
{
    public class Hmac
    {
        // The hash function gets replaced every time the secret string changes.
        private static HMACSHA256 hashFunction;

        private static string secret;

        static Hmac()
        {
            ValidationParameters = new TokenValidationParameters
            {
                SignatureValidator = verifySignature
            };
        }

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
                hashFunction = new HMACSHA256(Convert.FromBase64String(value));
            }
        }

        public static TokenValidationParameters ValidationParameters { get; internal set; }

        private static string ComputeSignature(JwtSecurityToken jwt)
        {
            if (jwt == null)
            {
                throw new ArgumentNullException(nameof(jwt));
            }

            Trace.Assert(!string.IsNullOrWhiteSpace(Secret), "Secret string is required for Hmac algorithm.");

            var protectedContent = Encoding.ASCII.GetBytes($"{jwt.EncodedHeader}.{jwt.EncodedPayload}");
            var hash = hashFunction.ComputeHash(protectedContent);
            return Base64UrlEncoder.Encode(hash);
        }

        private static SecurityToken verifySignature(string token, TokenValidationParameters validationParameters)
        {
            JwtSecurityToken jwt = null;
            string computedSignature = null;
            try
            {
                jwt = new JwtSecurityToken(token);
                computedSignature = ComputeSignature(jwt);
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