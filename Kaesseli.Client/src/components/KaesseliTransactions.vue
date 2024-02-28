<template>
  <div class="q-pa-md">
    <q-btn-dropdown :label="`${currentLabel}`" v-model="show">
      <q-list style="min-width: 100px">
        <q-item v-for="(item, index) in summaries" :key="index" clickable v-ripple @click="select(item)">
          <q-item-section avatar>
            <q-avatar icon="receipt_long" />
          </q-item-section>
          <q-item-section>
            <q-item-label>{{ item.accountName }}</q-item-label>
            <q-item-label caption>{{ formatDate(item.valueDateFrom) }} - {{ formatDate(item.valueDateTo) }}, {{ item.nrOfTransactions }} Transaktionen</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-btn-dropdown>
  </div>
  <div>
    <div v-if="current">
      <KaesseliTransactionTable :summaryId="current.id"></KaesseliTransactionTable>
    </div>
    <div v-else>
      <p>Bitte wählen Sie einen Kontoauszug aus.</p>
    </div>
  </div>
</template>
<script lang="ts">
  import { defineComponent, Ref, ref, onMounted } from 'vue';
  import KaesseliTransactionTable from './KaesseliTransactionTable.vue'; 
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
    components: {
      // Registrieren Sie die Komponente
      KaesseliTransactionTable,
    },
    setup() {
      const summaries = ref<ITransactionSummary[]>([]);
      const show = ref(false);
      const currentLabel: Ref<string> = ref("Kontoauszug wählen");
      const current: Ref<ITransactionSummary | null> = ref(null);;
      const FetchSummaries = async () => {
        try {
          const response = await axios.get('https://localhost:7123/transactionSummary');
          summaries.value = response.data;
        } catch (error) {
          console.error('There was an error fetching the accounts:', error);
        }
      };
      function select(option: ITransactionSummary) {
        currentLabel.value = `${option.accountName} ${formatDate(option.valueDateFrom)} - ${formatDate(option.valueDateTo)}`; // Speichert die aktuelle Auswahl
        current.value = option;
        show.value = false; // Schließt das Menü
      }
      const formatDate = (dateStr: Date) => {
        const date = new Date(dateStr);
        return new Intl.DateTimeFormat(navigator.language).format(date);
      };
      onMounted(() => {
        FetchSummaries();
      });
      return {
        summaries,
        formatDate,
        currentLabel,
        current,
        select,
        show
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
