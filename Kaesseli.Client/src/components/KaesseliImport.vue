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
  import { useQuasar } from 'quasar';
  import { onMounted, ref, Ref } from 'vue';

  export default {
    setup() {

      interface IAccount {
        id: string;
        name: string;
        type: string;
        typeId: number;
        icon: string;
        iconColor: string;
      }


      const uploadedFile: Ref<Blob | null> = ref(null);
      const selectedAccount: Ref<string | null> = ref(null);
      const accounts: Ref<IAccount[] | null> = ref(null);
      const $q = useQuasar();

      const fetchAccounts = async () => {
        try {
          const response = await axios.get('https://localhost:7123/account?accountType=1');
          accounts.value = response.data;
        } catch (error) {
          $q.notify({
            type: 'negative',
            message: 'There was an error fetching the accounts',
            caption: error instanceof Error ? error.message : String(error)
          });
        }
      };
      const onFileChange = () => {
        console.log('Datei ausgewählt:', uploadedFile);
      };
      const onAccountSelect = (value: IAccount) => {
        console.log('Ausgewähltes Konto:', value);
      }
      const uploadFile = async () => {
        if (!uploadedFile.value || !selectedAccount.value) {
          alert('Bitte wählen Sie eine Datei und ein Konto aus.');
          return;
        };

        let formData = new FormData();
        const selectedPeriod: string | null = localStorage.getItem('selectedPeriod');
        if (selectedPeriod == null) return;
        formData.append('file', uploadedFile.value);
        formData.append('accountId', selectedAccount.value);
        formData.append('accountingPeriodId', selectedPeriod);

        try {
          await axios.post('https://localhost:7123/file/upload', formData, {
            headers: {
              'Content-Type': 'multipart/form-data'
            }
          });
          alert('Datei erfolgreich hochgeladen!');
        } catch (error) {
          $q.notify({
            type: 'negative',
            message: 'Error uploading file',
            caption: error instanceof Error ? error.message : String(error)
          });
        }
      };

      onMounted(() => {
        fetchAccounts();
      });

      return {
        onMounted,
        fetchAccounts,
        onFileChange,
        onAccountSelect,
        uploadFile,
        uploadedFile,
        accounts,
        selectedAccount
      }

    }
  };
</script>
