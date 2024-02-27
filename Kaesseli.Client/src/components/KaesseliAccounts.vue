<template>
  <div class="row ">
    <div v-for="type in accountTypes" :key="type" class="col-md-6 col-sm-12 account-type q-pa-md">
      <q-card>
        <q-card-section class="text-h6">{{ type }}</q-card-section>
        <q-card-section>
          <q-table :rows="filteredAccounts(type)"
                   :columns="columns"
                  :hide-pagination="true"
                   :rows-per-page-options="[0]"
                   row-key="id" />
        </q-card-section>

      </q-card>
    </div>
  </div>
</template>

<script lang="ts">
  import { defineComponent, ref, onMounted } from 'vue';
  import axios from 'axios';

  interface Account {
    id: string;
    name: string;
    type: string;
    typeId: number;
    accountBalance: number;
    budget: number;
    budgetBalance: number;
  }

  export default defineComponent({
    name: 'KaesseliAccounts',
    setup() {
      const accounts = ref<Account[]>([]);
      const accountTypes = ref<string[]>(['Einkommen', 'Ausgaben', 'Aktiv', 'Passiv']);
      const columns = ref([
        { name: 'name', required: true, label: 'Name', align: 'left', field: (row: Account) => row.name, sortable: true },
        { name: 'accountBalance', label: 'Kontostand', align: 'right', field: (row: Account) => row.accountBalance, sortable: true, format: (val: number) => `${val.toFixed(2)}` },
        { name: 'budget', label: 'Budget', align: 'right', field: (row: Account) => row.budget, sortable: true, format: (val: number) => `${val.toFixed(2)}` },
        { name: 'budgetBalance', label: 'Budgetsaldo', align: 'right', field: (row: Account) => row.budgetBalance, sortable: true, format: (val: number) => `${val.toFixed(2)}`, classes: 'budgetBalance' },
      ]);

      const fetchAccounts = async () => {
        try {
          const response = await axios.get('https://localhost:7123/accountSummary');
          accounts.value = response.data;
        } catch (error) {
          console.error('There was an error fetching the accounts:', error);
        }
      };

    

      const filteredAccounts = (type: string) => {
        return accounts.value.filter(account => account.type === type);
      };

      onMounted(() => {
        fetchAccounts();
      });

      return {
        accounts,
        accountTypes,
        columns,
        filteredAccounts,
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
