require('@babel/core');
require('react-hot-loader/babel');
var wpHMRCli = require.resolve('webpack-hot-middleware/client');
var webPackCommonCfg = require('./webPackCommonConfig');
var path = require('path');
const webpack = require('webpack');
var MemoryFS = require("memory-fs");

function toBase64(str) {
    return "FILE:" + Buffer.from(str).toString('base64');
}
function prepareError(ex) {
    return toBase64("ERRO\n" + ex.message);
}
function handleError(ex) {
    console.log(prepareError(ex));
}

var fs = new MemoryFS();
function buildCompiler(withHMR) {
    var modulesPath = path.resolve(__dirname, 'node_modules');
    var cfg = {
        entry: withHMR ? [wpHMRCli + "?path=" + params.UrlRelativePath + pathHMR + "&timeout=1000&reload=true", params.File] : params.File,
        output: {
            path: "/",
            publicPath: params.UrlRelativePath,
            filename: params.Key + '.js',
            hotUpdateChunkFilename: '[id].[hash].hot-update.js',
            hotUpdateMainFilename: '[hash].hot-update.json'
        },
        mode: "development",
        devtool: "inline-source-map",
        plugins: withHMR ? [new webpack.HotModuleReplacementPlugin()] : [],
        resolve: {
            extensions: ['.js', '.jsx', '.module.scss', '.scss']
        }
    };
    webPackCommonCfg.setConfig(cfg, modulesPath, params.ExtractCss, true);

    var compiler = webpack(cfg);
    compiler.outputFileSystem = fs;
    return compiler;
}
function executeTranspiling() {
    try {
        var compiler = buildCompiler(false);
        compiler.run((err, stats) => {
            console.log(getFiles(err, stats, false));
        });
    } catch (ex) {
        handleError(ex);
    }
}
function executeWatching() {
    try {
        var compiler = buildCompiler(true);
        compiler.watch({}, (err, stats) => {
            const newlyCreatedAssets = stats.compilation.assets;
            for (var i = 0, m = fs.readdirSync("/"), imax = m.length; i < imax; i++)
                if (fs.statSync("/" + m[i]).isFile() && !newlyCreatedAssets[m[i]])
                    fs.unlinkSync("/" + m[i]);

            console.log(getFiles(err, stats, true));
        });
    } catch (ex) {
        handleError(ex);
    }
}
function getFiles(err, stats, isWatching) {
    try {
        var content = "";

        if ((onlyCss || !isWatching) && (err || stats.hasErrors()))
            content += "ERRO\n" + (err ? err.toString() : stats.toString("errors-only"));
        else {
            for (var i = 0, m = fs.readdirSync("/"), imax = m.length; i < imax; i++)
                if (fs.statSync("/" + m[i]).isFile())
                    content += m[i] + "\n" + fs.readFileSync("/" + m[i]).toString('base64') + "\n";
            if (isWatching)
                content += pathHMR + "\n" + generateHMRFile(stats);
            else if (content.length > 0)
                content = content.substring(0, content.length - 1);
        }
        return toBase64(content);
    } catch (ex) {
        return prepareError(ex);
    }
}

//Os três métodos abaixo são do webpack-hot-middleware.
//No original, eles geram a informação para ser gerar os dados que vão por websocket
//para o arquivo que o client do HMR consulta. Neste caso, do conteúdo será gerado
//um arquivo que ficará no cache.
function generateHMRFile(statsResult) {
    var stats = statsResult.toJson({
        all: false,
        children: true,
        modules: true,
        timings: true,
        hash: true
    });
    // For multi-compiler, stats will be an object with a 'children' array of stats
    var payloadContent = "";
    var bundles = extractBundles(stats);
    bundles.forEach(function (stats) {
        var name = stats.name || "";

        // Fallback to compilation name in case of 1 bundle (if it exists)
        if (bundles.length === 1 && !name && statsResult.compilation) {
            name = statsResult.compilation.name || "";
        }

        var payload = {
            name: name,
            action: "sync",
            time: stats.time,
            hash: stats.hash,
            warnings: stats.warnings || [],
            errors: stats.errors || [],
            modules: buildModuleMap(stats.modules)
        };
        payloadContent += "data: " + JSON.stringify(payload) + "\n\n";
    });
    return Buffer.from(payloadContent).toString('base64');
}
function extractBundles(stats) {
    // Stats has modules, single bundle
    if (stats.modules) return [stats];

    // Stats has children, multiple bundles
    if (stats.children && stats.children.length) return stats.children;

    // Not sure, assume single
    return [stats];
}
function buildModuleMap(modules) {
    var map = {};
    modules.forEach(function (module) {
        map[module.id] = module.name;
    });
    return map;
}


var params;
var pathHMR;
var onlyCss = false;
try {
    var command = process.argv[2];
    params = JSON.parse(process.argv[3]);
    pathHMR = params.Key + ".HMR";
    onlyCss = params.File.endsWith(".scss");

    if (command === "compile")
        executeTranspiling();
    else if (command === "watch")
        executeWatching();
} catch (ex) {
    handleError(ex);
}