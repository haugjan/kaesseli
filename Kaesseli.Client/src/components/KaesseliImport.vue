<template>

  <!--TODO TypeScript-->
  <div class="q-pa-md">
    <q-file v-model="uploadedFile"
            label="Datei hochladen"
            filled
            use-chips
            @change="onFileChange" />

    <q-select v-model="selectedAccount"
              :options="accounts"
              option-value="id"
              option-label="name"
              label="Konto auswählen"
              filled
              emit-value
              map-options
              @input="onAccountSelect" />

    <q-btn label="Hochladen" color="primary" @click="uploadFile" />
  </div>
</template>

<script lang="ts">
  import axios from 'axios';
  import { useQuasar } from 'quasar'

  export default {
    data() {
      return {
        uploadedFile: null,
        selectedAccount: null,
        accounts: [],
      };
    },
    mounted() {
      this.fetchAccounts();
    },
    methods: {
      async fetchAccounts() {
        const $q = useQuasar();
        try {
          const response = await axios.get('https://localhost:7123/account?accountType=1');
          this.accounts = response.data;
        } catch (error) {
          $q.notify({
            type: 'negative',
            message: 'There was an error fetching the accounts',
            caption: error
          });
        }
      },
      onFileChange() {
        console.log('Datei ausgewählt:', this.uploadedFile);
      },
      onAccountSelect(value) {
        console.log('Ausgewähltes Konto:', value);
      },
      async uploadFile() {
        if (!this.uploadedFile || !this.selectedAccount) {
          alert('Bitte wählen Sie eine Datei und ein Konto aus.');
          return;
        }
        const $q = useQuasar();

        let formData = new FormData();
        formData.append('file', this.uploadedFile);
        formData.append('accountId', this.selectedAccount);

        try {
          await axios.post('https://localhost:7123/camt/upload', formData, {
            headers: {
              'Content-Type': 'multipart/form-data'
            }
          });
          alert('Datei erfolgreich hochgeladen!');
        } catch (error) {
          $q.notify({
            type: 'negative',
            message: 'Error uploading file',
            caption: error
          });
        }
      },
    },
  };
</script>
