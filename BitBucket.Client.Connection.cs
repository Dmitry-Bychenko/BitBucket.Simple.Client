using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace BitBucket.Simple.Client {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// BitBacket Connection
  /// </summary>
  /// <seealso cref="https://developer.atlassian.com/server/bitbucket/reference/rest-api/"/>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class BitBucketConnection {
    #region Private Data

    private static readonly CookieContainer s_CookieContainer;

    private static readonly HttpClient s_HttpClient;

    #endregion Private Data

    #region Create

    static BitBucketConnection() {
      try {
        ServicePointManager.SecurityProtocol =
          SecurityProtocolType.Tls |
          SecurityProtocolType.Tls11 |
          SecurityProtocolType.Tls12;
      }
      catch (NotSupportedException) {
        ;
      }

      s_CookieContainer = new CookieContainer();

      var handler = new HttpClientHandler() {
        CookieContainer = s_CookieContainer,
        Credentials = CredentialCache.DefaultCredentials,
      };

      s_HttpClient = new HttpClient(handler) {
        Timeout = Timeout.InfiniteTimeSpan,
      };
    }

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public BitBucketConnection(string login, string password, string server) {
      Login = login ?? throw new ArgumentNullException(nameof(login));
      Password = password ?? throw new ArgumentNullException(nameof(password));
      Server = server?.Trim().TrimEnd('/') ?? throw new ArgumentNullException(nameof(server));

      Auth = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Login}:{Password}"))}";
    }

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public BitBucketConnection(string login, string password) : this(login, password, null) { }

    #endregion Create

    #region Public

    /// <summary>
    /// Http Client
    /// </summary>
    public static HttpClient Client => s_HttpClient;

    /// <summary>
    /// Create Query
    /// </summary>
    public BitBucketQuery CreateQuery() => new(this);

    /// <summary>
    /// Login
    /// </summary>
    public string Login { get; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Authentification
    /// </summary>
    public string Auth { get; }

    /// <summary>
    /// Server
    /// </summary>
    public string Server { get; }

    /// <summary>
    /// To String
    /// </summary>
    public override string ToString() => $"{Login}@{Server}";

    #endregion Public
  }

}
