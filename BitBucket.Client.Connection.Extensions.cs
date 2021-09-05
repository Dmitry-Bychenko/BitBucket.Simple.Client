using BitBucket.Simple.Client.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace BitBucket.Simple.Client {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// BitBucketConnection Extensions
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class BitBucketConnectionExtensions {
    #region Public

    /// <summary>
    /// Projects 
    /// </summary>
    public static async IAsyncEnumerable<JsonElement> ProjectsAsync(this BitBucketConnection connection) {
      if (connection is null)
        throw new ArgumentNullException(nameof(connection));

      var query = connection.CreateQuery();

      await foreach (var page in query.QueryPagedAsync("projects")) {
        using (page) {
          foreach (var item in page.RootElement.GetProperty("values").EnumerateArray())
            yield return item;
        }
      }
    }

    /// <summary>
    /// Repositories 
    /// </summary>
    public static async IAsyncEnumerable<JsonElement> ReposAsync(this BitBucketConnection connection,
                                                                      string projectKey) {
      if (connection is null)
        throw new ArgumentNullException(nameof(connection));

      if (projectKey is null)
        throw new ArgumentNullException(nameof(projectKey));

      var query = connection.CreateQuery();

      string jql = string.Join("/", "projects", projectKey, "repos");

      await foreach (var page in query.QueryPagedAsync(jql)) {
        using (page) {
          foreach (var item in page.RootElement.GetProperty("values").EnumerateArray())
            yield return item;
        }
      }
    }

    /// <summary>
    /// All Repositories 
    /// </summary>
    public static async IAsyncEnumerable<(JsonElement project, JsonElement repo)> AllReposAsync(this BitBucketConnection connection) {
      if (connection is null)
        throw new ArgumentNullException(nameof(connection));

      var query = connection.CreateQuery();

      await foreach (JsonElement project in ProjectsAsync(connection)) {
        string jql = string.Join("/", "projects", project.Read("key").GetString(), "repos");

        await foreach (var page in query.QueryPagedAsync(jql)) {
          using (page) {
            foreach (var repo in page.RootElement.GetProperty("values").EnumerateArray())
              yield return (project, repo);
          }
        }
      }
    }

    /// <summary>
    /// Commits 
    /// </summary>
    public static async IAsyncEnumerable<JsonElement> CommitsAsync(this BitBucketConnection connection,
                                                                        string projectKey,
                                                                        string repoSlug) {
      if (connection is null)
        throw new ArgumentNullException(nameof(connection));

      if (projectKey is null)
        throw new ArgumentNullException(nameof(projectKey));

      if (repoSlug is null)
        throw new ArgumentNullException(nameof(repoSlug));

      var query = connection.CreateQuery();

      string jql = string.Join("/", "projects", projectKey, "repos", repoSlug, "commits");

      var response = await query.TryQueryAsync(jql, "", HttpMethod.Get, CancellationToken.None);

      if (response.document is not null)
        using (response.document) {
          foreach (var item in response.document.RootElement.GetProperty("values").EnumerateArray())
            yield return item;
        }
    }

    /// <summary>
    /// Commits 
    /// </summary>
    public static async IAsyncEnumerable<(JsonElement project, JsonElement repo, JsonElement commit)> AllCommitsAsync(this BitBucketConnection connection) {
      if (connection is null)
        throw new ArgumentNullException(nameof(connection));

      var query = connection.CreateQuery();

      await foreach (var pair in AllReposAsync(connection)) {
        string jql = string.Join("/",
          "projects",
           pair.project.GetProperty("key").GetString(),
          "repos",
           pair.repo.GetProperty("slug").GetString(),
          "commits");

        var response = await query.TryQueryAsync(jql, "", HttpMethod.Get, CancellationToken.None);

        if (response.document is not null)
          using (response.document) {
            foreach (var item in response.document.RootElement.GetProperty("values").EnumerateArray())
              yield return (pair.project, pair.repo, item);
          }
      }
    }

    /// <summary>
    /// Branches 
    /// </summary>
    public static async IAsyncEnumerable<JsonElement> BranchesAsync(this BitBucketConnection connection,
                                                                         string projectKey,
                                                                         string repoSlug) {
      if (connection is null)
        throw new ArgumentNullException(nameof(connection));

      if (projectKey is null)
        throw new ArgumentNullException(nameof(projectKey));

      if (repoSlug is null)
        throw new ArgumentNullException(nameof(repoSlug));

      var query = connection.CreateQuery();

      string jql = string.Join("/", "projects", projectKey, "repos", repoSlug, "branches");

      var response = await query.TryQueryAsync(jql, "", HttpMethod.Get, CancellationToken.None);

      if (response.document is not null)
        using (response.document) {
          foreach (var item in response.document.RootElement.GetProperty("values").EnumerateArray())
            yield return item;
        }
    }

    /// <summary>
    /// Branches
    /// </summary>
    public static async IAsyncEnumerable<(JsonElement project, JsonElement repo, JsonElement branch)> AllBranchesAsync(this BitBucketConnection connection) {
      if (connection is null)
        throw new ArgumentNullException(nameof(connection));

      var query = connection.CreateQuery();

      await foreach (var pair in AllReposAsync(connection)) {
        string jql = string.Join("/",
          "projects",
           pair.project.GetProperty("key").GetString(),
          "repos",
           pair.repo.GetProperty("slug").GetString(),
          "branches");

        var response = await query.TryQueryAsync(jql, "", HttpMethod.Get, CancellationToken.None);

        if (response.document is not null)
          using (response.document) {
            foreach (var item in response.document.RootElement.GetProperty("values").EnumerateArray())
              yield return (pair.project, pair.repo, item);
          }
      }
    }

    #endregion Public
  }

}
