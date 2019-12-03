using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace ReactInWebForms.App_Code
{
    public abstract class TranspilerFile : HttpTaskAsyncHandler, IReadOnlySessionState
    {
        protected abstract string getContentType();
        protected abstract bool getExportCss();

        private void processRequest(HttpContext context)
        {
            try
            {
                context.Response.ContentType = getContentType();

                var urlPath = context.Request.Url.LocalPath;
                var file = context.Server.MapPath(urlPath);
                if (!File.Exists(file))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                context.Response.BinaryWrite(getCompiledFile(file, Path.GetDirectoryName(urlPath).Replace("\\", "/") + "/"));
            }
            catch (Exception ex)
            {
                context.Response.Write("erro: " + ex.Message);
            }
        }

        private byte[] getCompiledFile(string file, string urlPath)
        {
            var transParams = new Transpiler.TranspilerParams { ExportType = getExportCss() ? Transpiler.ExportType.OnlyScss : Transpiler.ExportType.AllButScss, UrlRelativePath = urlPath };
            return Transpiler.CompileFile(file, transParams, files => putFilesOnCache(files, urlPath));
        }
        private static byte[] putFilesOnCache(Transpiler.Files files, string urlPath)
        {
            for (int i = 1, iMax = files.Count; i < iMax; i++)
                TransitoryRepo.Add(urlPath, files[i]);
            return files.First().Value;
        }
        public override async Task ProcessRequestAsync(HttpContext context) => await Task.Run(() => processRequest(context));
    }
    public class ScssFile : TranspilerFile
    {
        protected override string getContentType() => "text/css";
        protected override bool getExportCss() => true;
    }
    public class ES6JSXFile : TranspilerFile
    {
        protected override string getContentType() => "text/javascript";
        protected override bool getExportCss() => false;
    }
}
