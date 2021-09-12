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
    #region Constants 

    private const int PAGE_SIZE = 10000;

    #endregion Constants

    #region Public

    /// <summary>
    /// Projects 
    /// </summary>
    public static async IAsyncEnumerable<JsonElement> ProjectsAsync(this BitBucketConnection connection) {
      if (connection is null)
        throw new ArgumentNullException(nameof(connection));

      var query = connection.CreateQuery();

      string jql = string.Join("/", "projects");

      jql += $"?limit={PAGE_SIZE}";
      int startAt = 0;

      for (bool loop = true; loop;) {
        loop = false;

        await foreach (var page in query.QueryPagedAsync(jql + $"&start={startAt}")) {
          using (page) {
            foreach (var item in page.RootElement.GetProperty("values").EnumerateArray())
              yield return item;

            if (loop = !page.RootElement.GetProperty("isLastPage").GetBoolean())
              startAt = page.RootElement.GetProperty("nextPageStart").GetInt32();
          }
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

      jql += $"?limit={PAGE_SIZE}";
      int startAt = 0;

      for (bool loop = true; loop;) {
        loop = false;

        await foreach (var page in query.QueryPagedAsync(jql + $"&start={startAt}")) {
          using (page) {
            foreach (var item in page.RootElement.GetProperty("values").EnumerateArray())
              yield return item;

            if (loop = !page.RootElement.GetProperty("isLastPage").GetBoolean())
              startAt = page.RootElement.GetProperty("nextPageStart").GetInt32();
          }
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

        jql += $"?limit={PAGE_SIZE}";
        int startAt = 0;

        for (bool loop = true; loop;) {
          loop = false;

          await foreach (var page in query.QueryPagedAsync(jql + $"&start={startAt}")) {
            using (page) {
              foreach (var repo in page.RootElement.GetProperty("values").EnumerateArray())
                yield return (project, repo);

              if (loop = !page.RootElement.GetProperty("isLastPage").GetBoolean())
                startAt = page.RootElement.GetProperty("nextPageStart").GetInt32();
            }
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

      jql += $"?limit={PAGE_SIZE}";
      int startAt = 0;

      for (bool loop = true; loop;) {
        loop = false;

        var (document, _) = await query.TryQueryAsync(jql + $"&start={startAt}", "", HttpMethod.Get, CancellationToken.None);

        if (document is not null)
          using (document) {
            foreach (var item in document.RootElement.GetProperty("values").EnumerateArray())
              yield return item;

            if (loop = !document.RootElement.GetProperty("isLastPage").GetBoolean())
              startAt = document.RootElement.GetProperty("nextPageStart").GetInt32();
          }
      }
    }

    /// <summary>
    /// Commits 
    /// </summary>
    public static async IAsyncEnumerable<(JsonElement project, JsonElement repo, JsonElement commit)> AllCommitsAsync(this BitBucketConnection connection) {
      if (connection is null)
        throw new ArgumentNullException(nameof(connection));

      var query = connection.CreateQuery();

      await foreach (var (project, repo) in AllReposAsync(connection)) {
        string jql = string.Join("/",
          "projects",
           project.GetProperty("key").GetString(),
          "repos",
           repo.GetProperty("slug").GetString(),
          "commits");

        jql += $"?limit={PAGE_SIZE}";
        int startAt = 0;

        for (bool loop = true; loop;) {
          loop = false;

          var (document, _) = await query.TryQueryAsync(jql + $"&start={startAt}", "", HttpMethod.Get, CancellationToken.None);

          if (document is not null)
            using (document) {
              foreach (var item in document.RootElement.GetProperty("values").EnumerateArray())
                yield return (project, repo, item);

              if (loop = !document.RootElement.GetProperty("isLastPage").GetBoolean())
                startAt = document.RootElement.GetProperty("nextPageStart").GetInt32();
            }
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

      jql += $"?limit={PAGE_SIZE}";
      int startAt = 0;

      for (bool loop = true; loop;) {
        loop = false;

        var (document, _) = await query.TryQueryAsync(jql + $"&start={startAt}", "", HttpMethod.Get, CancellationToken.None);

        if (document is not null)
          using (document) {
            foreach (var item in document.RootElement.GetProperty("values").EnumerateArray())
              yield return item;

            if (loop = document.RootElement.GetProperty("isLastPage").GetBoolean())
              startAt = document.RootElement.GetProperty("nextPageStart").GetInt32();
          }
      }
    }

    /// <summary>
    /// Branches
    /// </summary>
    public static async IAsyncEnumerable<(JsonElement project, JsonElement repo, JsonElement branch)> AllBranchesAsync(this BitBucketConnection connection) {
      if (connection is null)
        throw new ArgumentNullException(nameof(connection));

      var query = connection.CreateQuery();

      await foreach (var (project, repo) in AllReposAsync(connection)) {
        string jql = string.Join("/",
          "projects",
           project.GetProperty("key").GetString(),
          "repos",
           repo.GetProperty("slug").GetString(),
          "branches");

        jql += $"?limit={PAGE_SIZE}";
        int startAt = 0;

        for (bool loop = true; loop;) {
          loop = false;

          var (document, _) = await query.TryQueryAsync(jql + $"&start={startAt}", "", HttpMethod.Get, CancellationToken.None);

          if (document is not null)
            using (document) {
              foreach (var item in document.RootElement.GetProperty("values").EnumerateArray())
                yield return (project, repo, item);

              if (loop = !document.RootElement.GetProperty("isLastPage").GetBoolean())
                startAt = document.RootElement.GetProperty("nextPageStart").GetInt32();
            }
        }
      }
    }

    #endregion Public
  }

}
