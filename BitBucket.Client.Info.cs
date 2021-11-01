using System;
using System.Text.Json;

namespace BitBucket.Simple.Client {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// BitBucket Server Info
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class BitBucketServerInfo {
    #region Create

    internal BitBucketServerInfo(JsonDocument document) {
      if (document is null)
        throw new ArgumentNullException(nameof(document));

      var root = document.RootElement;

      Version = Version.Parse(root.GetProperty("version").GetString());
      BuildNumber = int.TryParse(root.GetProperty("buildNumber").GetString(), out var v) ? v : 0;
      BuildDate = DateTime.UnixEpoch.AddMilliseconds(
        long.TryParse(root.GetProperty("buildDate").GetString(), out var vv) ? vv : 0);

      Title = root.GetProperty("displayName").GetString();
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Version
    /// </summary>
    public Version Version { get; }

    /// <summary>
    /// Build Date
    /// </summary>
    public DateTime BuildDate { get; }

    /// <summary>
    /// Build Number: Major + Minor + Revision
    /// </summary>
    public int BuildNumber { get; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// To String
    /// </summary>
    public override string ToString() => Title;

    #endregion Public
  }

}
