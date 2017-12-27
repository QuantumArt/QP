/* eslint-disable */
const webpack = require('webpack');
const { resolve } = require('path');

const NODE_ENV = process.env.NODE_ENV && process.env.NODE_ENV.toLowerCase();

const RELEASE = ['relesase', 'production'].includes(NODE_ENV);

const DEBUG = !RELEASE;

module.exports = {
  devtool: 'source-map',
  entry: resolve(__dirname, 'Scripts/Quantumart/App.js'),
  output: {
    path: resolve(__dirname, 'Scripts/build'),
    filename: 'app.js',
  },
  module: {
    rules: [/*{
      test: /\.(js|jsx)?$/,
      exclude: /node_modules/,
      loader: 'babel-loader',
      options: {
        cacheDirectory: true
      }
    },*/ {
      test: /\.(js|jsx)?$/,
      exclude: /node_modules/,
      loader: 'ts-loader',
      options: {
        configFile: resolve(__dirname, 'tsconfig.json'),
        transpileOnly: true
      }
    }]
  },
  resolve: {
    extensions: ['.js', '.jsx']
  },
  plugins: compact([
    RELEASE && new webpack.optimize.UglifyJsPlugin({
      sourceMap: true,
      uglifyOptions: {
        compress: {
          sequences: false
        }
      }
    })
  ])
};

function compact(array) {
  return array.filter(x => !!x);
}
