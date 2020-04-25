const webpack = require('webpack');
const path = require("path");
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

module.exports = {
    entry: {
        account_login:  "./Scripts/src/pages/account/login.js",
        admin_achievements: "./Scripts/src/pages/admin/achievements.js",
        admin_audit:    "./Scripts/src/pages/admin/audit.js",
        admin_icons:    "./Scripts/src/pages/admin/icons.js",
        admin_index:    "./Scripts/src/pages/admin/index.js",
        admin_jobs:     "./Scripts/src/pages/admin/jobs.js",
        admin_lightning:"./Scripts/src/pages/admin/lightning.js",
        admin_users:    "./Scripts/src/pages/admin/users.js",
        group_detail:   "./Scripts/src/pages/group/detail.js",
        group_index:    "./Scripts/src/pages/group/index.js",
        group_members:  "./Scripts/src/pages/group/members.js",
        group_new:      "./Scripts/src/pages/group/new.js",
        home_about:     "./Scripts/src/pages/home/about.js",
        home_faq:       "./Scripts/src/pages/home/faq.js",
        home_index:     "./Scripts/src/pages/home/index.js",
        home_install:   "./Scripts/src/pages/home/install.js",
        mailer_default: "./Scripts/src/pages/mailer/default.js",
        manage_apikeys: "./Scripts/src/pages/manage/apikeys.js",
        manage_default: "./Scripts/src/pages/manage/default.js",
        manage_financial: "./Scripts/src/pages/manage/financial.js",
        manage_index:   "./Scripts/src/pages/manage/index.js",
        messages_alerts:"./Scripts/src/pages/messages/alerts.js",
        messages_all:   "./Scripts/src/pages/messages/all.js",
        messages_chat:  "./Scripts/src/pages/messages/chat.js",
        messages_chats: "./Scripts/src/pages/messages/chats.js",
        messages_index: "./Scripts/src/pages/messages/index.js",
        post_detail:    "./Scripts/src/pages/post/detail.js",
        post_edit:      "./Scripts/src/pages/post/edit.js",
        post_newpost:   "./Scripts/src/pages/post/newpost.js",
        post_postnotfound: "./Scripts/src/pages/post/postnotfound.js",
        user_achievements: "./Scripts/src/pages/user/achievements.js",
        user_index:     "./Scripts/src/pages/user/index.js"
    },
    output: {
        path: path.resolve(__dirname, "../dist"),
        filename: (c) => {
            var subpath = c.chunk.name.split('_')[0];
            var filename = c.chunk.name.split('_')[1];
            return subpath + "/" + filename + ".js";
        }
    },
    module: {
        rules: [
            {
                test: /\.(sa|sc|c)ss$/,
                use: [
                    {
                        loader: MiniCssExtractPlugin.loader
                    },
                    {
                        loader: 'css-loader'
                    },
                    {
                        loader: 'sass-loader',
                        options: {
                            implementation: require('sass')
                        }
                    }
                ]
            },
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
            CodeMirror: 'codemirror',
            'window.Quill': 'quill',     // because of quill-image-resize-module
            'Quill': 'quill'
        }),
        new MiniCssExtractPlugin({
            moduleFilename: (chunk) => `${chunk.name.split('_')[0]}/${chunk.name.split('_')[1]}.css`
            //filename: '[name].css',
            //chunkFilename: '[id].css'
        })
    ]
};