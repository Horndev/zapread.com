const webpack = require("webpack");
const path = require("path");
const glob = require('glob-all');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const PurgeCSSPlugin = require('purgecss-webpack-plugin');
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");

module.exports = {
  devtool: "source-map",
  mode: "production",//production",//"development",
  entry: {
    account_login: "./Scripts/src/pages/account/login.js",
    account_register: "./Scripts/src/pages/account/register.js",
    admin_achievements: "./Scripts/src/pages/admin/achievements.js",
    admin_accounting: "./Scripts/src/pages/admin/accounting.js",
    admin_audit:    "./Scripts/src/pages/admin/audit.js",
    admin_icons:    "./Scripts/src/pages/admin/icons.js",
    admin_index:    "./Scripts/src/pages/admin/index.js",
    admin_jobs:     "./Scripts/src/pages/admin/jobs.js",
    admin_lightning:"./Scripts/src/pages/admin/lightning.js",
    admin_users:    "./Scripts/src/pages/admin/users.js",
    group_detail:   "./Scripts/src/pages/group/detail.js",
    group_edit:     "./Scripts/src/pages/group/edit.js",
    group_index:    "./Scripts/src/pages/group/index.js",
    group_members:  "./Scripts/src/pages/group/members.js",
    group_new:      "./Scripts/src/pages/group/new.js",
    home_about:     "./Scripts/src/pages/home/about.js",
    home_faq:       "./Scripts/src/pages/home/faq.js",
    home_index:     "./Scripts/src/pages/home/index.js",
    home_install:   "./Scripts/src/pages/home/install.js",
    lnauth_login:   "./Scripts/src/pages/lnauth/login.js",
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
    post_postnotfound: "./Scripts/src/pages/post/postnotfound.js",
    subscription_unsubscribe: "./Scripts/src/pages/subscription/unsubscribe.js",
    user_achievements: "./Scripts/src/pages/user/achievements.js",
    user_index:     "./Scripts/src/pages/user/index.js"
  },
  output: {
    path: path.resolve(__dirname, "../dist"),
    filename: c => {
      var subpath = c.chunk.name.split("_")[0];
      var filename = c.chunk.name.split("_")[1];
      return subpath + "/" + filename + ".js";
    },
    chunkFilename: c => {
      try {
        var subpath = "0";//c.chunk.runtime.split("_")[0];
        return subpath + "/" + c.chunk.id + ".js?v=[chunkhash]";
      }
      catch {
        console.log(c);
      }
      return "[id].js";
    }
  },
  optimization: {
    minimize: true,
    minimizer: [
      new TerserPlugin({
        test: /\.js(\?.*)?$/i,
      }),
      // For webpack@5 you can use the `...` syntax to extend existing minimizers (i.e. `terser-webpack-plugin`), uncomment the next line
      // `...`,
      new CssMinimizerPlugin(
        //{
        //test: /\index.css$/i,
        //}
      ),
    ],
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
            loader: "css-loader"
          },
          {
            loader: "sass-loader",
            options: {
              implementation: require("sass")
            }
          }
        ]
      },
      {
        use: {
          loader: "babel-loader",
          options: { cacheDirectory: true }
        },
        test: /\.js$/,
        exclude: /node_modules/ //excludes node_modules folder from being transpiled by babel. We do this because it's a waste of resources to do so.
      },
      {
        test: /\.(png|jpe?g|gif)$/,
        use: [
          {
            loader: "file-loader",
            options: {
              name: "[name].[ext]",
              outputPath: "../../Content/images",
              publicPath: "/Content/images"
            }
          }
        ]
      },
      {
        test: /\.svg$/,
        use: [
          {
            loader: "raw-loader"
          }
        ]
      },
      {
        test: /\.(woff|woff2|ttf|otf|eot)$/,
        type: 'asset/resource',
        generator: {
          filename: "[name][ext][query]",
          outputPath: "../../Content/fonts",
          publicPath: "../../../Content/fonts/"
        }
      },
      {
        test: require.resolve("jquery"),
        loader: "expose-loader",
        options: {
          exposes: ["$", "jQuery"]
        }
      }
    ]
  },
  plugins: [
    new webpack.ProvidePlugin({
      $: "jquery",
      jQuery: "jquery",
      "window.Quill": "quill", // because of quill-image-resize-module
      Quill: "quill"
    }),
    new MiniCssExtractPlugin({
      filename: c => {
        var subpath = c.chunk.name.split("_")[0];
        var filename = c.chunk.name.split("_")[1];
        return subpath + "/" + filename + ".css";
      },
      chunkFilename: c => {
        try {
          var subpath = "0";//c.chunk.runtime.split("_")[0];
          return subpath + "/" + c.chunk.id + ".css?v=[chunkhash]";
        }
        catch {
          console.log(c);
        }
        return "[id].css";
      }
    }),
    //new PurgeCSSPlugin({
    //  paths: glob.sync([
    //    './Scripts/**',
    //    './Views/**'
    //  ], { nodir: true })
    //})
  ]
};
