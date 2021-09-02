using System;
using System.Data.Common;
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

  public sealed class BitBucketConnection : IEquatable<BitBucketConnection> {
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
      Server = server?.Trim()?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(server));

      Auth = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Login}:{Password}"))}";
    }

    // Data Source=http address;User ID=myUsername;password=myPassword;
    /// <summary>
    /// Conenction with Connection String 
    /// </summary>
    public BitBucketConnection(string connectionString) {
      if (connectionString is null)
        throw new ArgumentNullException(nameof(connectionString));

      DbConnectionStringBuilder builder = new() {
        ConnectionString = connectionString
      };

      if (builder.TryGetValue("User ID", out var login) &&
          builder.TryGetValue("password", out var password) &&
          builder.TryGetValue("Data Source", out var server)) {
        Login = login?.ToString() ?? throw new ArgumentException("Login not found", nameof(connectionString));
        Password = password?.ToString() ?? throw new ArgumentException("Password not found", nameof(connectionString));
        Server = server?.ToString()?.Trim()?.TrimEnd('/') ?? throw new ArgumentException("Server not found", nameof(connectionString));

        Auth = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Login}:{Password}"))}";
      }
      else
        throw new ArgumentException("Invalid connection string", nameof(connectionString));
    }

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

    #region IEquatable<BitBucketConnection>

    /// <summary>
    /// Equals 
    /// </summary>
    public bool Equals(BitBucketConnection other) {
      if (ReferenceEquals(this, other))
        return true;
      if (other is null)
        return false;

      return string.Equals(Login, other.Login) &&
             string.Equals(Password, other.Password) &&
             string.Equals(Server, other.Server, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Equals
    /// </summary>
    public override bool Equals(object obj) => obj is BitBucketConnection other && Equals(other);

    /// <summary>
    /// Get Hash Code
    /// </summary>
    public override int GetHashCode() =>
      Login.GetHashCode() ^
      Password.GetHashCode() ^
      Server.GetHashCode(StringComparison.OrdinalIgnoreCase);

    #endregion IEquatable<BitBucketConnection>
  }

}
