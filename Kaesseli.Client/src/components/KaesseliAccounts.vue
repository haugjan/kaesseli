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
                   row-key="id">
            <template v-slot:body="props">
              <q-tr :props="props" @click="onRowClick(props.row)">
                <q-td v-for="col in props.cols" :key="col.name" :props="props">
                  {{ col.value }}
                </q-td>
              </q-tr>
            </template>
          </q-table>
        </q-card-section>

      </q-card>
    </div>
  </div>
  <div class="row">
    <div v-if="current">
      <KaesseliAccountTable :accountId="current.id" />
    </div>
    <div v-else>
      nix
    </div>

  </div>
</template>

<script lang="ts">
  import { defineComponent, ref, Ref, onMounted } from 'vue';
  import KaesseliAccountTable from './KaesseliAccountTable.vue';
  import axios from 'axios';

  interface IAccount {
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
    components: {
      KaesseliAccountTable
    },
    methods: {
      onRowClick(row: IAccount) {
        this.current = row;
      }
    },
    setup() {
      const accounts = ref<IAccount[]>([]);
      const accountTypes = ref<string[]>(['Einkommen', 'Ausgaben', 'Aktiv', 'Passiv']);
      const current: Ref<IAccount | null> = ref(null);;

      const columns = ref([
        { name: 'name', required: true, label: 'Name', align: 'left', field: (row: IAccount) => row.name, sortable: true },
        { name: 'accountBalance', label: 'Kontostand', align: 'right', field: (row: IAccount) => row.accountBalance, sortable: true, format: (val: number) => `${val.toFixed(2)}` },
        { name: 'budget', label: 'Budget', align: 'right', field: (row: IAccount) => row.budget, sortable: true, format: (val: number) => `${val.toFixed(2)}` },
        { name: 'budgetBalance', label: 'Budgetsaldo', align: 'right', field: (row: IAccount) => row.budgetBalance, sortable: true, format: (val: number) => `${val.toFixed(2)}`, classes: 'budgetBalance' },
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
        current
      };
    }
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
