<template>
  <router-view />
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  import { msalConfig } from './authConfig.js';
  import { PublicClientApplication } from "@azure/msal-browser";

  const msalInstance = new PublicClientApplication(msalConfig);
  await msalInstance.initialize();


  msalInstance.handleRedirectPromise().then((tokenResponse) => {
    console.log(tokenResponse);

  }).catch((error) => {
    // handle error, either in the library or coming back from the server
  });

  try {
    msalInstance.loginRedirect({});
  } catch (err) {
    // handle error
  }

  export default defineComponent({
    name: 'App'
  });
</script>
