<template>
  <q-layout view="hHh lpR fFf">
    <q-header elevated class="bg-primary text-white">
      <q-toolbar>
        <q-toolbar-title>
          Kaesseli
        </q-toolbar-title>
      </q-toolbar>
    </q-header>

    <q-page-container>
      <q-tabs v-model="tab" inline-label
        outside-arrows
        mobile-arrows align="justify" class="bg-grey-8 text-white">
        <q-tab name="konten" icon="account_balance" label="Konten" />
        <q-tab name="transaktionen" icon="receipt_long" label="Transaktionen" />
      </q-tabs>

      <q-tab-panels v-model="tab" animated>
        <q-tab-panel name="konten">
          <KaesseliAccounts />
        </q-tab-panel>
        <q-tab-panel name="transaktionen">
          <KaesseliTransactions />
        </q-tab-panel>
      </q-tab-panels>
    </q-page-container>
  </q-layout>
</template>

<script>
  import KaesseliAccounts from '../components/KaesseliAccounts.vue';
  import KaesseliTransactions from '../components/KaesseliTransactions.vue';

  export default {
    components: {
      KaesseliAccounts,
      KaesseliTransactions
    },
    data() {
      return {
        tab: 'konten', // Standardmäßig ausgewählter Tab
      };
    },
  };
</script>
