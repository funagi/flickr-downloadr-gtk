namespace FloydPink.Flickr.Downloadr.OAuth {
    using System;
    using System.Collections.Generic;
    using DotNetOpenAuth.OAuth.ChannelElements;
    using DotNetOpenAuth.OAuth.Messages;
    using Model;
    using Repository;

    public class TokenManager : IConsumerTokenManager {
        private readonly IRepository<Token> _tokenRepository;

        private readonly Dictionary<string, Tuple<string, TokenType>> _tokens =
            new Dictionary<string, Tuple<string, TokenType>>();

        public TokenManager(string consumerKey, string consumerSecret, IRepository<Token> tokenRepository) {
            if (String.IsNullOrEmpty(consumerKey)) {
                throw new ArgumentNullException("consumerKey");
            }

            _tokenRepository = tokenRepository;

            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;

            GetStoredAccessToken();
        }

        private void GetStoredAccessToken() {
            var token = _tokenRepository.Get();
            if (!string.IsNullOrEmpty(token.TokenString)) {
                _tokens[token.TokenString] = new Tuple<string, TokenType>(token.Secret, TokenType.AccessToken);
            }
        }

        #region IConsumerTokenManager Members

        public string ConsumerKey { get; private set; }

        public string ConsumerSecret { get; private set; }

        public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken,
                                                             string accessTokenSecret) {
            _tokens.Remove(requestToken);
            _tokens[accessToken] = new Tuple<string, TokenType>(accessTokenSecret, TokenType.AccessToken);
            _tokenRepository.Save(new Token(accessToken, accessTokenSecret));
        }

        public string GetTokenSecret(string token) {
            return _tokens[token].Item1;
        }

        public TokenType GetTokenType(string token) {
            return _tokens[token].Item2;
        }

        public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response) {
            _tokens[response.Token] = new Tuple<string, TokenType>(response.TokenSecret, TokenType.RequestToken);
        }

        #endregion
    }
}
