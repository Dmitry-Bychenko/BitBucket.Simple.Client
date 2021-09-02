# BitBucket.Simple.Client
BitBucket Simple Client

Demo:

```c#

using BitBucket.Simple.Client;

...

private static BitBucketConnection s_BitBucket = new BitBucketConnection(
  "MyLogin",
  "MyPassword",
  "https://stash-my_server.com");

...

// All projects 
private static async Task<List<string>> Projects() {
  var q = s_BitBucket.CreateQuery();
  
  List<string> list = new List<string>();
  
  await foreach(var doc in q.QueryPagedAsync("projects")) {
    try {
      var array = doc.RootElement.GetProperty("values");
        
      using var en = array.EnumerateArray();
        
      while (en.MoveNext()) 
        list.Add(en.Current.GetProperty("key").GetString()); 
    }
    finally {
      doc.Dispose();
    }
  }
  
  return list;
}

```
