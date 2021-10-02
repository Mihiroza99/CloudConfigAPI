using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Microsoft.Owin.Security.OAuth;


namespace CloudConfiguration.WebAPI.Authorization
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        #region Constants

        private const string CRYPTO_ENCODING_KEY = "!@trABvf%^&$09GH";
        private const string CRYPTO_ENCODING_IV = "2811da22377d62fc";

        private const int MAX_LOGIN_FAIL_COUNT = 3;

        #endregion

        #region Private Members

        private static Dictionary<string, int> _loginFailDirectory = new Dictionary<string, int>();

        #endregion

        #region OAuthAuthorizationServerProvider

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            bool isValid = false;
            bool isBlocked = false;

            //using (RosterDBEntities entities = new RosterDBEntities())
            //{
            //    try
            //    {
            //        T_Users loginUser = entities.T_Users.Where((T_Users user) => user.UserName == context.UserName).FirstOrDefault();
            //        if (loginUser != null)
            //        {
            //            isBlocked = loginUser.IsBlocked.HasValue && loginUser.IsBlocked.Value;

            //            if (loginUser.UserPassword == EncryptString(context.Password))
            //            {
            //                isValid = true;
            //                _loginFailDirectory.Remove(context.UserName);
            //            }
            //            else if (!isBlocked && !context.UserName.Equals("Charleston", StringComparison.OrdinalIgnoreCase))
            //            {
            //                if (!_loginFailDirectory.ContainsKey(context.UserName))
            //                {
            //                    _loginFailDirectory[context.UserName] = 1;
            //                }
            //                else
            //                {
            //                    int loginFailCount = _loginFailDirectory[context.UserName];
            //                    if (++loginFailCount >= MAX_LOGIN_FAIL_COUNT)
            //                    {
            //                        loginUser.IsBlocked = true;
            //                        entities.SaveChanges();
            //                    }
            //                    else
            //                    {
            //                        _loginFailDirectory[context.UserName] = loginFailCount;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    catch
            //    {
            //        context.SetError("Exception");
            //        return;
            //    }
            //}

            if (!isValid)
            {
                context.SetError("Invalid");
            }
            else if (isBlocked)
            {
                context.SetError("Blocked");
            }
            else
            {
                context.Validated(new ClaimsIdentity(context.Options.AuthenticationType));
            }
        }

        #endregion

        #region Private Methods

        private string EncryptString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            string encryptedText = null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(CRYPTO_ENCODING_KEY);
                aes.IV = Encoding.UTF8.GetBytes(CRYPTO_ENCODING_IV);

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(text);
                        }

                        encryptedText = Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }

            return encryptedText;
        }

        #endregion
    }
}
