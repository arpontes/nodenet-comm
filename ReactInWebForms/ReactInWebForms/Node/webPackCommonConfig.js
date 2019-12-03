require('@babel/core');
require('babel-loader');
require('@babel/preset-env');
require('@babel/preset-react');

require("@babel/plugin-proposal-decorators");
require("@babel/plugin-proposal-function-sent");
require("@babel/plugin-proposal-export-namespace-from");
require("@babel/plugin-proposal-numeric-separator");
require("@babel/plugin-proposal-throw-expressions");

require("@babel/plugin-syntax-dynamic-import");
require("@babel/plugin-syntax-import-meta");
require("@babel/plugin-proposal-class-properties");
require("@babel/plugin-proposal-json-strings");


require('node-sass');
require('style-loader');
require('css-loader');
require('postcss-loader');
require('sass-loader');
require('url-loader');
require('file-loader');

const miniCssExtractPlugin = require("mini-css-extract-plugin");
const cssnano = require('cssnano');
const autoPrefixer = require('autoprefixer');

module.exports = {
    setConfig: function (cfg, modulesPath, extractCss, includeReactHotLoader) {
        if (cfg.module == null) cfg.module = {};
        if (cfg.plugins == null) cfg.plugins = [];
        if (cfg.module.rules == null) cfg.module.rules = [];

        if (extractCss) {
            cfg.plugins.push(new miniCssExtractPlugin({ filename: "[name].[hash].css" }));
            cfg.optimization = { splitChunks: { cacheGroups: { styles: { name: 'styles', test: /\.s?css$/, chunks: 'all', enforce: true } } } };
        }

        cfg.module.rules = [];
        var r = buildRules(modulesPath, extractCss, includeReactHotLoader);
        for (var i = 0; i < r.length; i++)
            cfg.module.rules.push(r[i]);
    }
}
function buildRules(modulesPath, extractCss, includeReactHotLoader) {
    var babelModulesPath = modulesPath + '\\@babel\\preset-';
    var babelModulesPathPlugin = modulesPath + '\\@babel\\plugin-';
    var jsCfg = {
        test: /\.jsx?$/,
        exclude: /node_modules/,
        use: {
            loader: 'babel-loader',
            options: {
                babelrc: false,
                cacheDirectory: true,
                presets: [
                    [babelModulesPath + "env", { "targets": { "browsers": ["last 2 versions", "not ie <= 11"] } }],
                    babelModulesPath + "react"
                ],
                plugins: [
                    [babelModulesPathPlugin + "proposal-decorators", { legacy: true }],
                    babelModulesPathPlugin + "proposal-function-sent",
                    babelModulesPathPlugin + "proposal-export-namespace-from",
                    babelModulesPathPlugin + "proposal-numeric-separator",
                    babelModulesPathPlugin + "proposal-throw-expressions",
                    babelModulesPathPlugin + "syntax-dynamic-import",
                    babelModulesPathPlugin + "syntax-import-meta",
                    [babelModulesPathPlugin + "proposal-class-properties", { loose: false }],
                    babelModulesPathPlugin + "proposal-json-strings"
                ]
            }
        }
    };
    if (includeReactHotLoader)
        jsCfg.use.options.plugins.push(modulesPath + "\\react-hot-loader\\babel");

    var buildScssCfg = (cssModules) => {
        return [
            extractCss ? miniCssExtractPlugin.loader : { loader: 'style-loader' },
            { loader: 'css-loader', options: { modules: cssModules, importLoaders: 2 } },
            { loader: 'postcss-loader', options: { plugins: () => [autoPrefixer(), cssnano()] } },
            { loader: 'sass-loader' }
        ];
    };
    var cssLoaders = { oneOf: [{ test: /\.module\.s?css$/, use: buildScssCfg(true) }, { test: /\.s?css$/, use: buildScssCfg(false) }] };

    var urlCfg = { test: /\.(mp4|webm|wav|mp3|m4a|aac|oga)(\?.*)?$/, loader: 'url-loader', query: { limit: 10000, name: '[name].[hash:8].[ext]' } };
    var fileCfg = { test: /\.(ico|jpg|jpeg|png|gif|eot|otf|webp|svg|ttf|woff|woff2)(\?.*)?$/, loader: 'file-loader', query: { name: '[name].[hash:8].[ext]' } };

    return [jsCfg, cssLoaders, urlCfg, fileCfg];
}