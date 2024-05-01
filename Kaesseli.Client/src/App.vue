<template>
  <router-view />
</template>

<script lang="ts">
  import { defineComponent, onMounted } from 'vue';
  import { msalConfig } from './authConfig.js';
  import { PublicClientApplication } from "@azure/msal-browser";

  const msalInstance = new PublicClientApplication(msalConfig);

  export default defineComponent({
    name: 'App',
    setup() {
      onMounted(async () => {
        await msalInstance.handleRedirectPromise().then((tokenResponse) => {
          console.log(tokenResponse);
        }).catch((error) => {
          console.error(error);
        });

        const accounts = msalInstance.getAllAccounts();
        if (accounts.length === 0) {
          try {
            await msalInstance.loginRedirect({});
          } catch (err) {
            console.error(err);
          }
        } else {
          try {
            const silentResult = await msalInstance.acquireTokenSilent({
              account: accounts[0],
              scopes: ["User.Read"] // Ersetzen Sie dies durch die tatsächlichen Bereiche, die Sie benötigen
            });
            console.log(silentResult);
          } catch (err) {
            if (err instanceof msal.InteractionRequiredAuthError) {
              // fallback to interaction when silent call fails
              try {
                await msalInstance.loginRedirect({});
              } catch (err) {
                console.error(err);
              }
            } else {
              console.error(err);
            }
          }
        }
      });
    }
  });
</script>
