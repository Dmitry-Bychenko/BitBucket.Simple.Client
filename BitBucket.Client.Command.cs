﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BitBucket.Simple.Client {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// BitBucket Query
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class BitBucketQuery {
    #region Constants

    /// <summary>
    /// Default Page Size for QueryPagedAsync
    /// </summary>
    public const int DEFAULT_PAGE_SIZE = 500;

    #endregion Constants

    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public BitBucketQuery(BitBucketConnection connection) {
      Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Connection
    /// </summary>
    public BitBucketConnection Connection { get; }

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address"></param>
    /// <param name="query"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public async Task<JsonDocument> QueryAsync(string address, string query, HttpMethod method, CancellationToken token) {
      if (string.IsNullOrEmpty(address))
        throw new ArgumentNullException(nameof(address));

      address = string.Join("/", Connection.Server, "rest", address.TrimStart('/'));

      query ??= "{}";

      using var req = new HttpRequestMessage {
        Method = method,
        RequestUri = new Uri(address),
        Headers = {
          { HttpRequestHeader.Accept.ToString(), "application/json" },
          { HttpRequestHeader.Authorization.ToString(), Connection.Auth},
        },
        Content = new StringContent(query, Encoding.UTF8, "application/json")
      };

      var response = await BitBucketConnection.Client.SendAsync(req, token).ConfigureAwait(false);

      if (!response.IsSuccessStatusCode)
        throw new DataException(response.ReasonPhrase);

      using Stream stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);

      return await JsonDocument.ParseAsync(stream, default, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address"></param>
    /// <param name="query"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public async Task<JsonDocument> QueryAsync(string address, string query, HttpMethod method) =>
      await QueryAsync(address, query, method, CancellationToken.None).ConfigureAwait(false);

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<JsonDocument> QueryAsync(string address, string query, CancellationToken token) =>
      await QueryAsync(address, query, HttpMethod.Post, token).ConfigureAwait(false);

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<JsonDocument> QueryAsync(string address, string query) =>
      await QueryAsync(address, query, HttpMethod.Post, CancellationToken.None).ConfigureAwait(false);

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<JsonDocument> QueryAsync(string address, CancellationToken token) =>
      await QueryAsync(address, "", HttpMethod.Get, token).ConfigureAwait(false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<JsonDocument> QueryAsync(string address) =>
      await QueryAsync(address, "", HttpMethod.Get, CancellationToken.None).ConfigureAwait(false);

    /// <summary>
    /// Query
    /// </summary>
    /// <param name="address"></param>
    /// <param name="query"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                                string query,
                                                                HttpMethod method,
                                                                int pageSize,
                                                                [EnumeratorCancellation]
                                                                CancellationToken token) {
      if (string.IsNullOrEmpty(address))
        throw new ArgumentNullException(nameof(address));

      if (pageSize <= 0)
        pageSize = DEFAULT_PAGE_SIZE;

      address = string.Join("/", Connection.Server, "rest", address.TrimStart('/'));

      if (address.Contains('?'))
        address += $"&limit={pageSize}";
      else
        address += $"?limit={pageSize}";

      query ??= "{}";

      int start = 0;

      while (start >= 0) {
        using var req = new HttpRequestMessage {
          Method = method,
          RequestUri = new Uri(address + (start == 0 ? "" : $"&start={start}")),
          Headers = {
          { HttpRequestHeader.Accept.ToString(), "application/json" },
          { HttpRequestHeader.Authorization.ToString(), Connection.Auth},
        },
          Content = new StringContent(query, Encoding.UTF8, "application/json")
        };

        var response = await BitBucketConnection.Client.SendAsync(req, token).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
          throw new DataException(string.IsNullOrEmpty(response.ReasonPhrase)
            ? $"Query failed with {response.StatusCode} ({(int)response.StatusCode}) code"
            : response.ReasonPhrase);

        using Stream stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);

        var jsonDocument = await JsonDocument.ParseAsync(stream, default, token).ConfigureAwait(false);

        if (jsonDocument.RootElement.TryGetProperty("nextPageStart", out var prop))
          start = prop.GetInt32();
        else
          start = -1;

        yield return jsonDocument;
      }
    }

    /// <summary>
    /// Paged Query 
    /// </summary>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                                string query,
                                                                HttpMethod method,
                                                                int pageSize) {
      await foreach (var item in QueryPagedAsync(address, query, method, pageSize, CancellationToken.None))
        yield return item;
    }

    /// <summary>
    /// Paged Query 
    /// </summary>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                                string query,
                                                                int pageSize,
                                                                [EnumeratorCancellation]
                                                                CancellationToken token) {
      await foreach (var item in QueryPagedAsync(address, query, HttpMethod.Post, pageSize, token))
        yield return item;
    }

    /// <summary>
    /// Paged Query 
    /// </summary>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                                string query,
                                                                int pageSize) {
      await foreach (var item in QueryPagedAsync(address, query, HttpMethod.Post, pageSize, CancellationToken.None))
        yield return item;
    }

    /// <summary>
    /// Paged Query 
    /// </summary>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                                int pageSize,
                                                                [EnumeratorCancellation]
                                                                CancellationToken token) {
      await foreach (var item in QueryPagedAsync(address, "", HttpMethod.Get, pageSize, token))
        yield return item;
    }

    /// <summary>
    /// Paged Query 
    /// </summary>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                                int pageSize) {
      await foreach (var item in QueryPagedAsync(address, "", HttpMethod.Get, pageSize, CancellationToken.None))
        yield return item;
    }

    /// <summary>
    /// Paged Query
    /// </summary>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                                string query,
                                                                HttpMethod method,
                                                               [EnumeratorCancellation]
                                                                CancellationToken token) {
      await foreach (var item in QueryPagedAsync(address, query, method, DEFAULT_PAGE_SIZE, token))
        yield return item;
    }

    /// <summary>
    /// Paged Query 
    /// </summary>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                                string query,
                                                                HttpMethod method) {
      await foreach (var item in QueryPagedAsync(address, query, method, DEFAULT_PAGE_SIZE, CancellationToken.None))
        yield return item;
    }

    /// <summary>
    /// Paged Query 
    /// </summary>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                                string query,
                                                               [EnumeratorCancellation]
                                                                CancellationToken token) {
      await foreach (var item in QueryPagedAsync(address, query, HttpMethod.Post, DEFAULT_PAGE_SIZE, token))
        yield return item;
    }

    /// <summary>
    /// Paged Query 
    /// </summary>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                                string query) {
      await foreach (var item in QueryPagedAsync(address, query, HttpMethod.Post, DEFAULT_PAGE_SIZE, CancellationToken.None))
        yield return item;
    }

    /// <summary>
    /// Paged Query 
    /// </summary>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address,
                                                               [EnumeratorCancellation]
                                                                CancellationToken token) {
      await foreach (var item in QueryPagedAsync(address, "", HttpMethod.Get, DEFAULT_PAGE_SIZE, token))
        yield return item;
    }

    /// <summary>
    /// Paged Query 
    /// </summary>
    public async IAsyncEnumerable<JsonDocument> QueryPagedAsync(string address) {
      await foreach (var item in QueryPagedAsync(address, "", HttpMethod.Get, DEFAULT_PAGE_SIZE, CancellationToken.None))
        yield return item;
    }

    #endregion Public
  }

}
