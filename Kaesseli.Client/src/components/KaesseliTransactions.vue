<template>
  <div class="row ">
    <q-card class="col-md-6 col-sm-12 account-type q-pa-md">
      <q-card-section class="text-h6">Transaktionen</q-card-section>
      <q-card-section>
        <q-list bordered class="rounded-borders">
          <q-item v-for="(item, index) in summaries" :key="index" clickable v-ripple>
            <q-item-section avatar>
              <q-avatar icon="receipt_long" />
            </q-item-section>
            <q-item-section>
              <q-item-label>{{ item.accountName }}</q-item-label>
              <q-item-label caption>{{ formatDate(item.valueDateFrom) }} - {{ formatDate(item.valueDateTo) }}</q-item-label>
            </q-item-section>
          </q-item>
        </q-list>
      </q-card-section>

    </q-card>
  </div>
</template>

<script lang="ts">
  import { defineComponent, ref, onMounted } from 'vue';
  import axios from 'axios';

  interface ITransactionSummary {
    id: string,
    accountName: string,
    valueDateFrom: Date,
    valueDateTo: Date,
    balanceBefore: number,
    balanceAfter: number,
    reference: string,
    nrOfTransactions: number
  }

  export default defineComponent({
    name: 'KaesseliAccounts',
    setup() {
      const summaries = ref<ITransactionSummary[]>([]);


      const FetchSummaries = async () => {
        try {
          const response = await axios.get('https://localhost:7123/transactionSummary');
          summaries.value = response.data;
        } catch (error) {
          console.error('There was an error fetching the accounts:', error);
        }
      };

      const formatDate = (dateStr: string) => {
        const date = new Date(dateStr);
        return new Intl.DateTimeFormat(navigator.language).format(date);
      };

      onMounted(() => {
        FetchSummaries();
      });

      return {
        summaries,
        formatDate
      };
    },
  });
</script>

<style>
  .budgetBalance[data-fldval^='-'] {
    color: red;
    font-weight: bold;
  }

  .budgetBalance:not([data-fldval^='-']) {
    color: green;
    font-weight: bold;
  }
</style>
