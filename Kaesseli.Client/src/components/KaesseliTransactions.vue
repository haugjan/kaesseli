<template>
  <div class="q-pa-md">
    <q-select v-model="selected"
              :options="summaries"
              emit-value
              map-options
              option-value="id"
              option-label="accountName"
              @update:model-value="select"
              :label="currentLabel"
              style="min-width: 300px"></q-select>
  </div>
  <div class="q-pa-md">
    <div v-if="current">
      <KaesseliTransactionTable :summaryId="current.id"></KaesseliTransactionTable>
    </div>
    <div v-else>
      <p>Bitte wählen Sie einen Kontoauszug aus.</p>
    </div>
  </div>
</template>

<script lang="ts">
  import { defineComponent, ref, onMounted } from 'vue';
  import KaesseliTransactionTable from './KaesseliTransactionTable.vue';
  import axios from 'axios';

  interface ITransactionSummary {
    id: string;
    accountName: string;
    valueDateFrom: Date;
    valueDateTo: Date;
    balanceBefore: number;
    balanceAfter: number;
    reference: string;
    nrOfTransactions: number;
  }

  export default defineComponent({
    name: 'KaesseliAccounts',
    components: {
      KaesseliTransactionTable,
    },
    setup() {
      const summaries = ref<ITransactionSummary[]>([]);
      const selected = ref<ITransactionSummary | null>(null);
      const current = ref<ITransactionSummary | null>(null);
      const currentLabel = ref("Kontoauszug wählen");

      const fetchSummaries = async () => {
        try {
          const response = await axios.get('https://localhost:7123/transactionSummary');
          summaries.value = response.data;
        } catch (error) {
          console.error('There was an error fetching the summaries:', error);
        }
      };

      const select = (id: string) => {
        const option = summaries.value.find(summary => summary.id === id);
        if (option) {
          currentLabel.value = `${option.accountName} ${formatDate(option.valueDateFrom)} - ${formatDate(option.valueDateTo)}`;
          current.value = option;
        }
      };

      const formatDate = (date: Date): string => {
        return new Intl.DateTimeFormat(navigator.language, {
          year: 'numeric',
          month: '2-digit',
          day: '2-digit'
        }).format(new Date(date));
      };

      onMounted(fetchSummaries);

      return {
        summaries,
        formatDate,
        currentLabel,
        current,
        select,
        selected
      };
    },
  });
</script>
