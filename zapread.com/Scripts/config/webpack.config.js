const webpack = require('webpack');
const path = require("path");
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

module.exports = {
    entry: {
        account_login:  "./Scripts/src/pages/account/login.js",
        admin_achievements: "./Scripts/src/pages/admin/achievements.js",
        admin_audit:    "./Scripts/src/pages/admin/audit.js",
        group_detail:   "./Scripts/src/group/detail.js",
        home_index:     "./Scripts/src/home/index.js",
        index:          "./Scripts/src/index.js",
        manage_apikeys: "./Scripts/src/manage/apikeys.js",
        manage_default: "./Scripts/src/pages/manage/default.js",
        manage_index:   "./Scripts/src/manage/index.js",
        post_detail:    "./Scripts/src/post/detail.js",
        post_edit:      "./Scripts/src/post/edit.js",
        user_index:     "./Scripts/src/user/index.js"
    },
    output: {
        path: path.resolve(__dirname, "../dist"),
        filename: "[name].js"
    },
    module: {
        rules: [
            {
                test: /\.(sa|sc|c)ss$/,
                use: [
                    {
                        loader: MiniCssExtractPlugin.loader
                    },
                    //{
                    //    loader: 'style-loader',
                    //},
                    {
                        loader: 'css-loader'
                    },
                    //{
                    //    loader: 'postcss-loader',
                    //},
                    {
                        loader: 'sass-loader',
                        options: {
                            implementation: require('sass')
                        }
                    }
                ]
            },
            //{
            //    test: /\.css$/,
            //    use: ['style-loader', 'css-loader']
            //},
            {
                use: {
                    loader: "babel-loader"
                },
                test: /\.js$/,
                exclude: /node_modules/ //excludes node_modules folder from being transpiled by babel. We do this because it's a waste of resources to do so.
            },
            {
                test: /\.(png|jpe?g|gif|svg)$/,
                use: [
                    {
                        loader: 'file-loader',
                        options: {
                            name: '[name].[ext]',
                            outputPath: '../../Content/images',
                            publicPath: '/Content/images'
                        }
                    }
                ]
            },
            {
                test: /\.(woff|woff2|ttf|otf|eot)$/,
                use: [
                    {
                        loader: 'file-loader',
                        options: {
                            name: '[name].[ext]',
                            outputPath: '../../Content/fonts',
                            publicPath: '/Content/fonts'
                        }
                    }
                ]
            },
            {
                test: require.resolve('jquery'),
                use: [{
                    loader: 'expose-loader',
                    options: 'jQuery'
                }, {
                    loader: 'expose-loader',
                    options: '$'
                }]
            }
        ]
    },
    plugins: [
        new webpack.ProvidePlugin({
            $: 'jquery',
            jQuery: 'jquery',
            CodeMirror: 'codemirror'
        }),
        new MiniCssExtractPlugin({
            filename: '[name].css',
            chunkFilename: '[id].css'
        })
    ]
};