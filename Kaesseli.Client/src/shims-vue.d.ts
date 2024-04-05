/* eslint-disable */

/// <reference types="vite/client" />

// Mocks all files ending in `.vue` showing them as plain Vue instances
declare module '*.vue' {
// ReSharper disable once UnusedLocalImport
  import  DefineComponent from 'vue';
// ReSharper disable once TS2709
  const component: DefineComponent<{}, {}, any>;
  export default component;
}
