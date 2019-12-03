using ReactInWebForms.App_Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace ReactInWebForms
{
    public class Global : System.Web.HttpApplication
    {
        public override void Init()
        {
            base.Init();

            //new RemapHandlerByFileExtensionModule<ScssFile>(".scss").Init(this);
            //new RemapHandlerByFileExtensionModule<ES6JSXFile>(".jsx").Init(this);

            //new RemapHandlerByUrlStartModule<TransitoryRepo>(TransitoryRepo.TRANSITORYSTARTURL + "/").Init(this);
        }
    }

    public abstract class RemapHandlerByUrlPatternModuleBase<T> : IHttpModule where T : IHttpHandler, new()
    {
        protected readonly string search;
        public RemapHandlerByUrlPatternModuleBase(string search) => this.search = search;

        protected abstract bool shouldExecuteHandler(string url);
        public void Init(HttpApplication context)
        {
            context.PostResolveRequestCache += (sender, e) =>
            {
                var app = (HttpApplication)sender;
                if (shouldExecuteHandler(app.Request.Url.LocalPath))
                    app.Context.RemapHandler(new T());
            };
        }
        public void Dispose() { }
    }
    public class RemapHandlerByFileExtensionModule<T> : RemapHandlerByUrlPatternModuleBase<T> where T : IHttpHandler, new()
    {
        public RemapHandlerByFileExtensionModule(string extension) : base(extension) { }
        protected override bool shouldExecuteHandler(string url) => url.EndsWith(base.search, StringComparison.InvariantCultureIgnoreCase);
    }
    public class RemapHandlerByUrlStartModule<T> : RemapHandlerByUrlPatternModuleBase<T> where T : IHttpHandler, new()
    {
        public RemapHandlerByUrlStartModule(string start) : base(start) { }
        protected override bool shouldExecuteHandler(string url) => url.StartsWith(base.search, StringComparison.OrdinalIgnoreCase);
    }
}