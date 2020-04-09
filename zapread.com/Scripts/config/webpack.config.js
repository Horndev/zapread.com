const path = require("path");

module.exports = {
    entry: {
        index: "./Scripts/src/index.js",
        manage_apikeys: "./Scripts/src/manage/apikeys.js",
        home_index: "./Scripts/src/home/index.js"
    },
    output: {
        path: path.resolve(__dirname, "../dist"),
        filename: "[name].js"
    },
    module: {
        rules: [
            {
                test: /\.css$/,
                use: ['style-loader', 'css-loader']
            },
            {
                use: {
                    loader: "babel-loader"
                },
                test: /\.js$/,
                exclude: /node_modules/ //excludes node_modules folder from being transpiled by babel. We do this because it's a waste of resources to do so.
            }
        ]
    }
};