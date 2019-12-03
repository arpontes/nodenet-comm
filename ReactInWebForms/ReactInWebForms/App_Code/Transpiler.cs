using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ReactInWebForms.App_Code
{
    public class Transpiler
    {
        private static readonly string NodePath = string.Concat(new DirectoryInfo(HttpRuntime.BinDirectory).Parent.FullName, "\\Node");
        static Transpiler()
        {
            AppDomain.CurrentDomain.DomainUnload += (s, e) => unloadWatchers();
            AppDomain.CurrentDomain.ProcessExit += (s, e) => unloadWatchers();
        }

        public class Files : List<KeyValuePair<string, byte[]>> { }
        public delegate T ProcessFiles<T>(Files files);
        public enum ExportType { OnlyScss, AllButScss, All }
        public class TranspilerParams
        {
            public string UrlRelativePath { get; set; }
            public ExportType ExportType { get; set; }
        }

        public static T CompileFile<T>(string file, TranspilerParams tparams, ProcessFiles<T> fnProcessFiles)
            => watchFile(file, tparams, fnProcessFiles);

        #region Debug Mode
        private class Watcher
        {
            public string File { get; set; }
            public ProcessContainerForWatching Process { get; set; }
        }
        private static readonly ConcurrentDictionary<string, Watcher> dictWatch = new ConcurrentDictionary<string, Watcher>();
        private static T watchFile<T>(string file, TranspilerParams tparams, ProcessFiles<T> fnProcessFiles)
        {
            var fileKey = getHashedString(file);
            var watcher = dictWatch.GetOrAdd(fileKey, key => new Watcher
            {
                File = file,
                Process = new ProcessContainerForWatching(generateInputParams(fileKey, file, tparams), files => fnProcessFiles(separateFiles(fileKey, files, tparams.ExportType)))
            });
            watcher.Process.RenewIfNecessary();
            var ret = watcher.Process.GetLastOutput();
            return fnProcessFiles(separateFiles(fileKey, ret, tparams.ExportType));
        }
        private static void unloadWatchers()
        {
            foreach (var item in dictWatch)
                try
                {
                    item.Value.Process.EndProcess();
                }
                catch { }
        }
        #endregion

        private static string getHashedString(string originalData)
            => string.Join(string.Empty, MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(originalData)).Select(x => x.ToString("x2")));

        private static string generateInputParams(string fileKey, string filePath, TranspilerParams tparams)
        {
            //Explicação dos paths:
            //[path relativo ao arquivo que está sendo processado, incluindo o caminho de arquivos transitórios. Exemplo: se a URL está em: http://localhost/Web/Teste.jsx, este path deverá ser: /[transitorypath]/Web/]
            return JsonConvert.SerializeObject(new
            {
                Key = fileKey,
                File = filePath,
                UrlRelativePath = string.Concat(TransitoryRepo.TRANSITORYSTARTURL, tparams.UrlRelativePath),
                ExtractCss = tparams.ExportType != ExportType.AllButScss
            });
        }
        private static Files separateFiles(string fileKey, List<string> stout, ExportType exportType)
        {
            if (stout.Count == 0)
                throw new Exception("Transpiler's empty response!");
            if (stout[0].Equals("ERRO", StringComparison.OrdinalIgnoreCase))
                throw new Exception(string.Join(Environment.NewLine, stout));

            var ret = new Files();
            for (var i = 0; i < stout.Count; i += 2)
                if (exportType != ExportType.OnlyScss || stout[i].EndsWith(".css", StringComparison.Ordinal))
                    ret.Add(new KeyValuePair<string, byte[]>(stout[i], Convert.FromBase64String(stout[i + 1])));

            //O arquivo principal ([fileKey] + .js) deve ser o primeiro da lista exportada.
            var mainJsFileIdx = ret.FindIndex(x => x.Key.Equals(fileKey + ".js", StringComparison.Ordinal));
            if (mainJsFileIdx > -1)
            {
                var mainFile = ret[mainJsFileIdx];
                ret.RemoveAt(mainJsFileIdx);
                ret.Insert(0, mainFile);
            }

            return ret;
        }


        private class ProcessContainerForWatching : ProcessContainerKeepRunning
        {
            public ProcessContainerForWatching(string parameters, Action<List<string>> fnProcessFiles) : base("node", NodePath, $"transpiler.js watch \"{parameters.Replace("\"", "\\\"")}\"", fnProcessFiles) { }
        }
    }
}