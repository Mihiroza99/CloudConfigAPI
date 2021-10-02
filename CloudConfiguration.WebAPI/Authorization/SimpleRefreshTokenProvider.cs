using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

namespace CloudConfiguration.WebAPI.Authorization
{
    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {
        #region Constants

        private const int TOKEN_REFRESH_HOURS = 12;

        #endregion

        #region Private Members

        private static ConcurrentDictionary<string, AuthenticationTicket> _refreshTokens = new ConcurrentDictionary<string, AuthenticationTicket>();

        #endregion

        #region Public Methods

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            string refreshToken = Guid.NewGuid().ToString();

            AuthenticationProperties refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                ExpiresUtc = DateTime.UtcNow.AddHours(TOKEN_REFRESH_HOURS)
            };

            _refreshTokens.TryAdd(refreshToken, new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties));

            context.SetToken(refreshToken);
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            if (_refreshTokens.TryRemove(context.Token, out AuthenticationTicket ticket))
            {
                context.SetTicket(ticket);
            }
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
