/* eslint-disable */
const webpack = require('webpack');
const { resolve } = require('path');

const NODE_ENV = process.env.NODE_ENV && process.env.NODE_ENV.toLowerCase();

const PRODUCTION = ['relesase', 'production'].includes(NODE_ENV);

module.exports = {
  devtool: PRODUCTION ? 'source-map' : 'eval-source-map',
  entry: {
    'app': resolve(__dirname, 'Scripts/Quantumart/App.js'),
    'app-logon': resolve(__dirname, 'Scripts/Quantumart/AppLogon.js'),
  },
  output: {
    path: resolve(__dirname, 'Scripts/build'),
    filename: '[name].js',
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
    extensions: ['.js', '.jsx']
  },
  plugins: compact([
    PRODUCTION && new webpack.optimize.UglifyJsPlugin({
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
