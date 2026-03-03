/* eslint-disable */
const webpack = require('webpack');
const { resolve } = require('path');
const TsconfigPathsPlugin = require('tsconfig-paths-webpack-plugin');
const TerserPlugin = require('terser-webpack-plugin');

const NODE_ENV = process.env.NODE_ENV && process.env.NODE_ENV.toLowerCase();

const PRODUCTION = ['release', 'production'].includes(NODE_ENV);

module.exports = {
  mode: PRODUCTION ? 'production' : 'development',
  devtool: PRODUCTION ? 'source-map' : 'eval-source-map',

  entry: {
    'app': resolve(__dirname, 'Scripts/Quantumart/App.js'),
    'app-logon': resolve(__dirname, 'Scripts/Quantumart/AppLogon.js'),
  },

  output: {
    path: resolve(__dirname, 'Scripts/build'),
    filename: '[name].js',
    clean: false  // gulp manages cleanup
  },

  module: {
    rules: [{
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
    extensions: ['.js', '.jsx'],
    preferRelative: true,  // Allow imports like 'Utils' to resolve to './Utils'

    plugins: [
      new TsconfigPathsPlugin({
        configFile: resolve(__dirname, 'tsconfig.json')
      })
    ]
  },

  optimization: {
    minimize: PRODUCTION,
    minimizer: [
      new TerserPlugin({
        terserOptions: {
          compress: {
            sequences: false
          },
          sourceMap: true
        }
      })
    ]
  },

  plugins: []
};
