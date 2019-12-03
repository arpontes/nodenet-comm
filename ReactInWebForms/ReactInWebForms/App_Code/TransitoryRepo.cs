using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace ReactInWebForms.App_Code
{
    public class TransitoryRepo : HttpTaskAsyncHandler, IReadOnlySessionState
    {
        public static readonly string TRANSITORYSTARTURL = "/transrep";

        private static readonly ConcurrentDictionary<string, byte[]> dictCache = new ConcurrentDictionary<string, byte[]>();

        private static Dictionary<string, string> dictContentTypes = new Dictionary<string, string> {
                { ".js", "text/javascript" }, { ".css", "text/css" }, { ".json", "text/json" }, { ".png", "image/png" },
                { ".HMR", "text/event-stream" }
            };
        private void processRequest(HttpContext context)
        {
            var localPath = context.Request.Url.LocalPath.Substring(TRANSITORYSTARTURL.Length);

            var fileName = Path.GetFileName(localPath);
            localPath = Path.GetDirectoryName(localPath).Replace("\\", "/");
            if (localPath == "/") localPath = "";

            var cacheKey = string.Concat(localPath, "/", fileName);
            var contentType = dictContentTypes.FirstOrDefault(x => fileName.EndsWith(x.Key, StringComparison.Ordinal));
            if (!string.IsNullOrEmpty(contentType.Value))
                context.Response.ContentType = contentType.Value;

            if (!dictCache.TryGetValue(cacheKey, out byte[] fileContent))
                context.Response.StatusCode = 404;
            else
                context.Response.BinaryWrite(fileContent);
        }
        public override async Task ProcessRequestAsync(HttpContext context) => await Task.Run(() => processRequest(context));

        public static void Add(string urlPath, KeyValuePair<string, byte[]> file)
        {
            dictCache.AddOrUpdate(string.Concat(urlPath, file.Key), _ => file.Value, (k, o) => file.Value);
        }
    }
}