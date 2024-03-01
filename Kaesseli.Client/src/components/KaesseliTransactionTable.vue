<template>
  <div class="row">
    <q-table class="col-md-8 col-sm-12" :rows="transactions"
             :columns="columns"
             :hide-pagination="true"
             dense
             :rows-per-page-options="[0]"
             row-key="id"
             wrap-cells="false" />
  </div>
</template>

<script lang="ts">
  import { defineComponent, ref, watch, onMounted } from 'vue';
  import axios from 'axios';

  interface ITransaction {
    id: string,
    rawText: string,
    amount: number,
    valueDate: Date,
    bookDate: Date,
    description: string,
    reference: string,
    transactionCode: string,
    transactionCodeDetail: string
  }

  export default defineComponent({
    props: {
      summaryId: {
        type: String,
        required: true,
      },
    },
    setup(props) {
      const transactions = ref<ITransaction[]>([]);
      const columns = ref([
        { name: 'valueDate', required: true, label: 'Datum', align: 'left', field: (row: ITransaction) => formatDate(row.valueDate), sortable: true },
        { name: 'description', required: true, label: 'Beschreibung', align: 'left', field: (row: ITransaction) => row.description, sortable: true},
        { name: 'amount', required: true, label: 'Betrag', align: 'right', field: (row: ITransaction) => row.amount.toFixed(2), sortable: true },
      ]);

      const FetchSummaries = async (summaryId: string) => {
        try {
          const response = await axios.get(`https://localhost:7123/transaction?transactionSummaryId=${summaryId}`);
          transactions.value = response.data;
        } catch (error) {
          console.error('There was an error fetching the transactions:', error);
        }
      };
      const formatDate = (dateStr: Date) => {
        const date = new Date(dateStr);
        return new Intl.DateTimeFormat(navigator.language, {
          year: 'numeric',
          month: '2-digit',
          day: '2-digit'
        }).format(date);
      };
      watch(() => props.summaryId, (newVal) => {
        if (newVal) {
          FetchSummaries(newVal);
        }
      }, { immediate: true }); 
      return {
        columns,
        transactions,
      };
    },
  });
</script>

<style>
  .q-table {
    width: 100%;
    max-width: 100%; /* Stellt sicher, dass die Tabelle nicht über den Container hinausgeht */
    overflow-x: auto; /* Ermöglicht vertikales Scrollen, falls notwendig */
  }
  
</style>
